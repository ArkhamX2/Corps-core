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
        List<int> Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect, List<List<int>> prev_selected);
        string Print();
    }

    /// <summary>
    /// Выбор лучших карт
    /// </summary>
    public class BestSelectStrategy : ISelectionStrategy
    {
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect, List<List<int>> prev_selected)
        {
            //смотрим на карты других игроков, считаем сколько очков они заработают и выбираем из своих 
            List<int> selected = new List<int>();
            return selected;

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
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect, List<List<int>> prev_selected)
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
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect, List<List<int>> prev_selected)
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
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect, List<List<int>> prev_selected)
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
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect, List<List<int>> prev_selected)
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