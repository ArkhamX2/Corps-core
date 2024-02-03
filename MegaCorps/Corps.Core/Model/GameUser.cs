﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corps.Core.Model
{
    public class GameUser
    {
        public int ID { get; set; }
        public int NextId { get; set; }
        public int PrevId { get; set; }
        public int Score { get; set; }
        public GameUser(int id)
        {
            ID=id;
            Score=1;
        }
    }
}
