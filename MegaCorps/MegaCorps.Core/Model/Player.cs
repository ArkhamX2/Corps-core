using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model
{
    public class Player : GameUser
    {
        private int _score;
        private PlayerHand _hand;
        private List<GameCard> _targeted;
        private Queue<GameCard> _selected;

        public int Score { get => _score; set => _score = value; }
        public PlayerHand Hand { get => _hand; set => _hand = value; }
        public List<GameCard> Targeted { get => _targeted; set => _targeted = value; }
        public Queue<GameCard> Selected { get => _selected; set => _selected = value; }

        public Player(int id) : base(id) { Score = 1; Hand = new PlayerHand(); Selected = new Queue<GameCard>(); }

        public void PlayHand()
        {
            int defenceCards = Hand.Cards.Where((card) => card.Color == "Green" && card.State == CardState.Used).Count();
            if (_targeted.Count > defenceCards)
            {
                Score += defenceCards - _targeted.Count();
            }
            Score += Hand.Cards.Where((card) => card.Color == "Yellow" && card.State == CardState.Used).Count();
        
        }
    }

    public class PlayerHand
    {
        private List<GameCard> _cards;

        public List<GameCard> Cards { get => _cards; set => _cards = value; }

        public PlayerHand() { Cards = new List<GameCard>(); }

        public PlayerHand(List<GameCard> cards)
        {
            this._cards = cards;
        }

    }
}
