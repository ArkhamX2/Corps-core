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

        /// <summary>
        /// Перемешать колоду
        /// </summary>
        public void Shuffle()
        {
            var r = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));
            //Random r = new Random();
            //Guid guid = Guid.NewGuid();
            for (int n = UnplayedCards.Count - 1; n > 0; --n)
            {
                int k = r.Value.Next(n + 1);

                //int k = r.Next(n + 1);
                //int k = RandomHelper.Next(n + 1);
                GameCard temp = UnplayedCards[n];
                UnplayedCards[n] = UnplayedCards[k];
                UnplayedCards[k] = temp;
            }
        }

        /// <summary>
        /// Раздать карты игрокам
        /// </summary>
        /// <param name="dealCount">Количество карт, которые необходимо раздать</param>
        /// <param name="playersCount">Количество игроков, которым необходимо раздать карты</param>
        /// <returns></returns>
        public List<List<GameCard>> Deal(int dealCount, int playersCount)
        {
            if(UnplayedCards.Count <= dealCount * playersCount)
            {
                foreach (GameCard card in PlayedCards)
                {
                    card.State = Enums.CardState.Unused;
                }
                UnplayedCards = new List<GameCard>(PlayedCards);
                PlayedCards = new List<GameCard>();
            }
            List<List<GameCard>> hands = Enumerable.Range(0, playersCount).Select(i => new List<GameCard>()).ToList();

            int counter = 0;
            while (counter < dealCount*playersCount)
            {
                foreach (var player in hands)
                {
                    if (UnplayedCards.Count == 0)
                    {
                        return new List<List<GameCard>>();
                    }
                    player.Add(UnplayedCards[UnplayedCards.Count - 1]);
                    UnplayedCards.RemoveAt(UnplayedCards.Count - 1);
                    if (UnplayedCards.Count == 0)
                        break;
                    counter++;
                }
            }
            return hands;
        }


        public override string ToString()
        {
            string ans = "";
            ans += " Played: ";
            foreach (GameCard item in PlayedCards)
            {
                ans += item.ToString() + " ";
            }
            ans += "|| Unplayed: ";
            foreach (GameCard item in UnplayedCards)
            {
                ans += item.ToString() + " ";
            }
            return ans;
        }
    }
}
