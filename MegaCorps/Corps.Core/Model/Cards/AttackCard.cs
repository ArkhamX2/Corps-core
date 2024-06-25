using MegaCorps.Core.Model.Enums;

namespace MegaCorps.Core.Model.Cards
{
    /// <summary>
    /// Класс атаки
    /// </summary>
    public class AttackCard : GameCard
    {
        /// <summary>
        /// Сила атаки
        /// </summary>
        public int Damage { get; set; }
        /// <summary>
        /// Направление действия карты
        /// </summary>
        public CardDirection Direction { get; set; }
        /// <summary>
        /// Тип атаки
        /// </summary>
        public AttackType AttackType { get; set; }
        public AttackCard(int id, CardDirection direction, int damage, AttackType type) : base(id)
        {
            Direction = direction;
            Damage = damage;
            AttackType = type;
        }

        public AttackCard(AttackCard card) : this(card.Id, card.Direction, card.Damage, card.AttackType) { }
    }
}
