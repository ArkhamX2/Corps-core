using Corps.Analysis;
using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.GameUtils;
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
        List<int> Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect);
        string Print();
    }

    /// <summary>
    /// Выбор лучших карт
    /// </summary>
    public class MonteCarloSelectStrategy : ISelectionStrategy
    {
        public List<int> Scores { get; set; }
        public Deck Deck { get; set; }
        public List<ISelectionStrategy> Strategies { get; set; }

        public float ChosenProbability { get; set; }
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect)
        {
            Dictionary<float, List<int>> selectedList = new Dictionary<float, List<int>>();
            for (int i = 0; i < cards[0].Count() - 2; i++)
            {
                for (int j = i + 1; j < cards[0].Count() - 1; j++)
                {
                    for (int k = j + 1; k < cards[0].Count(); k++)
                    {
                        List<int> tmp = new List<int> { i, j, k };
                        selectedList[AnalizeMonteCarlo(tmp, Scores, cards, Deck, Strategies)] = tmp;
                    }
                }
            }
            ChosenProbability = selectedList.Keys.Max();
            return selectedList[ChosenProbability];
        }

        private float AnalizeMonteCarlo(List<int> currentChoose, List<int> scores, List<List<GameCard>> cards, Deck deck, List<ISelectionStrategy> strategies)
        {
            int numberOfPlayers = strategies.Count();
            List<int> winners = Enumerable.Repeat(0, numberOfPlayers).ToList();
            List<ISelectionStrategy> selectionStrategies = new List<ISelectionStrategy>(strategies);
            selectionStrategies[0] = new RandomSelectStrategy();

            int turnCount = 0;
            int numberOfIterations = 1000;
            int CARDS_TO_CHOOSE = 3;
            int CARDS_TO_DEAL = 3;

            List<List<GameCard>> cardsCopy;
            SelectHelper selectHelper = new SelectHelper();
            GameEngine engine;

            for (int i = 0; i < numberOfIterations; i++)
            {
                cardsCopy = new List<List<GameCard>>();
                cards.ForEach(cardList => {
                    cardsCopy.Add(new List<GameCard>());
                    cardList.ForEach(card =>
                    {
                        if(card is AttackCard)
                        {
                            cardsCopy.Last().Add(new AttackCard(card as AttackCard));
                        }
                        if (card is DefenceCard)
                        {
                            cardsCopy.Last().Add(new DefenceCard(card as DefenceCard));
                        }
                        if (card is DeveloperCard)
                        {
                            cardsCopy.Last().Add(new DeveloperCard(card as DeveloperCard));
                        }
                    });
                });
                engine = new GameEngine(new List<int>(scores), cardsCopy, DeckBuilder.CopyDeck(deck));
                List<List<int>> tmpSelected = selectHelper.SelectCards(engine.GetPlayersHands(), selectionStrategies, CARDS_TO_CHOOSE, engine.GetPlayersScores(), engine.Deck).Values.First();
                tmpSelected[0] = currentChoose;
                engine.SelectCards(tmpSelected); 
                engine.Turn();
                engine.Deal(CARDS_TO_DEAL); 
                turnCount++;

                while (!engine.Win)
                {
                    engine.SelectCards(selectHelper.SelectCards(engine.GetPlayersHands(), selectionStrategies, CARDS_TO_CHOOSE, engine.GetPlayersScores(), engine.Deck).Values.First());
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
        List<int> ISelectionStrategy.Select(int playerIndex, List<List<GameCard>> cards, int numberToSelect)
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