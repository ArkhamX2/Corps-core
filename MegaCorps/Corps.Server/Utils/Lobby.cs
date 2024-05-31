using Corps.Server.CorpsException;
using MegaCorps.Core.Model;
using System.Security.Cryptography;
using System.Text;

namespace Corps.Server.Hubs
{
    public class Lobby
    {
        public int Id { get; set; }
        public string Code{ get; set; }
        public static readonly int CODE_LENGTH = 3;
        public List<LobbyMember> lobbyMembers { get; private set; }
        
        public LobbyState State{ get; set; }
        public Lobby(int id) {
            Id = id;
            lobbyMembers = new List<LobbyMember>();
            State = LobbyState.Waiting;
            Code = GenerateUniqueSequence(id, CODE_LENGTH);
        }
        public Lobby(int id,string source):this(id)
        {
            Code = GenerateUniqueSequence(source + DateTime.Now,CODE_LENGTH);
        }

        public int Join(string username)
        {
            LobbyMember member = new LobbyMember(lobbyMembers.Count, username);
            lobbyMembers.Add(member);
            return member.Id;
        }

        public void Leave(int playerId)
        {
            lobbyMembers.RemoveAll(x => x.Id == playerId);
        }

        public void PlayerReadyChange(int playerId)
        {
            int foundPlayerIndex = lobbyMembers.FindIndex(x => x.Id == playerId);
            if (foundPlayerIndex != -1)
            {
                if(lobbyMembers[foundPlayerIndex].IsReady == true)
                {
                    lobbyMembers[foundPlayerIndex].IsReady = false;
                }
                else lobbyMembers[foundPlayerIndex].IsReady = true;
            }
            else
            {
                throw new PlayerNotFoundException();
            }
        }

        public static string GenerateUniqueSequence(object key,int length)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(key.ToString());
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < length; i++) 
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToUpper();
            }
        }
    }

    public class LobbyMember
    {
        public LobbyMember(int id,string username)
        {
            Id = id;
            Username = username;
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public bool IsReady { get; set; }
    }

    public enum LobbyState
    {
        Waiting,
        Started
    }
}
