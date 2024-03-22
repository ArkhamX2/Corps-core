using MegaCorps.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Corps.Analysis
{
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

        public GameEngine Engine { get => _engine; set => _engine = value; }
        public int NumberOfPlayers { get => _numberOfPlayers; set => _numberOfPlayers = value; }

        public string Run(int numberOfIterations)
        {
            int turnCount = 0;

            //Запускаем сами игры
            for (int i = 0; i < numberOfIterations; i++)
            {
                _engine.Deal(MAX_CARDS);
                while (!_engine.Win)
                {
                    //имитация хода - создать n ботиков с разными стратегиями, которые будут просто выбирать из предложенных карт определённые 3
                    _engine.SelectCards(SelectHelper.SelectCards(_engine.GetPlayersHands(), _selectionStrategyList, CARDS_TO_CHOOSE));
                    _engine.Turn();
                    if (_engine.Players[0].Hand.Cards.Count < 3)
                    {
                        Console.WriteLine("qwerty");
                    }
                    _engine.Deal(CARDS_TO_DEAL);
                    turnCount++;
                }
                _winners[_engine.Winner-1]++;
                _engine.Reset();
            }

            float averageTurnCount = turnCount / numberOfIterations;
            List<float> averageWins = new List<float>();
            foreach (int winCount in _winners)
            {
                float percentage = (float)winCount / numberOfIterations;
                averageWins.Add(percentage);
            }

            _winners = Enumerable.Repeat(0, NumberOfPlayers).ToList();

            return $"Количество итераций: {numberOfIterations}; Игроков: {NumberOfPlayers}; Среднее количество ходов: {averageTurnCount};\n\tСреднее количество выигрышей: \n{WinsToString(averageWins)}";
        }

        private object WinsToString(List<float> averageWins)
        {
            string ans = "";
            for (int i = 0; i < averageWins.Count; i++)
            {
                float wins = averageWins[i];
                ans += $"\t\t{i+1} Игрок:" + averageWins[i].ToString() + "\n";
            }
            return ans;
        }
    }
}