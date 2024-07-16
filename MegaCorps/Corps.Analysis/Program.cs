using Corps.Core.Model.Common;
using Corps.Server.Hubs;
using Corps.Server.Services;
using MegaCorps.Core.Model;
using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using MegaCorps.Core.Model.GameUtils;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace Corps.Analysis
{
    internal class Program
    {
        private const string MONTE_CARLO = "BestStrategy";
        private const string RANDOM_STRATEGY = "RandomStrategy";
        private const string ATTACK_STRATEGY = "AttackStrategy";
        private const string DEFENCE_STRATEGY = "DefenseStrategy";
        private const string DEVELOP_STRATEGY = "DeveloperStrategy";
        private const int ITERATION_COUNT = 50;
        private static Dictionary<string, ISelectionStrategy> possibleStrategy = new Dictionary<string, ISelectionStrategy>() {
            {MONTE_CARLO ,new MonteCarloSelectStrategy() },
            {RANDOM_STRATEGY ,new RandomSelectStrategy() },
            {ATTACK_STRATEGY ,new AgressiveSelectStrategy() },
            {DEFENCE_STRATEGY ,new DefenciveSelectStrategy() },
            {DEVELOP_STRATEGY ,new DevelopSelectStrategy() }
        };
        private static List<List<ISelectionStrategy>> strategiesList = new List<List<ISelectionStrategy>> { };

        static void Main(string[] args)
        {
            /*FillStrategiesList();
            //Добавляем ещё несколько вариантов игр, где игроки сидят в разном порядке
            strategiesList.Add(Shuffle(strategiesList[0]));
            strategiesList.Add(Shuffle(strategiesList[1]));
            //strategiesList.Add(Shuffle(strategiesList[3]));
            //strategiesList.Add(Shuffle(strategiesList[3]));
            //strategiesList.Add(Shuffle(strategiesList[3]));
            strategiesList.Add(Shuffle(strategiesList[2]));
            //strategiesList.Add(Shuffle(strategiesList[4]));
            //strategiesList.Add(Shuffle(strategiesList[4]));
            //strategiesList.Add(Shuffle(strategiesList[4]));
            //strategiesList.Add(Shuffle(strategiesList[4]));
            //for (int i = 0; i < 1; i++)
            //{
            //    AnalizeOneGame(strategiesList[2]);
            //}
            //Console.WriteLine("Конец");
            TestBestStrategy();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();*/            
            var results =  new Dictionary<int, int>();
            var bots = new List<Bot>() { new Bot(0, 0, "random"), new Bot(1, 0, "random"), new Bot(2, 1, "aggressive"), new Bot(3, 4, "clever"), new Bot(4, 3, "researchive"), new Bot(5, 4, "clever"), };
            List<string> names = new List<string>();
            foreach (var bot in bots)
            {
                names.Add(bot.Name);
                results.Add(bot.Id, 0);
            }
            results.Add(bots.Count(), 0);
            int n = 100;
            for (int i = 0; i < n; i++)
            {
                ImageService imageService = new ImageService(
                    "..\\..\\..\\..\\Corps.Server\\Resource\\Text\\Card\\Direction\\directions.json",
                    "..\\..\\..\\..\\Corps.Server\\Resource\\Text\\Card\\Description",
                    "..\\..\\..\\..\\Corps.Server\\Resource\\Image"
                    );
                Deck deck = DeckBuilder.GetDeckFromResources(imageService.AttackInfos, imageService.DefenceInfos, imageService.DeveloperInfos, imageService.Directions, imageService.EventInfos);
                var turnCount = 0;                
                GameEngine game = new GameEngine(deck, names);
                game.Deal(6);
                while (true)
                {
                    foreach (var bot in bots)
                    {
                        bot.SetBotHand(game.Players[bot.Id].Hand);
                        List<int> cardIds = bot.SelectCards(game.GetPlayersScores());
                        List<GameCard> selectedCards = game.Players[bot.Id].Hand.Cards.Where(card => cardIds.Contains(card.Id)).ToList();
                        selectedCards.ForEach(card => card.State = CardState.Used );
                    }
                    game.TargetCards();
                    game.Turn();
                    turnCount += 1;
                    if (game.Win)
                    {
                        results[bots.Count()] += turnCount;
                        results[game.Winner - 1] += 1;
                        break;
                    }
                    foreach (var bot in bots)
                    {
                        game.Players[bot.Id].Hand.SelectedCardQueue.Clear();
                    }

                    game.Deal(3);
                }
            }
            /*foreach (var res in results)
            {
                Console.WriteLine(res);
            }*/
            foreach (var bot in bots)
            {
                Console.WriteLine("Strategy:" + bot.Name + "wins:" + results[bot.Id]);
            }
            Console.WriteLine("turnCount:" + results[bots.Count()]/n);
        }

        /// <summary>
        /// Формирование набора стратегий с разным количеством игроков
        /// </summary>
        private static void FillStrategiesList()
        {
            //strategiesList.Add(new List<ISelectionStrategy> {
            //    possibleStrategy[MONTE_CARLO],
            //    possibleStrategy[RANDOM_STRATEGY]
            //});
            //strategiesList.Add(new List<ISelectionStrategy> {
            //    possibleStrategy[MONTE_CARLO],
            //    possibleStrategy[DEFENCE_STRATEGY],
            //    possibleStrategy[DEVELOP_STRATEGY],
            //});
            strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[MONTE_CARLO],
                possibleStrategy[DEFENCE_STRATEGY],
                possibleStrategy[DEVELOP_STRATEGY],
                possibleStrategy[ATTACK_STRATEGY],
            });
            strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[MONTE_CARLO],
                possibleStrategy[DEFENCE_STRATEGY],
                possibleStrategy[DEVELOP_STRATEGY],
                possibleStrategy[RANDOM_STRATEGY],
                possibleStrategy[ATTACK_STRATEGY],
            });
            strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[MONTE_CARLO],
                possibleStrategy[DEFENCE_STRATEGY],
                possibleStrategy[DEVELOP_STRATEGY],
                possibleStrategy[ATTACK_STRATEGY],
                possibleStrategy[DEFENCE_STRATEGY],
                possibleStrategy[DEVELOP_STRATEGY],
            });
        }

        /// <summary>
        /// Перемешать порядок рассадки игроков
        /// </summary>
        /// <param name="basestrategyList"></param>
        /// <returns></returns>
        public static List<ISelectionStrategy> Shuffle(List<ISelectionStrategy> basestrategyList)
        {
            List<ISelectionStrategy> strategyList = new List<ISelectionStrategy>(basestrategyList);
            Random r = new Random();
            for (int n = strategyList.Count - 1; n > 0; --n)
            {
                int k = r.Next(n + 1);
                if (!(strategyList[k] is MonteCarloSelectStrategy) && !(strategyList[n] is MonteCarloSelectStrategy))
                {
                    ISelectionStrategy temp = strategyList[n];
                    strategyList[n] = strategyList[k];
                    strategyList[k] = temp;
                }
            }
            if (strategiesList.Contains(strategyList))
                return Shuffle(strategyList);
            else
                return strategyList;
        }

        /// <summary>
        /// Проанализировать одну игру
        /// </summary>
        /// <param name="strategyList"></param>
        private static void AnalizeOneGame(List<ISelectionStrategy> strategyList)
        {
            Analizer analizer = new Analizer(strategyList);
            List<AnalizerResult> resultList = new List<AnalizerResult>() { analizer.Run(1) };
            float averageTurnCount = 0;
            int numberOfPlayers = strategyList.Count;
            List<float> averageWins = new List<float>();
            for (int i = 0; i < numberOfPlayers; i++)
            {
                averageWins.Add(0);
            }
            foreach (AnalizerResult analizerResult in resultList)
            {
                averageTurnCount += analizerResult.averageTurnCount;
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    averageWins[i] += analizerResult.averageWins[i];
                }
            }
            averageTurnCount = averageTurnCount / 1;
            for (int i = 0; i < numberOfPlayers; i++)
            {
                averageWins[i] = averageWins[i] / 1;
            }
            Console.WriteLine($" Количество итераций: {1}; Игроков: {numberOfPlayers}; Среднее количество ходов: {averageTurnCount};\n\tСреднее количество выигрышей: \n{WinsToString(resultList, averageWins, strategyList)}");
        }

        /// <summary>
        /// Проанализировать набор стратегий на большом количестве игр параллельно
        /// </summary>
        private static void TestBestStrategy()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Parallel.ForEach(strategiesList, AnalizeGame);
            stopwatch.Stop();
            Console.WriteLine("Заняло времени: " + stopwatch.Elapsed);
        }

        /// <summary>
        /// Анализ ITERATION_COUNT игр. Разбиваем их на кусочки по 1000 и считаем параллельно
        /// </summary>
        /// <param name="strategyList"></param>
        private static void AnalizeGame(List<ISelectionStrategy> strategyList)
        {
            int localIterationCount = ITERATION_COUNT / 50;
            int numberOfPlayers = strategyList.Count;
            List<AnalizerResult> resultList = new List<AnalizerResult>();
            float averageTurnCount = 0;
            List<float> averageWins = new List<float>();
            for (int i = 0; i < numberOfPlayers; i++)
            {
                averageWins.Add(0);
            }
            Parallel.For(0, localIterationCount, action =>
            {
                Analizer analizer = new Analizer(strategyList);
                resultList.Add(analizer.Run(50));
            });
            foreach (AnalizerResult analizerResult in resultList)
            {
                averageTurnCount += analizerResult.averageTurnCount;
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    averageWins[i] += analizerResult.averageWins[i];
                }
            }
            averageTurnCount = averageTurnCount / localIterationCount;
            for (int i = 0; i < numberOfPlayers; i++)
            {
                averageWins[i] = averageWins[i] / localIterationCount;
            }
            Console.WriteLine($"Количество итераций: {localIterationCount * 50}; Игроков: {numberOfPlayers}; Среднее количество ходов: {averageTurnCount};\n\tСреднее количество выигрышей: \n{WinsToString(resultList, averageWins, strategyList)}");
        }

        /// <summary>
        /// Преобразовать список выигрышей игроков в строку
        /// </summary>
        /// <param name="averageWins"></param>
        /// <param name="strategyList"></param>
        /// <returns></returns>
        private static object WinsToString(List<AnalizerResult> analizerResults, List<float> averageWins, List<ISelectionStrategy> strategyList)
        {
            string ans = "";
            if (analizerResults != null)
            {
                analizerResults.ForEach(x => ans += x.MCProbability + "\n____________________\n" + string.Join(",", x.scores) + "\n\n");
            }
            for (int i = 0; i < averageWins.Count; i++)
            {
                float wins = averageWins[i];
                ans += $"\t\t{i + 1} Игрок:" + averageWins[i].ToString() + " " + strategyList[i].Print();

                ans += "\n";
            }
            return ans;
        }
    }
}
