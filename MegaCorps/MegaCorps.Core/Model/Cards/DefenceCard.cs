using MegaCorps.Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model.Cards
{
    public class DefenceCard:GameCard
    {
        public int Damage { get; set; }
        List<AttackType> AttackTypes { get; set; }
        public DefenceCard(int id, int damage, List<AttackType> attackTypes) : base(id)
        {
            Color = "Green";
            Damage = damage;
            AttackTypes = attackTypes;
        }
    }
}
