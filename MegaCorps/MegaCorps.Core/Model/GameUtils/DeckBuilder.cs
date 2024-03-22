using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model.GameUtils
{
    public static class DeckBuilder
    {
        private  const int MAX_DECK_SIZE = 72;
        private const int MAX_ATTACK_CARDS_COUNT = 40;
        private const int MAX_DEFENCE_CARDS_COUNT = 10;
        public static void UpdateDeck(List<GameCard> deck,int players)
        {
           
        }
        public static Deck GetDeck()
        {
            var deck= new List<GameCard>();

            for (int i = 0; i < MAX_DECK_SIZE; i++)
            {
                if (i < MAX_ATTACK_CARDS_COUNT)
                {
                    deck.Add(new AttackCard(i,CardDirection.Left));
                }
                else if (i < MAX_ATTACK_CARDS_COUNT + MAX_DEFENCE_CARDS_COUNT)
                {
                    deck.Add(new DefenceCard(i));
                }
                else
                {
                    deck.Add(new DeveloperCard(i));
                }
            }

            return new Deck(deck);
        }

    }
}
