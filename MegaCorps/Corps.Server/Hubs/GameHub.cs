using MegaCorps.Core.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


namespace Corps.Server.Hubs
{
    /// <summary>
    /// Хаб лобби и игры
    /// </summary>
    public class GameHub : Hub
    {
        Dictionary<int, Lobby> _lobbies = new Dictionary<int, Lobby>();
        Dictionary<int, GameEngine> _games = new Dictionary<int, GameEngine>();

        /// <summary>
        /// Хост создаёт лобби
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task CreateLobby()
        {
            //TODO: Генерировать код лобби и отвязаться от просто индекса, заменив его уникальным кодом из 6 знаков (1 млн кодов)
            _lobbies[_lobbies.Count] = new Lobby();
            await Groups.AddToGroupAsync(Context.ConnectionId, (_lobbies.Count() - 1) + "Host");
            await Clients.Group((_lobbies.Count() - 1) + "Host").SendAsync("CreateSuccess", _lobbies.Count - 1, _lobbies[_lobbies.Count - 1]);
        }

        /// <summary>
        /// Подключение к лобби.
        /// </summary>
        /// <param name="lobbyId">идентификатор лобби</param>
        /// <param name="username">имя участника лобби</param>
        /// <returns></returns>
        public async Task JoinLobby(int lobbyId, string username)
        {
            _lobbies[lobbyId].LobbyMemberList.Add(new LobbyMember(username));
            await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId + "Player");
            await Clients.Group(lobbyId + "Player").SendAsync("JoinSuccess");
            await Clients.Group(lobbyId + "Host").SendAsync("PlayerJoined", _lobbies[lobbyId]);
        }

        /// <summary>
        /// Участник лобби готов начать игру. Сообщаем об этом всем участникам лобби и хосту
        /// </summary>
        /// <param name="lobbyId">идентификатор лобби</param>
        /// <param name="username">имя участника лобби</param>
        /// <returns></returns>
        public async Task LobbyMemberReady(int lobbyId, string username)
        {
            //TODO: Обработка ошибки отсутствия лобби с таким идентификатором
            //TODO: Идентификация участника лобби не по никнейму, а по айди - добавить в LobbyMemberID и выдавать его при подключении
            int foundPlayerIndex = _lobbies[lobbyId].LobbyMemberList.FindIndex(x => x.Username == username);

            if(foundPlayerIndex != -1)
            {
                _lobbies[lobbyId].LobbyMemberList[foundPlayerIndex].IsReady = true;
            }
            else
            {
                //TODO: Обработка ошибки отсутствия участника лобб
            }
                
            await Clients.Group(lobbyId + "Player").SendAsync("ReadySuccess");
            await Clients.Group(lobbyId + "Host").SendAsync("LobbyMemberReady", _lobbies[lobbyId]);
        }

        /// <summary>
        /// Хост даёт команду о начале игры. Инициализируем и отправляем клиентам соответствующие сообщения
        /// </summary>
        /// <param name="lobbyId">идентификатор лобби</param>
        /// <returns></returns>
        [Authorize]
        public async Task StartGame(int lobbyId)
        {
            //TODO: Обработка ошибки отсутствия лобби с таким идентификатором
            _lobbies[lobbyId].State = LobbyState.Started;
            _games[lobbyId] = new GameEngine(_lobbies[lobbyId].LobbyMemberList.Select(x => x.Username).ToList());
            _games[lobbyId].Deal(6);
            foreach(Player player in _games[lobbyId].Players)
            {
                await Clients.Group(lobbyId + "Player").SendAsync("GameStarted", player.Hand);
            }
            await Clients.Group(lobbyId + "Host").SendAsync("GameStarted", _games[lobbyId]);
        }

        /// <summary>
        /// Игрок выбирает одну из своих карт. Сообщаем хосту о выборе игрока.
        /// </summary>
        /// <param name="lobbyId">идентификатор лобби</param>
        /// <param name="playerId">идентификатор игрока</param>
        /// <param name="selectedCardId">идентификатор выбранной карты</param>
        /// <returns></returns>
        public async Task SelectCard(int lobbyId, int playerId, int selectedCardId)
        {
            //TODO: Обработка ошибки отсутствия лобби, игрока и карты с такими идентификаторами
            int foundCardIndex = _games[lobbyId].Players[playerId].Hand.Cards.FindIndex(x => x.Id == selectedCardId);
            if(foundCardIndex != -1)
            {
                _games[lobbyId].Players[playerId].Hand.Cards[foundCardIndex].State = MegaCorps.Core.Model.Enums.CardState.Used;
            }
            else
            {
                //TODO: Обработка ошибки выбора карты
            }

            await Clients.Group(lobbyId + "Host").SendAsync("CardSelected", playerId, selectedCardId);
        }

        /// <summary>
        /// Игрок готов сделать ход. Сообщаем хосту о готовности игрока.
        /// Если в этот момент все игроки в лобби также готовы сделать ход,
        /// он проводится движком.
        /// Хост отрисовывает результаты хода
        /// Игроки в состоянии ожидания.
        /// </summary>
        /// <param name="lobbyId">идентификатор лобби</param>
        /// <param name="playerId">идентификатор игрока</param>
        /// <returns></returns>
        public async Task PlayerReady(int lobbyId, int playerId)
        {
            //TODO: Обработка ошибки отсутствия лобби и игрока с такими идентификаторами
            _games[lobbyId].Players[playerId].IsReady = true;

            if (_games[lobbyId].Players.All(x => x.IsReady))
            {
                await Clients.Group(lobbyId + "Host").SendAsync("GamePlayerIsReady", playerId);
                _games[lobbyId].Turn();
                if (_games[lobbyId].Win)
                {
                    await Clients.Group(lobbyId + "Host").SendAsync("WinnerFound", _games[lobbyId].Winner);
                    await Clients.Group(lobbyId + "Player").SendAsync("WinnerFound", _games[lobbyId].Winner);
                    return;
                }
                _games[lobbyId].Deal(3);
                await Clients.Group(lobbyId + "Host").SendAsync("AllPlayerIsReady");
                await Clients.Group(lobbyId + "Player").SendAsync("AllPlayerIsReady");
                return;
            }
            await Clients.Group(lobbyId + "Host").SendAsync("GamePlayerIsReady", playerId);
        }

        /// <summary>
        /// Хост сообщает об окончании отрисовки игрового процесса. 
        /// Игра продолжается. 
        /// Игрокам выдаются новые руки, хост обновляет состояние игры
        /// </summary>
        /// <param name="lobbyId">идентификатор лобби</param>
        /// <returns></returns>
        public async Task GameChangesShown(int lobbyId)
        {
            //TODO: Обработка ошибки отсутствия лобби с таким идентификатором
            //Выдаём новые карты игрокам
            foreach (Player player in _games[lobbyId].Players)
            {
                await Clients.Group(lobbyId + "Player").SendAsync("GameChangesShown", player.Hand);
            }
            await Clients.Group(lobbyId + "Host").SendAsync("GameChangesShown", _games[lobbyId]);
        }


        //TODO: реконнект.
        //Есть вариант  всё пихнуть в метод JoinLobby и если у лобби статус Started перекидывать в игру.
       
    }
}
