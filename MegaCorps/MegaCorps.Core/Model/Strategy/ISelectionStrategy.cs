using Corps.Analysis;
using MegaCorps.Core.Model.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MegaCorps.Core.Model
{
    /// <summary>
    /// Стратегия выбора карты
    /// </summary>
    public interface ISelectionStrategy
    {
        List<int> Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect, List<int> scores);
        string Print();
    }

    /// <summary>
    /// Выбор лучших карт
    /// </summary>
    public class MonteCarloSelectStrategy : ISelectionStrategy
    {
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect, List<int> scores)
        {
            Dictionary<float, List<int>> selectedList = new Dictionary<float, List<int>>();

            for (int l = 0; l < cards.Count(); l++)
            {
                for (int i = 0; i < cards[l].Count() - 2; i++)
                {
                    for (int j = i + 1; j < cards[l].Count() - 1; j++)
                    {
                        for (int k = j + 1; k < cards[l].Count(); k++)
                        {
                            List<int> tmp = new List<int> { i, j, k };
                            selectedList[AnalizeMonteCarlo(tmp, scores, cards)] = tmp;
                        }
                    }
                }
            }
            

            return selectedList[selectedList.Keys.Max()];

        }

        private float AnalizeMonteCarlo(List<int> currentChoose, List<int> scores, List<List<GameCard>> cards)
        {
            string BEST_STRATEGY = "BestStrategy";
            string RANDOM_STRATEGY = "RandomStrategy";
            string ATTACK_STRATEGY = "AttackStrategy";
            string DEFENCE_STRATEGY = "DefenseStrategy";
            string DEVELOP_STRATEGY = "DeveloperStrategy";

            Dictionary<string, ISelectionStrategy> possibleStrategy = new Dictionary<string, ISelectionStrategy>() {
            {BEST_STRATEGY ,new MonteCarloSelectStrategy() },
            {RANDOM_STRATEGY ,new RandomSelectStrategy() },
            {ATTACK_STRATEGY ,new AgressiveSelectStrategy() },
            {DEFENCE_STRATEGY ,new DefenciveSelectStrategy() },
            {DEVELOP_STRATEGY ,new DevelopSelectStrategy() }
            };

            List<ISelectionStrategy> selectionStrategyList = new List<ISelectionStrategy> {
                possibleStrategy[RANDOM_STRATEGY],
                possibleStrategy[RANDOM_STRATEGY],
                possibleStrategy[RANDOM_STRATEGY],
                possibleStrategy[RANDOM_STRATEGY],
                possibleStrategy[RANDOM_STRATEGY],
            };

            int numberOfPlayers = selectionStrategyList.Count();
            List<int> winners = Enumerable.Repeat(0, numberOfPlayers).ToList();

            int turnCount = 0;
            int numberOfIterations = 1000;
            int CARDS_TO_CHOOSE = 3;
            int CARDS_TO_DEAL = 3;

            for (int i = 0; i < numberOfIterations; i++)
            {
                GameEngine engine = new GameEngine(scores,cards);

                List<List<int>> tmpSelected = SelectHelper.SelectCards(engine.GetPlayersHands(), selectionStrategyList, CARDS_TO_CHOOSE, engine.GetPlayersScores());
                tmpSelected[0] = currentChoose;
                engine.SelectCards(tmpSelected);
                engine.Turn();
                engine.Deal(CARDS_TO_DEAL);
                turnCount++;

                while (!engine.Win)
                {
                    engine.SelectCards(SelectHelper.SelectCards(engine.GetPlayersHands(), selectionStrategyList, CARDS_TO_CHOOSE, engine.GetPlayersScores()));
                    engine.Turn();
                    engine.Deal(CARDS_TO_DEAL);
                    turnCount++;
                }

                winners[engine.Winner - 1]++;
            }

            List<float> averageWins = new List<float>();
            foreach (int winCount in winners)
            {
                float percentage = (float)winCount / numberOfIterations;
                averageWins.Add(percentage);
            }

            return averageWins[0];
        }

        string ISelectionStrategy.Print()
        {
            return "BestSelectStrategy";
        }

    }
    /// <summary>
    /// Случайный выбор карт
    /// </summary>
    public class RandomSelectStrategy : ISelectionStrategy
    {
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect, List<int> scores)
        {
            return Enumerable.Range(0, cards[0].Count()).OrderBy(x => RandomHelper.Next()).Take(numberToSelect).ToList();
        }
        string ISelectionStrategy.Print()
        {
            return "RandomSelectStrategy";
        }
    }
    /// <summary>
    /// Агрессивный выбор карт
    /// </summary>
    public class AgressiveSelectStrategy : ISelectionStrategy
    {
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect, List<int> scores)
        {
            //атака, разраб,защита
            List<int> selected = new List<int>();

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is AttackCard)
                {
                    AttackCard card = cards[playerIndex][i] as AttackCard;
                    if (card.Damage == 2)
                    {
                        selected.Add(i);
                    }
                }
            }

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is AttackCard)
                {
                    AttackCard card = cards[playerIndex][i] as AttackCard;
                    if (card.Damage != 2)
                    {
                        selected.Add(i);
                    }
                }
            }

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is DeveloperCard)
                {
                    DeveloperCard card = cards[playerIndex][i] as DeveloperCard;
                    if (card.DevelopmentPoint == 2)
                    {
                        selected.Add(i);
                    }
                }
            }

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is DeveloperCard)
                {
                    DeveloperCard card = cards[playerIndex][i] as DeveloperCard;
                    if (card.DevelopmentPoint != 2)
                    {
                        selected.Add(i);
                    }
                }
            }
            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is DefenceCard)
                {
                    selected.Add(i);
                }
            }


            return selected;
        }

        string ISelectionStrategy.Print()
        {
            return "AgressiveSelectStrategy";
        }
    }
    /// <summary>
    /// Защитный выбор карт
    /// </summary>
    public class DefenciveSelectStrategy : ISelectionStrategy
    {
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect, List<int> scores)
        {
            //защита, разраб,атака
            List<int> selected = new List<int>();

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is DefenceCard)
                {
                    selected.Add(i);
                }
            }

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is DeveloperCard)
                {
                    DeveloperCard card = cards[playerIndex][i] as DeveloperCard;
                    if (card.DevelopmentPoint == 2)
                    {
                        selected.Add(i);
                    }
                }
            }

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is DeveloperCard)
                {
                    DeveloperCard card = cards[playerIndex][i] as DeveloperCard;
                    if (card.DevelopmentPoint != 2)
                    {
                        selected.Add(i);
                    }
                }
            }

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is AttackCard)
                {
                    AttackCard card = cards[playerIndex][i] as AttackCard;
                    if (card.Damage == 2)
                    {
                        selected.Add(i);
                    }
                }
            }

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is AttackCard)
                {
                    AttackCard card = cards[playerIndex][i] as AttackCard;
                    if (card.Damage != 2)
                    {
                        selected.Add(i);
                    }
                }
            }

            return selected;
        }
        string ISelectionStrategy.Print()
        {
            return "DefenciveSelectStrategy";
        }
    }
    /// <summary>
    /// Развивающийся выбор карт
    /// </summary>
    public class DevelopSelectStrategy : ISelectionStrategy
    {
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect, List<int> scores)
        {
            //разраб,защита, атака
            List<int> selected = new List<int>();

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is DeveloperCard)
                {
                    DeveloperCard card = cards[playerIndex][i] as DeveloperCard;
                    if (card.DevelopmentPoint == 2)
                    {
                        selected.Add(i);
                    }
                }
            }

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is DeveloperCard)
                {
                    DeveloperCard card = cards[playerIndex][i] as DeveloperCard;
                    if (card.DevelopmentPoint != 2)
                    {
                        selected.Add(i);
                    }
                }
            }
            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is DefenceCard)
                {
                    selected.Add(i);
                }
            }

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is AttackCard)
                {
                    AttackCard card = cards[playerIndex][i] as AttackCard;
                    if (card.Damage == 2)
                    {
                        selected.Add(i);
                    }
                }
            }

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i] is AttackCard)
                {
                    AttackCard card = cards[playerIndex][i] as AttackCard;
                    if (card.Damage != 2)
                    {
                        selected.Add(i);
                    }
                }
            }

            return selected;
        }
        string ISelectionStrategy.Print()
        {
            return "DevelopSelectStrategy";
        }
    }
}