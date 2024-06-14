

using Corps.Server.Services;
using MegaCorps.Core.Model;
using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using MegaCorps.Core.Model.GameUtils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Corps.Server.Hubs
{
    /// <summary>
    /// Хаб лобби и игры
    /// </summary>
    public class GameHub(ILogger<GameHub> logger,ImageService imageService) : Hub
    {
        private static ConcurrentDictionary<int, Lobby> _lobbies = new ConcurrentDictionary<int, Lobby>();
        private static ConcurrentDictionary<int, GameEngine> _games = new ConcurrentDictionary<int, GameEngine>();
        private ILogger<GameHub> logger = logger;


        /// <summary>
        /// Хост создаёт лобби
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task CreateLobby(string hostname)
        {
            try
            {

                _lobbies[_lobbies.Count] = new Lobby(_lobbies.Count, hostname);
                Lobby created = _lobbies[_lobbies.Count - 1];
                while (_lobbies.Values.SkipLast(1).Any(x => x.Code == created.Code))
                {
                    created.Code = Lobby.GenerateUniqueSequence(_lobbies.Count, Lobby.CODE_LENGTH);
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, created.Id + "Host");
                await Clients.Group(created.Id + "Host").SendAsync("CreateSuccess", created);
                Log_Lobby(nameof(CreateLobby));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Clients.Caller.SendAsync("HandleException", "Произошла ошибка при создании лобби");
            }
        }


        /// <summary>
        /// Подключение к лобби.
        /// </summary>
        /// <param name="lobbyCode">идентификатор лобби</param>
        /// <param name="username">имя участника лобби</param>
        /// <returns></returns>
        public async Task JoinLobby(string lobbyCode, string username, int avatarId)
        {
            try
            {
                IEnumerable<Lobby> found = _lobbies.Values.Where(x => x.Code == lobbyCode);
                if (found.Count() < 0) throw new Exception("Не найдено лобби с таким идентификатором");

                Lobby joinTo = found.First();
                int playerId = joinTo.Join(username,avatarId);

                await Groups.AddToGroupAsync(Context.ConnectionId, joinTo.Id + "Player");
                await Clients.Group(joinTo.Id + "Player").SendAsync("JoinSuccess", joinTo, playerId);
                await Clients.Group(joinTo.Id + "Player").SendAsync("PlayerJoined", joinTo);
                await Clients.Group(joinTo.Id + "Host").SendAsync("PlayerJoined", joinTo);

                Log_Lobby(nameof(JoinLobby));
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                await Clients.Caller.SendAsync("HandleException", ex.Message);
            }
        }

        /// <summary>
        /// Участник лобби готов начать игру. Сообщаем об этом всем участникам лобби и хосту
        /// </summary>
        /// <param name="lobbyId">идентификатор лобби</param>
        /// <param name="username">имя участника лобби</param>
        /// <returns></returns>
        public async Task LobbyMemberReady(int lobbyId, int playerId)
        {
            try
            {
                if (!_lobbies.ContainsKey(lobbyId)) throw new Exception("Не найдено лобби с таким идентификатором");

                Lobby joinTo = _lobbies[lobbyId];
                joinTo.PlayerReadyChange(playerId);

                await Clients.Group(lobbyId + "Player").SendAsync("ReadySuccess");
                await Clients.Group(lobbyId + "Player").SendAsync("LobbyMemberReady", joinTo);
                await Clients.Group(lobbyId + "Host").SendAsync("LobbyMemberReady", _lobbies[lobbyId]);

                Log_Lobby(nameof(LobbyMemberReady));
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                await Clients.Caller.SendAsync("HandleException", ex.Message);
            }
        }

        /// <summary>
        /// Хост даёт команду о начале игры. Инициализируем и отправляем клиентам соответствующие сообщения
        /// </summary>
        /// <param name="lobbyId">идентификатор лобби</param>
        /// <returns></returns>
        [Authorize]
        public async Task StartGame(int lobbyId)
        {
            try
            {
                if (!_lobbies.ContainsKey(lobbyId)) throw new Exception("Не найдено лобби с таким идентификатором");

                Lobby lobby = _lobbies[lobbyId];

                if (lobby.lobbyMembers.Count < 2) throw new Exception("Недостаточно игроков для начала игры");
                if (!lobby.lobbyMembers.All(x => x.IsReady)) throw new Exception("Не все игроки готовы начать игру");

                foreach (var player in lobby.lobbyMembers)
                    lobby.State = LobbyState.Started;

                Deck deck = DeckBuilder.GetDeckFromResources(imageService.attackInfos, imageService.defenceInfos, imageService.developerInfos, imageService.directions, imageService.eventInfos);
                _games[lobbyId] = new GameEngine(deck,lobby.lobbyMembers.Select(x => x.Username).ToList());
                GameEngine game = _games[lobbyId];
                game.Deal(6);

                foreach (Player player in game.Players)
                    await Clients.Group(lobbyId + "Player").SendAsync($"GameStarted{player.Id}", player.Hand.Cards);
                await Clients.Group(lobbyId + "Host").SendAsync("GameStarted", game.Players);

                Log_Lobby(nameof(StartGame));
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                await Clients.Caller.SendAsync("HandleException", ex.Message);
            }
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
            try
            {
                if (!_games.ContainsKey(lobbyId)) throw new Exception("Не найдено лобби с таким идентификатором");
                GameEngine game = _games[lobbyId];
                if (!(playerId >= 0 && playerId < game.Players.Count)) throw new Exception("Не найден игрок с таким идентификатором");
                int foundCardIndex = game.Players[playerId].Hand.Cards.FindIndex(x => x.Id == selectedCardId);
                if (foundCardIndex == -1) throw new Exception("Не найдена карта с таким идентификатором");

                List<GameCard> Cards=new List<GameCard>();
                if(game.Players[playerId].Hand.Cards[foundCardIndex].State == CardState.Used)
                {
                    game.Players[playerId].Hand.Cards[foundCardIndex].State = CardState.Unused;
                    game.Players[playerId].Hand.SelectedCardQueue.Remove(game.Players[playerId].Hand.Cards.FirstOrDefault(card => card.Id == selectedCardId)!);
                    Cards.Add(game.Players[playerId].Hand.Cards[foundCardIndex]);
                }
                else
                {
                    game.Players[playerId].Hand.Cards[foundCardIndex].State=CardState.Used;
                    Cards.Add(game.Players[playerId].Hand.Cards[foundCardIndex]);
                    int unSelectedId = game.Players[playerId].Hand.PushCardToSelectedQueue(selectedCardId);
                    if (unSelectedId != -1)
                    {
                        var foundUnselectCardIndex = game.Players[playerId].Hand.Cards.FindIndex(x => x.Id == unSelectedId);
                        game.Players[playerId].Hand.Cards[foundUnselectCardIndex].State = CardState.Unused;
                        Cards.Add(game.Players[playerId].Hand.Cards[foundUnselectCardIndex]);
                    }
                }
                await Clients.Group(lobbyId + "Host").SendAsync("CardSelected", playerId, Cards);
                await Clients.Group(lobbyId + "Player").SendAsync($"CardSelected{playerId}", Cards);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                await Clients.Caller.SendAsync("HandleException", ex.Message);
            }
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
            try
            {
                if (!_games.ContainsKey(lobbyId)) throw new Exception("Не найдена игра с таким идентификатором");
                GameEngine game = _games[lobbyId];
                if (!(playerId >= 0 && playerId < game.Players.Count)) throw new Exception("Не найден игрок с таким идентификатором");
                game.Players[playerId].IsReady = !game.Players[playerId].IsReady;

                if (game.Players.All(x => x.IsReady))
                {
                    game.TargetCards();
                    await Clients.Group(lobbyId + "Host").SendAsync("AllPlayerReady", game.Players);
                    await Clients.Group(lobbyId + "Player").SendAsync("AllPlayerReady");
                }
                else await Clients.Group(lobbyId + "Host").SendAsync("GamePlayerIsReady", playerId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                await Clients.Caller.SendAsync("HandleException", ex.Message);
            }
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
            try
            {
                if (!_games.ContainsKey(lobbyId)) throw new Exception("Не найдена игра с таким идентификатором");
                GameEngine game = _games[lobbyId];
                game.Turn();
                if (game.Win)
                {
                    await Clients.Group(lobbyId + "Host").SendAsync("WinnerFound", game.Winner);
                    await Clients.Group(lobbyId + "Player").SendAsync("WinnerFound", game.Winner);
                    return;
                }
                game.Deal(3);

                foreach (Player player in game.Players)
                {
                    await Clients.Group(lobbyId + "Player").SendAsync("GameChangesShown", player.Hand.Cards);
                }
                await Clients.Group(lobbyId + "Host").SendAsync("GameChangesShown", game.Players);

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                await Clients.Caller.SendAsync("HandleException", ex.Message);
            }
        }

        //TODO: реконнект. Есть вариант  всё пихнуть в метод JoinLobby и если у лобби статус Started перекидывать в игру.
        //TODO: Логгирование ошибок. Вместо CW записывать в ILogger

        private static void Log_Lobby(string callerMethod)
        {
            Console.WriteLine(callerMethod);
            foreach (var item in _lobbies.Keys)
            {
                Console.WriteLine(item);
                if (_lobbies.TryGetValue(item, out Lobby lobby))
                {
                    Console.WriteLine(lobby.Code);
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
