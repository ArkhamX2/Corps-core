using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.GameUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model
{
    /// <summary>
    /// Класс, позволяющий выбрать карты статично
    /// </summary>
    public class SelectHelper
    {
        public List<List<int>> SelectCards(List<List<GameCard>> hands, List<ISelectionStrategy> strategyList, int numberToSelect, List<int> scores, Deck deck)
        {
            var selected = new List<List<int>>();
            for (int i = 0; i < hands.Count(); i++)
            {
                if (strategyList[i] is MonteCarloSelectStrategy)
                {
                    MonteCarloSelectStrategy monteCarloSelectStrategy = new MonteCarloSelectStrategy();
                    monteCarloSelectStrategy.Deck = deck;
                    monteCarloSelectStrategy.Scores = scores;
                    monteCarloSelectStrategy.Strategies = strategyList;
                    selected.Add((monteCarloSelectStrategy as ISelectionStrategy).Select(i, hands, numberToSelect));
                    (strategyList[i] as MonteCarloSelectStrategy).ChosenProbability.AddRange(monteCarloSelectStrategy.ChosenProbability);
                }
                else
                {
                    selected.Add(strategyList[i].Select(i, hands, numberToSelect));
                }
            }
            return selected;
        }
    }
}
