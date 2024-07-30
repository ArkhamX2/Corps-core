using MegaCorps.Core.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corps.Core.Model.Common
{
    public class SelectedCardsConcurrentDictionary
    {
        public ConcurrentDictionary<(int, PlayerHand, List<int>, Deck), (List<int>, double)> concurrentDictionary = new ConcurrentDictionary<(int, PlayerHand, List<int>, Deck), (List<int>, double)>();

    }
}
