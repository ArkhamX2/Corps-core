using MegaCorps.Core.Model.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MegaCorps.Core.Model
{
    public interface ISelectionStrategy
    {
        List<int> Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect);
        //string Print();
    }

    public class BestSelectStrategy : ISelectionStrategy
    {
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect)
        {
            throw new System.NotImplementedException();
        }

    }
    public class RandomSelectStrategy : ISelectionStrategy
    {
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect)
        {
            
            return Enumerable.Range(0, cards[0].Count()).OrderBy(x => RandomHelper.Next()).Take(numberToSelect).ToList();
        }
    }
    public class AgressiveSelectStrategy : ISelectionStrategy
    {
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect)
        {
            //атака, разраб,защита
            List<int> selected = new List<int>();

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i].Color == "Red")
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
                if (cards[playerIndex][i].Color == "Yellow")
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
                if (cards[playerIndex][i].Color == "Green")
                {
                    selected.Add(i);
                }
            }
            

            return selected;
        }
    }
    public class DefenciveSelectStrategy : ISelectionStrategy
    {
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect)
        {
            //защита, разраб,атака
            List<int> selected = new List<int>();

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i].Color == "Green")
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
                if (cards[playerIndex][i].Color == "Yellow")
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
                if (cards[playerIndex][i].Color == "Red")
                {
                    selected.Add(i);
                }
            }

            return selected;
        }
    }
    public class DevelopSelectStrategy : ISelectionStrategy
    {
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect)
        {
            //разраб,защита, атака
            List<int> selected = new List<int>();

            for (int i = 0; i < cards[playerIndex].Count(); i++)
            {
                if (selected.Count() >= 3)
                {
                    return selected;
                }
                if (cards[playerIndex][i].Color == "Yellow")
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
                if (cards[playerIndex][i].Color == "Green")
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
                if (cards[playerIndex][i].Color == "Red")
                {
                    selected.Add(i);
                }
            }
            
            return selected;
        }
    }
}