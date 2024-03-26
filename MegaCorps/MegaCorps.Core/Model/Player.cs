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
        private List<AttackCard> _targeted;
        private Queue<GameCard> _selected;

        public new int Score { get => _score; set => _score = value; }
        public PlayerHand Hand { get => _hand; set => _hand = value; }
        public List<AttackCard> Targeted { get => _targeted; set => _targeted = value; }
        public Queue<GameCard> Selected { get => _selected; set => _selected = value; }

        public Player(int id) : base(id) { Score = 1; Hand = new PlayerHand(); Selected = new Queue<GameCard>(); Targeted = new List<AttackCard>(); }

        public void PlayHand()
        {
            List<GameCard> defenceCards = Hand.Cards.Where((card) => card is DefenceCard && card.State == CardState.Used).ToList();

            List<AttackType> defenceTypes = new List<AttackType>();

            foreach (GameCard card in defenceCards)
            {
                DefenceCard current = card as DefenceCard;
                defenceTypes.AddRange(current.AttackTypes);
            }

            int damage = 0;

            foreach (AttackCard attack in Targeted)
            {
                if (!defenceTypes.Contains(attack.AttackType))
                {
                    damage += attack.Damage;
                }
            }

            Score += Hand.Cards.Where((card) => card is DeveloperCard && card.State == CardState.Used).Count() - damage;
        
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
