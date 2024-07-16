using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using MegaCorps.Core.Model.GameUtils;

namespace MegaCorps.Core.Model
{
    /// <summary>
    /// Игровой движок
    /// </summary>
    public class GameEngine
    {

        /// <summary>
        /// Колода
        /// </summary>
        public Deck Deck { get; set; } = new();
        /// <summary>
        /// Игроки
        /// </summary>
        public List<Player> Players { get; set; } = new();
        /// <summary>
        /// Индикатор победы
        /// </summary>
        public bool Win { get; set; } = false;
        /// <summary>
        /// Индекс победителя в списке игроков
        /// </summary>
        public int Winner { get; set; } = -1;


        /// <summary>
        /// Количество игроков
        /// </summary>
        private int NumberOfPlayers { get; }


        public GameEngine(Deck deck, List<string> usernameList)
        {
            Deck = deck;
            deck.Shuffle();
            Players = UserSetup.CreateUserList(usernameList);
            NumberOfPlayers = Players.Count;

        }

        public GameEngine(List<string> usernameList)
        {
            Deck = DeckBuilder.GetDeck();
            Deck.Shuffle();
            Players = UserSetup.CreateUserList(usernameList);
            NumberOfPlayers = Players.Count;
        }

        public GameEngine(int numberOfPlayers)
        {
            NumberOfPlayers = numberOfPlayers;
            Deck = DeckBuilder.GetDeck();
            Deck.Shuffle();
            Players = UserSetup.CreateUserList(numberOfPlayers);
        }

        public GameEngine(List<int> scores, List<List<GameCard>> cards, Deck deck)
        {
            NumberOfPlayers = cards.Count();
            Deck = DeckBuilder.CopyDeck(deck);
            Deck.UnplayedCards.ForEach(x => x.State = CardState.Unused);
            Players = UserSetup.CreateUserList(NumberOfPlayers);
            for (int i = 0; i < Players.Count; i++)
            {
                Player player = Players[i];
                player.Score = scores[i];
                player.Hand = new PlayerHand(cards[i]);
            }
        }

        public GameEngine() { }

        /// <summary>
        /// Раздать карты игрокам
        /// </summary>
        /// <param name="dealCount">Количество карт, которые необходимо раздать</param>
        public void Deal(int dealCount)
        {
            List<List<GameCard>> hands = Deck.Deal(dealCount, NumberOfPlayers);

            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Hand.Cards.AddRange(hands[i]);
            }
        }

