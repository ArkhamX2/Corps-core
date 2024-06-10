using Corps.Server.Services;
using MegaCorps.Core.Model;
using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.GameUtils;
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
        private GameEngine _engine = new();
        private int _numberOfPlayers = new();
        private List<int> _winners = new();
        private List<ISelectionStrategy> _selectionStrategyList = new();
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
            SelectHelper selectHelper = new SelectHelper();
            ImageService imageService = new ImageService(
                                        "..\\..\\..\\..\\Corps.Server\\Resource\\Text\\Card\\Direction\\directions.json",
                                        "..\\..\\..\\..\\Corps.Server\\Resource\\Text\\Card\\Description",
                                        "..\\..\\..\\..\\Corps.Server\\Resource\\Image"
                                        );
            int turnCount = 0;
            List<string> scores = new List<string>();
            for (int i = 0; i < numberOfIterations; i++)
            {
                _engine.Reset(DeckBuilder.GetDeckFromResources(imageService.attackInfos, imageService.defenceInfos, imageService.developerInfos, imageService.directions));
                _engine.Deal(MAX_CARDS);
                while (!_engine.Win)
                {
                    List<List<int>> tmp = selectHelper.SelectCards(_engine.GetPlayersHands(), _selectionStrategyList, CARDS_TO_CHOOSE, _engine.GetPlayersScores(), _engine.Deck);
                    _engine.SelectCards(tmp);
                    _engine.TargetCards();
                    _engine.Turn();
                    _engine.Deal(CARDS_TO_DEAL);

                    turnCount++;
                }
                _engine.Players.Select(x => x.Score).ToList().ForEach(x => scores.Add(Convert.ToString(x)));
                _winners[_engine.Winner - 1]++;
            }

            float averageTurnCount = turnCount / numberOfIterations;
            List<float> averageWins = new List<float>();
            foreach (int winCount in _winners)
            {
                float percentage = (float)winCount / numberOfIterations;
                averageWins.Add(percentage);
            }

            _winners = Enumerable.Repeat(0, NumberOfPlayers).ToList();

            return new AnalizerResult(averageTurnCount, averageWins, scores, string.Join("%|-|", (_selectionStrategyList.Where(x => x is MonteCarloSelectStrategy).First() as MonteCarloSelectStrategy)!.ChosenProbability.Select(x => (x / 1000) * 100)));
        }
    }

    /// <summary>
    /// Результат анализа набора игр
    /// </summary>
    public class AnalizerResult
    {
        public float averageTurnCount = new();
        public List<float> averageWins = new();
        public List<string> scores = new();
        public string MCProbability;
        public AnalizerResult(float averageTurnCount, List<float> averageWins, List<string> scores, string probabilities)
        {
            this.averageTurnCount = averageTurnCount;
            this.averageWins = averageWins;
            this.scores = scores;
            this.MCProbability = probabilities;
        }
    }
}