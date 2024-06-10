using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using MegaCorps.Core.Model.GameUtils;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model
{
    /// <summary>
    /// Игровой движок
    /// </summary>
    public class GameEngine
    {
        
        private Deck _deck = new();
        /// <summary>
        /// Колода
        /// </summary>
        public Deck Deck { get => _deck; set => _deck = value; }
        /// <summary>
        /// Игроки
        /// </summary>
        public List<Player> Players { get => _players; set => _players = value; }
        /// <summary>
        /// Индикатор победы
        /// </summary>
        public bool Win { get => _win; set => _win = value; }
        /// <summary>
        /// Индекс победителя в списке игроков
        /// </summary>
        public int Winner { get => _winner; set => _winner = value; }


        /// <summary>
        /// Количество игроков
        /// </summary>
        private int NumberOfPlayers { get; }

        private int _winner = new();


        private bool _win = new();

        private List<Player> _players = new();

        public GameEngine(Deck deck, List<string> usernameList)
        {
            Deck = deck;
            deck.Shuffle();
            Players = UserSetup.CreateUserList(usernameList);
            NumberOfPlayers = Players.Count;
            _win = false;

        }

        public GameEngine(List<string> usernameList)
        {
            Deck = DeckBuilder.GetDeck();
            Deck.Shuffle();
            Players = UserSetup.CreateUserList(usernameList);
            NumberOfPlayers = Players.Count;
            _win = false;
        }

        public GameEngine(int numberOfPlayers)
        {
            NumberOfPlayers = numberOfPlayers;
            Deck = DeckBuilder.GetDeck();
            Deck.Shuffle();
            Players = UserSetup.CreateUserList(numberOfPlayers);
            _win = false;
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
            _win = false;
        }

        public GameEngine()
        {
        }

        /// <summary>
        /// Раздать карты игрокам
        /// </summary>
        /// <param name="dealCount">Количество карт, которые необходимо раздать</param>
        public void Deal(int dealCount)
        {
            List<List<GameCard>> hands = Deck.Deal(dealCount, NumberOfPlayers);

            if (hands.Count == 0)
            {
                Deck = DeckBuilder.GetDeck();
                Deck.Shuffle();
                hands = Deck.Deal(dealCount, NumberOfPlayers);
            }
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
            _win = false;
        }

        /// <summary>
        /// Получить руки всех игроков
        /// </summary>
        /// <returns></returns>
        public List<List<GameCard>> GetPlayersHands()
        {
            List<List<GameCard>> hands = new List<List<GameCard>>();

            for (int i = 0; i < Players.Count; i++)
            {
                hands.Add(Players[i].Hand.Cards);
            }


            return hands;
        }
        public void TargetCards()
        {
            List<List<GameCard>> all = GetPlayersHands();
            for (int i = 0; i < all.Count(); i++)
            {
                List<GameCard> playerUsedAttacks = all[i].Where((card) => card.State == CardState.Used && card is AttackCard).ToList();
                for (int j = 0; j < playerUsedAttacks.Count(); j++)
                {
                    AttackCard currentCard = (playerUsedAttacks[j] as AttackCard)!;
                    switch (currentCard.Direction)
                    {
                        case CardDirection.Left:
                            Players[i == Players.Count() - 1 ? 0 : i + 1].Hand.Targeted.Add(currentCard);
                            break;
                        case CardDirection.Right:
                            Players[i == 0 ? Players.Count() - 1 : i - 1].Hand.Targeted.Add(currentCard);
                            break;
                        case CardDirection.All:
                            foreach (Player player in Players)
                            {
                                player.Hand.Targeted.Add(currentCard);
                            }
                            break;
                        case CardDirection.Allbutnotme:
                            for (int k = 0; k < Players.Count; k++)
                            {
                                if (k != i)
                                {
                                    Players[k].Hand.Targeted.Add(currentCard);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    Players.ForEach(player => {
                        if (player.Hand.Cards.Select(x => x.Id).Contains(currentCard.Id))
                        {
                            player.Hand.Cards.Remove(currentCard);
                        }
                    });
                }

            }
        }

        /// <summary>
        /// Совершить 1 ход
        /// </summary>
        public void Turn()
        {
            foreach (Player player in Players)
            {
                player.PlayHand();
            }

            for (int i = 0; i < Players.Count; i++)
            {
                Deck.PlayedCards.AddRange(Players[i].Hand.Cards.Where((card) => card.State == CardState.Used));
                Players[i].Hand.Cards.RemoveAll((card) => card.State == CardState.Used);
                Players[i].Hand.Targeted.Clear();
            }
            Win = Players.Any(player => player.Score >= 10);
            Winner = Players.FindIndex(player => player.Score == Players.Max((item) => item.Score)) + 1;

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
