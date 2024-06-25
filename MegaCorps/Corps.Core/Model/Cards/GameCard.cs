using MegaCorps.Core.Model.Enums;

namespace MegaCorps.Core.Model.Cards
{
    /// <summary>
    /// Базовый класс карты
    /// </summary>
    public class GameCard
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

        public override string ToString()
        {
            string ans = "";
            ans += $"{Id}:{State}";
            return ans;
        }
    }
}
