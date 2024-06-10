using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Common;
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
    /// <summary>
    /// Класс, формирующий колоду
    /// </summary>
    public static class DeckBuilder
    {
        private const int MAX_DECK_SIZE = 100;
        private const int MAX_ATTACK_CARDS_COUNT = 35; //Всех типов атак по 5
        private const int MAX_DEFENCE_CARDS_COUNT = 35;
        static List<AttackType> attackTypes = new List<AttackType>() {
            AttackType.Trojan,
            AttackType.Worm,
            AttackType.DoS,
            AttackType.Scripting,
            AttackType.Botnet,
            AttackType.Fishing,
            AttackType.Spy };
        static List<CardDirection> directionList = new List<CardDirection>() { CardDirection.Left,CardDirection.Left,CardDirection.Left,CardDirection.Left,CardDirection.Left,CardDirection.Left,
            CardDirection.Right, CardDirection.Right,CardDirection.Right,CardDirection.Right,CardDirection.Right,CardDirection.Right,
            CardDirection.All,CardDirection.Allbutnotme };

        /// <summary>
        /// Сформировать колоду с нуля
        /// </summary>
        /// <returns></returns>
        public static Deck GetDeck()
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

        /// <summary>
        /// Сформировать колоду с нуля
        /// </summary>
        /// <returns></returns>
        public static Deck GetDeckFromResources(List<AttackCardDescriptionInfo> attackInfos, List<DefenceCardDescriptionInfo> defenceInfos, Queue<DeveloperCardDescriptionInfo> developerInfos, List<CardDirectionInfo> directionList)
        {
            var deck = new List<GameCard>();

            int attackAmount = attackInfos.Select(x => x.Amount).Sum();
            int defenceAmount = defenceInfos.Select(x => x.Amount).Sum();
            int developerAmount = developerInfos.Select(x => x.Amount).Sum();
            List<CardDirection> directions = new List<CardDirection>();
            directionList.ForEach(x => directions.AddRange(Enumerable.Range(1, x.Amount).Select(y=>x.Direction).ToList()));

            int id = 0;
            attackInfos.ForEach(x =>
            {
                int counter = 0;
                while (counter < x.Amount - 1)
                {
                    deck.Add(new AttackCard(
                        id,
                        x.DirectionList[counter%x.DirectionList.Count],
                        1,
                        x.AttackType
                        ));
                    counter++;
                    id++;
                }
                deck.Add(new AttackCard(
                        id,
                        x.DirectionList[counter % x.DirectionList.Count],
                        2,
                        x.AttackType
                        ));
                id++;
            });

            defenceInfos.ForEach(x =>
            {
                int counter = 0;
                while (counter < x.Amount)
                {
                    deck.Add(new DefenceCard(
                        id,
                        x.AttackTypeList
                        ));
                    counter++;
                    id++;
                }
                
            });

            int queueCounter = 0;
            while(queueCounter < (developerAmount * 2/3))
            {
                deck.Add(new DeveloperCard(
                    id,
                    1
                    ));
                id++;
                queueCounter++;
            }
            while (queueCounter < developerAmount)
            {
                deck.Add(new DeveloperCard(
                    id,
                    2
                    ));
                id++;
                queueCounter++;
            }
            return new Deck(deck);
        }

        public static Deck CopyDeck(Deck deck)
        {
            List<GameCard> cards = new List<GameCard>();
            List<GameCard> unplayed = new List<GameCard>();
            deck.UnplayedCards.ForEach(card =>
            {
                if (card is AttackCard)
                {
                    unplayed.Add(new AttackCard((card as AttackCard)!));
                }
                if (card is DefenceCard)
                {
                    unplayed.Add(new DefenceCard((card as DefenceCard)!));
                }
                if (card is DeveloperCard)
                {
                    unplayed.Add(new DeveloperCard((card as DeveloperCard)!));
                }
            });
            List<GameCard> played = new List<GameCard>();
            deck.PlayedCards.ForEach(card =>
            {
                if (card is AttackCard)
                {
                    played.Add(new AttackCard((card as AttackCard)!));
                }
                if (card is DefenceCard)
                {
                    played.Add(new DefenceCard((card as DefenceCard)!));
                }
                if (card is DeveloperCard)
                {
                    played.Add(new DeveloperCard((card as DeveloperCard)!));
                }
            });
            return new Deck(unplayed, played);
        }
    }
}
