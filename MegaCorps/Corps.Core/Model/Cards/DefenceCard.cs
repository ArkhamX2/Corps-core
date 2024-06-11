using MegaCorps.Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model.Cards
{
    /// <summary>
    /// Класс защиты
    /// </summary>
    public class DefenceCard:GameCard
    {
        /// <summary>
        /// Типы атак, от которых защищает карта
        /// </summary>
        public List<AttackType> AttackTypes { get; set; }
        public DefenceCard(int id, List<AttackType> attackTypes) : base(id)
        {
            AttackTypes = attackTypes;
        }

        public DefenceCard(DefenceCard card) : this(card.Id,card.AttackTypes){}
    }
}
