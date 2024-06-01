using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model
{
    /// <summary>
    /// Класс игрока
    /// </summary>
    public class Player
    {
        private int _id;
        private int _score;
        private string _name;
        private PlayerHand _hand;

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public int Id { get => _id; set => _id = value; }
        /// <summary>
        /// Количество очков
        /// </summary>
        public int Score { get => _score; set => _score = value; }
        /// <summary>
        /// "Рука" игрока
        /// </summary>
        public PlayerHand Hand { get => _hand; set => _hand = value; }
        public string Name { get => _name; set => _name = value; }
        public bool IsReady { get; set; }

        public Player(int id){ Id = id; Score = 1; Hand = new PlayerHand(); }

        public Player(int id, string username) : this(id)
        {
            Name = username;
        }

        /// <summary>
        /// Сыграть руку
        /// </summary>
        public void PlayHand()
        {
            int scoreDelta = Hand.Play();
            Score += scoreDelta;
            Score = Score <= 1 ? 1 : Score;
        }
        
    }
    /// <summary>
    /// Класс "руки" игрока
    /// </summary>
    public class PlayerHand
    {
        private List<GameCard> _cards;
        private List<AttackCard> _targeted;

        /// <summary>
        /// Содержание руки игрока
        /// </summary>
        public List<GameCard> Cards { get => _cards; set => _cards = value; }

        /// <summary>
        /// Направленные на игрока атаки
        /// </summary>
        public List<AttackCard> Targeted { get => _targeted; set => _targeted = value; }
        public List<GameCard> CardQueue { get; private set; } = new List<GameCard>();
        public PlayerHand() { Cards = new List<GameCard>(); }

        public PlayerHand(List<GameCard> cards)
        {
            this._cards = cards;
            this._targeted = new List<AttackCard>();
        }

        /// <summary>
        /// Метод для реализации текущей руки. Отбиваем направленные атаки, используем выбранные карты
        /// </summary>
        public int Play()
        {
            List<GameCard> defenceCards = Cards.Where((card) => card is DefenceCard && card.State == CardState.Used).ToList();

            List<AttackType> defenceTypes = new List<AttackType>();

            foreach (GameCard card in defenceCards)
            {
                DefenceCard current = card as DefenceCard;
                defenceTypes.AddRange(current.AttackTypes);
            }

            //Карта защиты кроет карту атаки (сбрасывать тогда) ИЛИ как сейчас - если 2 трояна, а защита от трояна одна, то она защищает от обоих
            int damage = 0;

            foreach (AttackCard attack in Targeted)
            {
                if (!defenceTypes.Contains(attack.AttackType))
                {
                    damage += attack.Damage;
                }
            }

            List<GameCard> develeopmentCards = Cards.Where((card) => card is DeveloperCard && card.State == CardState.Used).ToList();

            int devPoints = 0;

            foreach (DeveloperCard devCard in develeopmentCards)
            {
                devPoints += devCard.DevelopmentPoint;
            }

            return devPoints - damage;

        }
        public List<GameCard> SCard(int selectedCardid)
        {
            List<GameCard> cards = new List<GameCard>();
            GameCard selectcard = Cards.FirstOrDefault(card => card.Id==selectedCardid);
            cards.Add(selectcard);
            selectcard.State=CardState.Used;
            CardQueue.Add(selectcard);
            if (!(Cards.Where(card => card.State==CardState.Used).Count()>3))
            {
                CardQueue.Add(selectcard);
                CardQueue[0].State=CardState.Unused;
                cards.Add(CardQueue[0]);
                CardQueue.RemoveAt(0);
            }            
            return cards;
        }
    }
}
