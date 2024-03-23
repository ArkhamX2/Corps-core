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
        public DefenceCard(int id, int damage) : base(id)
        {
            Color = "Green";
            Damage = damage;
        }
    }
}
