using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using MegaCorps.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corps.Core.Model.GameUtils
{
    public class DynamicDeckBuilder
    {
        private int MAX_DECK_SIZE = 100;
        private int MAX_ATTACK_CARDS_COUNT = 35; //Всех типов атак по 5
        private int MAX_DEFENCE_CARDS_COUNT = 35;
        List<AttackType> attackTypes = new List<AttackType>() {
            AttackType.Trojan,
            AttackType.Worm,
            AttackType.DoS,
            AttackType.Scripting,
            AttackType.Botnet,
            AttackType.Fishing,
            AttackType.Spy };
        List<CardDirection> directionList = new List<CardDirection>() { 
            CardDirection.Left,
            CardDirection.Left,
            CardDirection.Left,
            CardDirection.Left,
            CardDirection.Right, 
            CardDirection.Right,
            CardDirection.Right,
            CardDirection.Right,
            CardDirection.All,
            CardDirection.Allbutnotme };

        public DynamicDeckBuilder(int deck_size, int attack_cards_count, int defence_cards_count)
        {
            MAX_DECK_SIZE = deck_size;
            MAX_ATTACK_CARDS_COUNT = attack_cards_count;
            MAX_DEFENCE_CARDS_COUNT = defence_cards_count;
        }

        /// <summary>
        /// Сформировать колоду с нуля
        /// </summary>
        /// <returns></returns>
        public Deck GetDeck()
        {
            var deck = new List<GameCard>();

            for (int i = 0; i < MAX_DECK_SIZE; i++)
            {
                if (i < MAX_ATTACK_CARDS_COUNT)
                {
                    deck.Add(new AttackCard(
                        i,
                        directionList[i % directionList.Count()],
                        i >= MAX_ATTACK_CARDS_COUNT * 0.75 ? 2 : 1,
                        attackTypes[i % attackTypes.Count()]));
                }
                else if (i < MAX_ATTACK_CARDS_COUNT + MAX_DEFENCE_CARDS_COUNT)
                {
                    deck.Add(new DefenceCard(
                        i,
                        new List<AttackType> { attackTypes[
                            i % attackTypes.Count() > 0 ?
                                i % attackTypes.Count() - 1 :
                                attackTypes.Count() - 1
                            ],
                            attackTypes[i % attackTypes.Count()] }));
                }
                else
                {
                    deck.Add(new DeveloperCard(
                        i,
                        i - MAX_ATTACK_CARDS_COUNT - MAX_DEFENCE_CARDS_COUNT >= (MAX_DECK_SIZE - MAX_ATTACK_CARDS_COUNT - MAX_DEFENCE_CARDS_COUNT) * 0.75 ? 2 : 1
                        ));
                }
            }

            return new Deck(deck);
        }
    }
}
