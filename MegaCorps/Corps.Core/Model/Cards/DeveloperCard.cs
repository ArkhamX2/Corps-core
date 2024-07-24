namespace MegaCorps.Core.Model.Cards
{
    /// <summary>
    /// Класс карты разработчика
    /// </summary>
    public class DeveloperCard : GameCard
    {
        /// <summary>
        /// Очки развития, которые приносит карта
        /// </summary>
        public int DevelopmentPoint { get; set; }
        public DeveloperCard(int id, int devPoint) : base(id)
        {
            DevelopmentPoint = devPoint;
        }

        public override DeveloperCard Copy()
        {
            return (DeveloperCard)Clone();
        }

        public DeveloperCard(DeveloperCard card) : this(card.Id, card.DevelopmentPoint) { }
    }
}
