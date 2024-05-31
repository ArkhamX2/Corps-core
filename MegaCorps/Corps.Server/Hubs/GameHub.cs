﻿using Corps.Server.CorpsException;
using MegaCorps.Core.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Frozen;


namespace Corps.Server.Hubs
{
    /// <summary>
    /// Хаб лобби и игры
    /// </summary>
    public class GameHub : Hub
    {
        private static ConcurrentDictionary<int, Lobby> _lobbies = new ConcurrentDictionary<int, Lobby>();
        private static ConcurrentDictionary<int, GameEngine> _games = new ConcurrentDictionary<int, GameEngine>();


        /// <summary>
        /// Хост создаёт лобби
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task CreateLobby(string hostname)
        {
            //TODO:
            //TODO: Генерировать код лобби и отвязаться от просто индекса, заменив его уникальным кодом из 6 знаков (1 млн кодов)
            _lobbies[_lobbies.Count] = new Lobby(_lobbies.Count,hostname);
            Lobby created = _lobbies[_lobbies.Count - 1];
            while (_lobbies.Values.SkipLast(1).Any(x => x.Code == created.Code))
            {
                created.Code = Lobby.GenerateUniqueSequence(_lobbies.Count, Lobby.CODE_LENGTH);
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, created.Id + "Host");
            await Clients.Group(created.Id + "Host").SendAsync("CreateSuccess", created);
            Log_Lobby(nameof(CreateLobby));
        }


        /// <summary>
        /// Подключение к лобби.
        /// </summary>
        /// <param name="lobbyCode">идентификатор лобби</param>
        /// <param name="username">имя участника лобби</param>
        /// <returns></returns>
        public async Task JoinLobby(string lobbyCode, string username)
        {
            IEnumerable<Lobby> found = _lobbies.Values.Where(x => x.Code == lobbyCode);
            if (found.Count() > 0)
            {
                Lobby joinTo = found.First();
                int playerId = joinTo.Join(username);
                await Groups.AddToGroupAsync(Context.ConnectionId, joinTo.Id + "Player");
                await Clients.Group(joinTo.Id + "Player").SendAsync("JoinSuccess", joinTo,playerId);
                await Clients.Group(joinTo.Id + "Host").SendAsync("PlayerJoined", joinTo);
            }
            else
            {
                //TODO: Обработка ошибки отсутствия лобби с таким кодом
            }
            Log_Lobby(nameof(JoinLobby));
        }

        /// <summary>
        /// Участник лобби готов начать игру. Сообщаем об этом всем участникам лобби и хосту
        /// </summary>
        /// <param name="lobbyId">идентификатор лобби</param>
        /// <param name="username">имя участника лобби</param>
        /// <returns></returns>
        public async Task LobbyMemberReady(int lobbyId, int playerId)
        {
            //TODO: Обработка ошибки отсутствия лобби с таким идентификатором
            IEnumerable<Lobby> found = _lobbies.Values.Where(x => x.Id == lobbyId);
            if (found.Count() > 0)
            {
                Lobby joinTo = found.First();
                try
                {
                    joinTo.PlayerReady(playerId);
                }
                catch(PlayerNotFoundException e)
                {
                    //TODO: Обработка ошибки готовности игрока
                }
                await Clients.Group(lobbyId + "Player").SendAsync("ReadySuccess");
                await Clients.Group(lobbyId + "Host").SendAsync("LobbyMemberReady", _lobbies[lobbyId]);
            }
            else
            {
                //TODO: Обработка ошибки отсутствия лобби с таким кодом
            }
            Log_Lobby(nameof(LobbyMemberReady));
        }

        /// <summary>
        /// Хост даёт команду о начале игры. Инициализируем и отправляем клиентам соответствующие сообщения
        /// </summary>
        /// <param name="lobbyId">идентификатор лобби</param>
        /// <returns></returns>
        [Authorize]
        public async Task StartGame(int lobbyId)
        {
            //TODO: Обработка готовности всех игроков перед началом игры
            //TODO: Обработка ошибки отсутствия лобби с таким идентификатором
            foreach(var player in _lobbies[lobbyId].lobbyMembers)
            _lobbies[lobbyId].State = LobbyState.Started;
            _games[lobbyId] = new GameEngine(_lobbies[lobbyId].lobbyMembers.Select(x => x.Username).ToList());
            _games[lobbyId].Deal(6);
            foreach (Player player in _games[lobbyId].Players)
            {
                await Clients.Group(lobbyId + "Player").SendAsync($"GameStarted{player.Id}", player.Hand.Cards);
            }
            await Clients.Group(lobbyId + "Host").SendAsync("GameStarted", _games[lobbyId]);
            Log_Lobby(nameof(StartGame));
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
            if (foundCardIndex != -1)
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

            if(_lobbies.Values.Where(x=>x.Id==lobbyId).Count()>0)
            {
                foreach (Player player in _games[lobbyId].Players)
                {
                    await Clients.Group(lobbyId + "Player").SendAsync("GameChangesShown", player.Hand);
                }
                await Clients.Group(lobbyId + "Host").SendAsync("GameChangesShown", _games[lobbyId]);
            }
            else
            {
               await Clients.Caller.SendAsync($"lobby {lobbyId} doesnt exist ");
            }
        }


        //TODO: реконнект.
        //Есть вариант  всё пихнуть в метод JoinLobby и если у лобби статус Started перекидывать в игру.



        private static void Log_Lobby(string callerMethod)
        {
            Console.WriteLine(callerMethod);
            foreach (var item in _lobbies.Keys)
            {
                Console.WriteLine(item);
                if (_lobbies.TryGetValue(item, out Lobby lobby))
                {                   
                    foreach (LobbyMember member in lobby.lobbyMembers)
                    {
                        Console.Write(member.Username + " " + member.IsReady + "|");
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
