using MegaCorps.Core.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Corps.Analysis
{
    /// <summary>
    /// Анализатор игры
    /// </summary>
    public class Analizer
    {
        private const int MAX_CARDS = 6;
        private const int CARDS_TO_DEAL = 3;
        private const int CARDS_TO_CHOOSE = 3;
        private GameEngine _engine;
        private int _numberOfPlayers;
        private List<int> _winners;
        private List<ISelectionStrategy> _selectionStrategyList;
        public Analizer(List<ISelectionStrategy> selectionStrategyList)
        {
            _numberOfPlayers = selectionStrategyList.Count();
            _winners = Enumerable.Repeat(0, NumberOfPlayers).ToList();
            _selectionStrategyList = selectionStrategyList;
            Engine = new GameEngine(NumberOfPlayers);
        }

        /// <summary>
        /// Движок игры
        /// </summary>
        public GameEngine Engine { get => _engine; set => _engine = value; }
        /// <summary>
        /// Количество игроков
        /// </summary>
        public int NumberOfPlayers { get => _numberOfPlayers; set => _numberOfPlayers = value; }

        /// <summary>
        /// Запуск анализа набора игр
        /// </summary>
        /// <param name="numberOfIterations">количество игр в наборе</param>
        /// <returns></returns>
        public AnalizerResult Run(int numberOfIterations)
        {
            List<List<List<int>>> dups = new List<List<List<int>>>();
            int turnCount = 0;
            //Guid guid = Guid.NewGuid();
            //Запускаем сами игры
            for (int i = 0; i < numberOfIterations; i++)
            {
                _engine.Deal(MAX_CARDS);
                while (!_engine.Win)
                {
                    //имитация хода - создать n ботиков с разными стратегиями, которые будут просто выбирать из предложенных карт определённые 3
                    _engine.SelectCards(SelectHelper.SelectCards(_engine.GetPlayersHands(), _selectionStrategyList, CARDS_TO_CHOOSE, _engine.GetPlayersScores()));
                    _engine.Turn();
                    _engine.Deal(CARDS_TO_DEAL);
                    turnCount++;
                }

                List<List<int>> tmp = _engine.decks.Select(x => {
                    List<int> q = new List<int>();
                    q.AddRange(x.UnplayedCards.Select(y => y.Id).Reverse());
                    q.AddRange(x.PlayedCards.Select(y => y.Id));
                    return q;
                }).ToList();

                _winners[_engine.Winner - 1]++;

                var duplicates = (from list in tmp
                                 where tmp.Except(new[] { list }).Any(l => l.SequenceEqual(list))
                                 select list).ToList();

                dups.Add(duplicates);
                
            }

            float averageTurnCount = turnCount / numberOfIterations;
            List<float> averageWins = new List<float>();
            foreach (int winCount in _winners)
            {
                float percentage = (float)winCount / numberOfIterations;
                averageWins.Add(percentage);
            }

            _winners = Enumerable.Repeat(0, NumberOfPlayers).ToList();

            return new AnalizerResult(averageTurnCount, averageWins,dups);
            //return $"Количество итераций: {numberOfIterations}; Игроков: {NumberOfPlayers}; Среднее количество ходов: {averageTurnCount};\n\tСреднее количество выигрышей: \n{WinsToString(averageWins)}";
        }
    }

    /// <summary>
    /// Результат анализа набора игр
    /// </summary>
    public class AnalizerResult
    {
        public float averageTurnCount;
        public List<float> averageWins;
        public List<List<List<int>>> duplicates;
        public AnalizerResult(float averageTurnCount, List<float> averageWins, List<List<List<int>>> dups)
        {
            this.averageTurnCount = averageTurnCount;
            this.averageWins = averageWins;
            this.duplicates = dups;
        }
    }
}