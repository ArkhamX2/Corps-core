﻿using MegaCorps.Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model.Cards
{
    public class AttackCard:GameCard
    {
        public int Damage {  get; set; }
        public CardDirection Direction { get; set; }
        public AttackCard(int id,CardDirection direction) : base(id)
        {
            Direction = direction;
            Damage = 1;
            Color = "Red";
        }
        

    }
}
