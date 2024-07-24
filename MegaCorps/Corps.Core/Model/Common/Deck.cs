using MegaCorps.Core.Model.Cards;

namespace MegaCorps.Core.Model
{
    /// <summary>
    /// Класс колоды
    /// </summary>
    public class Deck : ICloneable
    {
        /// <summary>
        /// Сброс карт
        /// </summary>
        public List<GameCard> PlayedCards { get; set; } = new();
        /// <summary>
        /// Колода карт
        /// </summary>
        public List<GameCard> UnplayedCards { get; set; } = new();

        public Deck(List<GameCard> cards)
        {
            UnplayedCards = cards;
            PlayedCards = new List<GameCard>();
        }

        public Deck(List<GameCard> unplayed, List<GameCard> played)
        {
            UnplayedCards = unplayed;
            PlayedCards = played;
        }

        public Deck() 
        {
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public Deck Copy()
        {
            Deck copy = new Deck();
            foreach (GameCard card in PlayedCards)
            {
                copy.PlayedCards.Add(card.Copy());
            }
            foreach (GameCard card in UnplayedCards)
            {
                copy.UnplayedCards.Add(card.Copy());
            }
            return copy;
        }

        /// <summary>
        /// Перемешать колоду
        /// </summary>
        public void Shuffle()
        {
            var r = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));
            UnplayedCards = UnplayedCards.OrderBy(x => r.Value!.Next(UnplayedCards.Count - 1)).ToList();
        }

        /// <summary>
        /// Раздать карты игрокам
        /// </summary>
        /// <param name="dealCount">Количество карт, которые необходимо раздать</param>
        /// <param name="playersCount">Количество игроков, которым необходимо раздать карты</param>
        /// <returns></returns>
        public List<List<GameCard>> Deal(int dealCount, int playersCount)
        {
            List<List<GameCard>> hands = new List<List<GameCard>>();

            for (int i = 0; i < playersCount; i++)
            {
                if (UnplayedCards.Count < dealCount)
                {
                    foreach (GameCard card in PlayedCards)
                    {
                        card.State = Enums.CardState.Unused;
                    }
                    UnplayedCards.AddRange(PlayedCards);
                    PlayedCards.Clear();
                    Shuffle();
                }
                List<GameCard> dealt = UnplayedCards.GetRange(0, dealCount);
                UnplayedCards.RemoveRange(0, dealCount);
                hands.Add(dealt);
            }
            return hands;
        }

        /// <summary>
        /// Раздать карты игрокам, исключая одного и его карты
        /// </summary>
        /// <param name="dealCount">Количество карт, которые необходимо раздать</param>
        /// <param name="playersCount">Количество игроков, которым необходимо раздать карты</param>
        /// <returns></returns>
        public List<List<GameCard>> DealExcept(int dealCount, int playersCount, int id, PlayerHand hand)
        {
            foreach (var card in hand.Cards)
            {
                UnplayedCards.Remove(UnplayedCards.Find((a) => a.Id == card.Id));
            }
            List<List<GameCard>> hands = new List<List<GameCard>>();

            for (int i = 0; i < playersCount; i++)
            {
                if (i == id)
                {
                    hands.Add(hand.Cards);
                }
                else
                {
                    if (UnplayedCards.Count < dealCount)
                    {
                        foreach (GameCard card in PlayedCards)
                        {
                            card.State = Enums.CardState.Unused;
                        }
                        UnplayedCards.AddRange(PlayedCards);
                        PlayedCards.Clear();
                        Shuffle();
                    }
                    List<GameCard> dealt = UnplayedCards.GetRange(0, dealCount);
                    UnplayedCards.RemoveRange(0, dealCount);
                    hands.Add(dealt);
                }
            }
            return hands;
        }

        public override string ToString()
        {
            string str = "{ ";
            str += "[ ";
            foreach (var card in PlayedCards)
            {
                if (card == PlayedCards.Last())
                    str += card.ToString() + " ";
                else
                    str += card.ToString() + ", ";
            }
            str += "],";
            str += "[ ";
            foreach (var card in UnplayedCards)
            {
                if (card == UnplayedCards.Last())
                    str += card.ToString() + " ";
                else
                    str += card.ToString() + ", ";
            }
            str += "] ";
            str += "}";
            return str;
        }
    }
}
