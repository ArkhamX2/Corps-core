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
            List<int> selected = new List<int>();
            
            Dictionary<int,List<int>> selectedList = new Dictionary<int, List<int>>();
            for (int i = 0; i < cards.Count()-2; i++)
            {
                for (int j = i+1; j < cards.Count()-1; j++)
                {
                    for (int k = j+1; k < cards.Count(); k++)
                    {
                        List<int> tmp = new List<int> { i, j, k };
                        selectedList[AnalizeMonteCarlo(tmp,scores,cards)] = tmp;
                    }
                }
            }

            return selectedList[selectedList.Keys.Max()];

        }

        private int AnalizeMonteCarlo(List<int> tmp, List<int> scores, List<List<GameCard>> cards)
        {
            throw new NotImplementedException();
        }

        string ISelectionStrategy.Print() {
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
            return Enumerable.Range(0, cards[0].Count()).OrderBy(x => new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode())).Value.Next()).Take(numberToSelect).ToList();
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
                    if(card.Damage == 2)
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