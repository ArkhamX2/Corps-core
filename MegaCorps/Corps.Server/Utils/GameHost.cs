namespace Corps.Server.Utils
{
    public class GameHost
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public GameHost(string name, string password)
        {
            Name = name;
            Password = password;
        }

    }
}