        /// <summary>
        /// Раздать карты игрокам, исключая одного и его карты
        /// </summary>
        /// <param name="dealCount">Количество карт, которые необходимо раздать</param>
        public void DealExcept(int dealCount, int id, PlayerHand hand)
        {
            List<List<GameCard>> hands = Deck.DealExcept(dealCount, NumberOfPlayers, id, hand);

            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Hand.Cards.AddRange(hands[i]);
            }
        }

        /// <summary>
        /// Привести движок к начальному состоянию
        /// </summary>
        public void Reset()
        {
            Deck = DeckBuilder.GetDeck();
            Deck.Shuffle();
            Players = UserSetup.CreateUserList(NumberOfPlayers);
            Win = false;
        }

        /// <summary>
        /// Привести движок к начальному состоянию
        /// </summary>
        public void Reset(Deck deck)
        {
            Deck = deck;
            Deck.Shuffle();
            Players = UserSetup.CreateUserList(NumberOfPlayers);
            Win = false;
        }

        /// <summary>
        /// Получить руки всех игроков
        /// </summary>
        /// <returns></returns>
        public List<List<GameCard>> GetPlayersHands() => Players.Select(x => x.Hand.Cards).ToList();

        /// <summary>
        /// Нацелить все карты атаки всех игроков
        /// </summary>
        public void TargetCards()
        {
            List<List<GameCard>> all = GetPlayersHands()
                .Select(x => x
                    .Where((card) => card.State == CardState.Used && card is AttackCard)
                    .ToList()
                    )
                .ToList();

            TargetExactCard(all);

            RemoveTargetedCardsFromHand(all
                .SelectMany(x => x)
                .Select(x => x as AttackCard).ToList()!);
        }
        /// <summary>
        /// Совершить 1 ход
        /// </summary>
        public void Turn()
        {
            Players.ForEach(x => x.PlayHand());
            PlayEventCards();
            PrepareForNextTurn();
            CheckWin();
        }
        private void TargetExactCard(List<List<GameCard>> all)
        {
            for (int i = 0; i < all.Count(); i++)
            {
                for (int j = 0; j < all[i].Count(); j++)
                {
                    AttackCard currentCard = (all[i][j] as AttackCard)!;
                    switch (currentCard.Direction)
                    {
                        case CardDirection.Left:
                            Players[i == Players.Count() - 1 ? 0 : i + 1].Hand.Targeted.Add(currentCard);
                            break;
                        case CardDirection.Right:
                            Players[i == 0 ? Players.Count() - 1 : i - 1].Hand.Targeted.Add(currentCard);
                            break;
                        case CardDirection.All:
                            Players.ForEach(player => player.Hand.Targeted.Add(currentCard));
                            break;
                        case CardDirection.Allbutnotme:
                            Players
                                .Select((player, index) => index)
                                .Where(index => index != i).ToList()
                                .ForEach(index => Players[index].Hand.Targeted.Add(currentCard));
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void RemoveTargetedCardsFromHand(List<AttackCard> cardsToRemove) => Players
            .ForEach(player => player.Hand.Cards
                                          .RemoveAll(card => cardsToRemove
                                                             .Contains(card)));

        private void CheckWin()
        {
            Win = Players.Any(player => player.Score >= 10);
            Winner = Players.FindIndex(player => player.Score == Players.Max((item) => item.Score)) + 1;
        }

        private void PrepareForNextTurn()
        {
            foreach (Player v in Players)
            {
                if (v.Score < 1) v.Score = 1;
                Deck.PlayedCards.AddRange(v.Hand.Cards.Where((card) => card.State == CardState.Used));
                var t = v.Hand.Targeted.Where((card) => card.State == CardState.Used);
                foreach (var card in t)
                {
                    if (!Deck.PlayedCards.Contains(card))
                    {
                        Deck.PlayedCards.Add(card);
                    }
                }
                v.Hand.Cards.RemoveAll((card) => card.State == CardState.Used);
                v.Hand.Targeted.Clear();
            }
        }

        private void PlayEventCards()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Player player = Players[i];
                List<EventCard> events = player.Hand.Cards.Where(x => x is EventCard).Select(x => x as EventCard).ToList()!;
                for (int j = 0; j < events.Count; j++)
                {
                    EventCard eventCard = events[j];
                    if (eventCard is ScoreEventCard)
                    {
                        player.Score += (eventCard as ScoreEventCard)?.Power ?? 0;
                    }
                    if (eventCard is NeighboursEventCards)
                    {
                        Players[(i + 1) % Players.Count].Score += (eventCard as NeighboursEventCards)!.Power;
                        Players[(i - 1) % Players.Count].Score += (eventCard as NeighboursEventCards)!.Power;
                    }
                    if (eventCard is SwapEventCard)
                    {
                        int maxScore = Players.Select(x => x.Score).Max();
                        if (maxScore == player.Score)
                            SwapLeaderWithOutsider(player, maxScore);
                        else
                            SwapPlayerWithAnyLeader(player, maxScore);
                    }
                    if (eventCard is AllLosingCard)
                    {
                        Players
                            .Where(x => x.Score > 1).ToList()
                            .ForEach(player => player.Score -= 1);
                    }
                }
            }
        }

        private void SwapPlayerWithAnyLeader(Player player, int maxScore)
        {
            Random rnd = new Random();
            List<Player> leaders = Players.Where(x => x.Score == maxScore).ToList();
            leaders[rnd.Next(leaders.Count)].Score = player.Score;
            player.Score = maxScore;
        }

        private void SwapLeaderWithOutsider(Player player, int maxScore)
        {
            int minScore = Players.Select(x => x.Score).Min();
            Random rnd = new Random();
            List<Player> outsiders = Players.Where(x => x.Score == minScore).ToList();
            player.Score = minScore;
            outsiders[rnd.Next(outsiders.Count)].Score = maxScore;
        }

        /// <summary>
        /// Выбрать карты в соответствии с списком индексов
        /// </summary>
        /// <param name="hands"></param>
        public void SelectCards(List<List<int>> hands)
        {
            for (int i = 0; i < hands.Count; i++)
            {
                foreach (var card in hands[i])
                {
                    Players[i].Hand.Cards[card].State = CardState.Used;
                }
            }
        }

        public List<int> GetPlayersScores()
        {
            List<int> scores = new List<int>();
            foreach (Player player in Players)
            {
                scores.Add(player.Score);
            }
            return scores;
        }

    }
}
