﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model.Cards
{
    public class DeveloperCard:GameCard
    {
       public int DevelopmentPoint { get; set; }
        public DeveloperCard(int id):base(id)
        {
                DevelopmentPoint = 1;
        }
    }
}
