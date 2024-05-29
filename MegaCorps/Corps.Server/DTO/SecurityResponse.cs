using Corps.Server.Utils;

namespace Corps.Server.DTO
{
    public class SecurityResponse
    {
        public required string host { get; set; }
        public required string Token {  get; set; }
    }
}
