namespace Corps.Server.DTO
{
    public class SecurityRequest
    {
        public required string login {  get; set; }
        public string password { get; set; } = null!;
    }
}
