using MegaCorps.Core.Model.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model
{
    public static class SelectHelper
    {
        public static List<List<int>> SelectCards(List<List<GameCard>> hands, List<ISelectionStrategy> strategyList, int numberToSelect)
        {
            var selected = new List<List<int>>
            {
                strategyList[0].Select(0, hands,numberToSelect)
            };
            for (int i = 1; i < hands.Count(); i++)
            {
                selected.Add(strategyList[i].Select(i, hands, numberToSelect));
            }
            return selected;
        }
    }
}
