
using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Common;
using MegaCorps.Core.Model.Enums;
using System.Data;

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
        public static Deck GetDeckFromResources(
            List<AttackCardDescriptionInfo> attackInfos,
            List<DefenceCardDescriptionInfo> defenceInfos,
            Queue<DeveloperCardDescriptionInfo> developerInfos,
            List<CardDirectionInfo> directionList,
            Queue<EventCardDescriptionInfo> eventInfos)
        {
            var deck = new List<GameCard>();
            int id = 0;
            CreateAttackCards(attackInfos, deck, ref id);
            CreateDefenceCards(defenceInfos, deck, ref id);
            CreateDeveloperCards(deck, developerInfos.Select(x => x.Amount).Sum(), ref id);
            //CreateEventCartds(deck, eventInfos.Select(x => x.Amount).Sum(), ref id);

            return new Deck(deck);
        }

        private static void CreateEventCartds(List<GameCard> deck, int eventAmount, ref int id)
        {
            int eventCounter = 0;
            while (eventCounter < eventAmount)
            {
                if (eventCounter % eventAmount == 0)
                {
                    deck.Add(new ScoreEventCard(id, 2));
                }
                if (eventCounter % eventAmount == 1)
                {
                    deck.Add(new NeighboursEventCards(id, 2));
                }
                if (eventCounter % eventAmount == 2)
                {
                    deck.Add(new SwapEventCard(id));
                }
                if (eventCounter % eventAmount == 3)
                {
                    deck.Add(new AllLosingCard(id, 2));
                }
                id++;
                eventCounter++;
            }

        }

        private static void CreateDeveloperCards(List<GameCard> deck, int developerAmount, ref int id)
        {
            int queueCounter = 0;
            while (queueCounter < developerAmount)
            {
                deck.Add(new DeveloperCard(
                    id,
                    queueCounter < (developerAmount * 2 / 3) ? 1 : 2
                    ));
                id++;
                queueCounter++;
            }
        }

        private static void CreateDefenceCards(List<DefenceCardDescriptionInfo> defenceInfos, List<GameCard> deck, ref int id)
        {
            foreach (DefenceCardDescriptionInfo defence in defenceInfos)
            {
                int counter = 0;
                while (counter < defence.Amount)
                {
                    deck.Add(new DefenceCard(
                        id: id,
                        defence.AttackTypeList
                        ));
                    counter++;
                    id++;
                }
            }
        }

        private static void CreateAttackCards(List<AttackCardDescriptionInfo> attackInfos, List<GameCard> deck, ref int id)
        {
            foreach (AttackCardDescriptionInfo attack in attackInfos)
            {
                int counter = 0;
                while (counter < attack.Amount)
                {
                    deck.Add(new AttackCard(
                        id,
                        attack.DirectionList[counter % attack.DirectionList.Count],
                        counter == attack.Amount - 1 ? 2 : 1,
                        attack.AttackType
                        ));
                    counter++;
                    id++;
                }
            }
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
