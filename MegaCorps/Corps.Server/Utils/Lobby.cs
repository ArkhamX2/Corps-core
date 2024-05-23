using MegaCorps.Core.Model;

namespace Corps.Server.Hubs
{
    public class Lobby
    {
        public List<LobbyMember> PlayerList{ get; set; }
        public LobbyState State{ get; set; }
        public Lobby() {
            PlayerList = new List<LobbyMember>();
            State = LobbyState.Waiting;
        }
    }

    public class LobbyMember
    {
        public LobbyMember(string username)
        {
            Username = username;
        }

        public string Username { get; set; }
        public bool IsReady { get; set; }
    }

    public enum LobbyState
    {
        Waiting,
        Started
    }
}
