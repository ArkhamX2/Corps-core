using MegaCorps.Core.Model.Enums;

namespace MegaCorps.Core.Model.Cards
{
    /// <summary>
    /// Базовый класс карты
    /// </summary>
    public class GameCard : ICloneable
    {
        public int Id { get; set; }
        /// <summary>
        /// Состояние карты
        /// </summary>
        public CardState State { get; set; }
        public GameCard(int id)
        {
            Id = id;
            State = CardState.Unused;
        }

        public GameCard(GameCard card)
        {
            Id = card.Id;
            State = card.State;
        }
        public GameCard()
        {
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public virtual GameCard Copy()
        {
            return (GameCard)Clone();
        }
        public override string ToString()
        {
            string ans = "";
            ans += $"{Id}:{State}";
            return ans;
        }
    }
}
