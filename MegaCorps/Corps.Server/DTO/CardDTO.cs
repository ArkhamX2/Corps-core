using MegaCorps.Core.Model.Enums;
using Newtonsoft.Json;

namespace Corps.Server.DTO
{

    public class CardDTO
    {
        public int Id { get; set; }
        public int BackgroundImageId { get; set; }
        public int IconImageId { get; set; }
        public required string Type { get; set; }
        public required string Background { get; set; }
        public required string Icon { get; set; }
        public required CardInfoDTO Info { get; set; }
    }

    public class CardInfoDTO
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public string? AttackTypes { get; set; }
        public string? Direction { get; set; }
        public int? Power { get; set; }
    }

}
