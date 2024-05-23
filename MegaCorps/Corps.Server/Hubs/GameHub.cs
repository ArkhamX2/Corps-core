using MegaCorps.Core.Model;
using Microsoft.AspNetCore.SignalR;


namespace Corps.Server.Hubs
{
    public class GameHub : Hub
    {
        Dictionary<int, Lobby> _lobbies;
        Dictionary<int, GameEngine> _games = new Dictionary<int, GameEngine>();


        //TODO: Навесить авторизацию
        public async Task CreateLobby()
        {
            _lobbies[_lobbies.Count] = new Lobby();
            await Groups.AddToGroupAsync(Context.ConnectionId, (_lobbies.Count() - 1) + "Host");
            await Clients.Group((_lobbies.Count() - 1) + "Host").SendAsync("CreateSuccess", _lobbies[_lobbies.Count - 1]);
        }

        public async Task JoinLobby(int lobbyId, string username)
        {
            _lobbies[lobbyId].PlayerList.Add(new LobbyMember(username));
            await Groups.AddToGroupAsync(Context.ConnectionId, (_lobbies[lobbyId] + "Player"));
            await Clients.Group((_lobbies[lobbyId] + "Player")).SendAsync("JoinSuccess", _lobbies[lobbyId]);
            await Clients.Group((_lobbies[lobbyId] + "Host")).SendAsync("PlayerJoined", _lobbies[lobbyId]);
        }
        public async Task LobbyMemberReady(int lobbyId, string username)
        {
            _lobbies[lobbyId].PlayerList.Where(x => x.Username == username).First().IsReady = true;
            await Clients.Group((_lobbies[lobbyId] + "Player")).SendAsync("ReadySuccess", _lobbies[lobbyId]);
            await Clients.Group((_lobbies[lobbyId] + "Host")).SendAsync("LobbyMemberReady", _lobbies[lobbyId]);
        }


        //TODO: Навесить авторизацию
        public async Task StartGame(int lobbyId)
        {
            _lobbies[lobbyId].State = LobbyState.Started;
            _games[lobbyId] = new GameEngine(_lobbies[lobbyId].PlayerList.Select(x => x.Username).ToList());
            _games[lobbyId].Deal(6);
            foreach(Player player in _games[lobbyId].Players)
            {
                await Clients.Group((_lobbies[lobbyId] + "Player")).SendAsync("GameStarted", player.Hand);
            }
            await Clients.Group((_lobbies[lobbyId] + "Host")).SendAsync("PlayerJoined", _games[lobbyId]);
        }

        public async Task SelectCard(int lobbyId, int playerId, int selectedCardId)
        {
            _games[lobbyId].Players[playerId].Hand.Cards.Where(x => x.Id == selectedCardId).First().State = MegaCorps.Core.Model.Enums.CardState.Used;

            await Clients.Group((_lobbies[lobbyId] + "Host")).SendAsync("CardSelected", playerId, selectedCardId);
        }

        public async Task PlayerReady(int lobbyId, int playerId)
        {
            _games[lobbyId].Players[playerId].IsReady = true;

            if (_games[lobbyId].Players.All(x => x.IsReady))
            {
                await Clients.Group(_lobbies[lobbyId] + "Host").SendAsync("GamePlayerIsReady", playerId);
                _games[lobbyId].Turn();
                if (_games[lobbyId].Win)
                {
                    await Clients.Group(_lobbies[lobbyId] + "Host").SendAsync("WinnerFound", _games[lobbyId].Winner);
                    await Clients.Group(_lobbies[lobbyId] + "Player").SendAsync("WinnerFound", _games[lobbyId].Winner);
                    return;
                }
                _games[lobbyId].Deal(3);
                await Clients.Group(_lobbies[lobbyId] + "Host").SendAsync("AllPlayerIsReady");
                await Clients.Group(_lobbies[lobbyId] + "Player").SendAsync("AllPlayerIsReady");
                return;
            }
            await Clients.Group(_lobbies[lobbyId] + "Host").SendAsync("GamePlayerIsReady", playerId);
        }

        public async Task GameChangesShown(int lobbyId)
        {
            foreach (Player player in _games[lobbyId].Players)
            {
                await Clients.Group((_lobbies[lobbyId] + "Player")).SendAsync("GameChangesShown", player.Hand);
            }
            await Clients.Group((_lobbies[lobbyId] + "Host")).SendAsync("GameChangesShown", _games[lobbyId]);
        }

    }
}
