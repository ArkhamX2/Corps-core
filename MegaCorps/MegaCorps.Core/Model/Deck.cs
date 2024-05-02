using MegaCorps.Core.Model.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model
{
    /// <summary>
    /// Класс колоды
    /// </summary>
    public class Deck
    {
        private List<GameCard> playedCards;
        private List<GameCard> unplayedCards;
        /// <summary>
        /// Сброс карт
        /// </summary>
        public List<GameCard> PlayedCards { get => playedCards; set => playedCards = value; }
        /// <summary>
        /// Колода карт
        /// </summary>
        public List<GameCard> UnplayedCards { get => unplayedCards; set => unplayedCards = value; }

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

        /// <summary>
        /// Перемешать колоду
        /// </summary>
        public void Shuffle()
        {
            var r = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));
            UnplayedCards = UnplayedCards.OrderBy(x => r.Value.Next(UnplayedCards.Count-1)).ToList();
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
                if(UnplayedCards.Count < dealCount)
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
    }
}
