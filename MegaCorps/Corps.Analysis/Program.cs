using Corps.Core.Model.Common;
using Corps.Core.Model.Enums;
using Corps.Core.Model.GameUtils;
using Corps.Server.Hubs;
using Corps.Server.Services;
using MegaCorps.Core.Model;
using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using MegaCorps.Core.Model.GameUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spire.Xls;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text.Json;

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
            //FillStrategiesList();
            //Добавляем ещё несколько вариантов игр, где игроки сидят в разном порядке
            //strategiesList.Add(Shuffle(strategiesList[0]));
            //strategiesList.Add(Shuffle(strategiesList[1]));
            //strategiesList.Add(Shuffle(strategiesList[3]));
            //strategiesList.Add(Shuffle(strategiesList[3]));
            //strategiesList.Add(Shuffle(strategiesList[3]));
            //strategiesList.Add(Shuffle(strategiesList[2]));
            //strategiesList.Add(Shuffle(strategiesList[4]));
            //strategiesList.Add(Shuffle(strategiesList[4]));
            //strategiesList.Add(Shuffle(strategiesList[4]));
            //strategiesList.Add(Shuffle(strategiesList[4]));
            //for (int i = 0; i < 1; i++)
            //{
            //    AnalizeOneGame(strategiesList[2]);
            //}
            //Console.WriteLine("Конец");
            /*for (int i = 0; i < ITERATION_COUNT; i++)
            {
                AnalizeOneGame(strategiesList[0]);
            }
            //TestBestStrategy();
            Console.ReadKey();*/

            SelectedCardsConcurrentDictionary previousResults = new SelectedCardsConcurrentDictionary();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var botsSettings = new List<List<Bot>>()
            {
                /*new List<Bot> { new Bot(0, BotStrategy.Random, "random"), new Bot(1, BotStrategy.Random, "random") },
                new List<Bot> { new Bot(0, BotStrategy.MonteCarlo, "monteCarlo", previousResults), new Bot(1, BotStrategy.Random, "random") },
                new List<Bot> { new Bot(0, BotStrategy.MonteCarlo, "monteCarlo", previousResults), new Bot(1, BotStrategy.Aggressive, "aggressive") },
                new List<Bot> { new Bot(0, BotStrategy.MonteCarlo, "monteCarlo", previousResults), new Bot(1, BotStrategy.Defensive, "defensive") },
                new List<Bot> { new Bot(0, BotStrategy.MonteCarlo, "monteCarlo", previousResults), new Bot(1, BotStrategy.Researchive, "researchive") },
                new List<Bot> { new Bot(0, BotStrategy.MonteCarlo, "monteCarlo", previousResults), new Bot(1, BotStrategy.Clever, "clever") }*/
                /*new List<Bot> { new Bot(0, BotStrategy.Random, "random"), new Bot(1, BotStrategy.Random, "random") },
                new List<Bot> { new Bot(0, BotStrategy.Random, "random"), new Bot(1, BotStrategy.Aggressive, "aggressive") },
                new List<Bot> { new Bot(0, BotStrategy.Random, "random"), new Bot(1, BotStrategy.Defensive, "defensive") },
                new List<Bot> { new Bot(0, BotStrategy.Random, "random"), new Bot(1, BotStrategy.Researchive, "researchive") },
                new List<Bot> { new Bot(0, BotStrategy.Random, "random"), new Bot(1, BotStrategy.Clever, "clever") },*/
                new List<Bot> { new Bot(0, BotStrategy.Random, "random"), new Bot(1, BotStrategy.Random, "random") },
                new List<Bot> { new Bot(0, BotStrategy.MonteCarlo, "monteCarlo", previousResults), new Bot(1, BotStrategy.Clever, "clever") }
            };
            int maxCards = 100;
            //mod 7, mod 10
            var deckSettings = new List<int[]>()
            {

            };
            fillDeckSettings(maxCards, deckSettings);

            var tasks = 10;
            var iterations = 20;

            List<(int, List<Dictionary<int, int>>)> resultCollection = new List<(int, List<Dictionary<int, int>>)>();
            foreach (var (deckSetting, j) in deckSettings.Select((deckSetting, j) => (deckSetting, j)))
            {
                foreach (var (botSetting, i) in botsSettings.Select((botSetting, i) => (botSetting, i)))
                {
                    try
                    {
                        resultCollection.Add((j, Simulate(tasks, iterations, botSetting, deckSetting)));
                        Console.WriteLine("----------------------------------------");
                        Console.WriteLine($"Simulation with botSetting: {i}, deckSetting: {j} computed.");
                        Console.WriteLine("----------------------------------------");
                        if (resultCollection.Last().Item2.Any(x => x.Values.Sum() - x.Values.Last() < iterations / 2))
                        {
                            Console.WriteLine($"Game with deckSetting: {j} unconfident.");
                            resultCollection.RemoveAll(a => a.Item1 == j);
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error occurred: ", e.ToString());
                    }
                }
            }
            Workbook workbook = new Workbook();
            foreach (var (value, i) in deckSettings.Select((value, i) => (value, i)))
            {
                Worksheet worksheet;
                try
                {
                    worksheet = workbook.Worksheets[i];
                }
                catch
                {
                    workbook.CreateEmptySheet($"Sheet{i+1}");
                    worksheet = workbook.Worksheets[i];
                }
                worksheet.Range[1, 1].Value = "Deck size";
                worksheet.Range[1, 2].Value2 = value[0];
                worksheet.Range[1, 3].Value = "Attack cards count";
                worksheet.Range[1, 4].Value2 = value[1];
                worksheet.Range[1, 5].Value = "Defence cards count";
                worksheet.Range[1, 6].Value2 = value[2];
                worksheet.Range[1, 7].Value = "Number of games";
                worksheet.Range[1, 8].Value2 = tasks*iterations;
            }
            var startWith = 0;
            var page = 0;
            foreach (var (value, i) in resultCollection.Select((value, i) => (value, i)))
            {
                if (page != value.Item1)
                {
                    startWith = 0;
                    page = value.Item1;
                }
                Worksheet worksheet = workbook.Worksheets[value.Item1];
                Console.WriteLine("deckSetting: " + value.Item1);
                var result = new Dictionary<int, int>();
                foreach (var results in value.Item2.First())
                {
                    result.Add(results.Key, 0);
                }
                foreach (var results in value.Item2)
                {
                    foreach (var r in results)
                    {
                        result[r.Key] += r.Value;
                    }
                }
                for (int j = 0; j < result.Count - 1; j++)
                {
                    Console.WriteLine("Strategy: " + botsSettings[i % botsSettings.Count()][j].Name + " Wins:" + result[j]);
                    worksheet.Range[3 + startWith + 2 * j, 1].Value = "Strategy";
                    worksheet.Range[3 + startWith + 2 * j, 2].Value2 = botsSettings[i % botsSettings.Count()][j].Name;
                    worksheet.Range[4 + startWith + 2 * j, 1].Value = "Wins";
                    worksheet.Range[4 + startWith + 2 * j, 2].Value2 = result[j];
                }
                Console.WriteLine("TurnCount: " + result[result.Count - 1] / iterations / tasks);
                worksheet.Range[3 + startWith + (result.Count - 1) * 2, 1].Value = "TurnCount";
                worksheet.Range[3 + startWith + (result.Count - 1) * 2, 2].Value2 = result[result.Count - 1] / iterations / tasks;
                startWith = startWith + (result.Count) * 2;
                Console.WriteLine("----------------------------------------------------------");
            }
            workbook.SaveToFile("SimResults.xlsx", ExcelVersion.Version2016);
            stopwatch.Stop();
            Console.WriteLine("Execution complited in " + stopwatch.ElapsedMilliseconds / 1000 + " seconds.");

            /*List<string> resultStr = new List<string>();
            foreach (var result in previousResults)
            {
                var str = "\"( ";
                str += result.Key.Item1.ToString();
                str += ", ";
                str += result.Key.Item2.ToString();
                str += ", ";
                var tmp = "[ ";
                foreach (var sc in result.Key.Item3)
                {
                    if (sc == result.Key.Item3.Last())
                        tmp += sc.ToString() + " ";
                    else
                        tmp += sc.ToString() + ", ";
                }
                tmp += "]";
                str += tmp;
                str += ",";
                str += result.Key.Item4.ToString();
                str += " )\": ";
                var tmp2 = "( [ ";
                foreach (var ch in result.Value.Item1)
                {
                    if (ch == result.Value.Item1.Last())
                        tmp2 += ch.ToString() + " ";
                    else
                        tmp2 += ch.ToString() + ", ";
                }
                str += tmp2;
                str += "], ";
                str += result.Value.Item2.ToString();
                str += " )";
                resultStr.Add(str);
            }
            foreach (var r in resultStr)
            {
                Console.WriteLine(r);
            }*/
            Console.ReadKey();
        }

        private static void fillDeckSettings(int maxCards, List<int[]> deckSettings)
        {
            for (int i = 7; i < maxCards; i = i + 7)
            {
                for (int j = 10; j < maxCards - i; j = j + 10)
                {
                    deckSettings.Add(new int[3] { maxCards, i, j });
                }
            }
        }

        private static List<Dictionary<int, int>> Simulate(int tasks, int iterations, List<Bot> bots, int[] cards, int maxTurnCount = 100)
        {
            List<Dictionary<int, int>> resultCollection = new List<Dictionary<int, int>>();
            var res = Parallel.For(0, tasks, iterator =>
            {
                var results = new Dictionary<int, int>();
                List<string> names = new List<string>();
                foreach (var bot in bots)
                {
                    names.Add(bot.Name);
                    results.Add(bot.Id, 0);
                }
                results.Add(bots.Count(), 0);
                int n = iterations;
                DynamicDeckBuilder deckBuilder = new DynamicDeckBuilder(cards[0], cards[1], cards[2]);
                for (int i = 0; i < n; i++)
                {
                    Deck deck = deckBuilder.GetDeck();
                    Deck defaultDeck = deck.Copy();
                    var turnCount = 0;
                    GameEngine game = new GameEngine(deck, names);
                    game.Deal(6);
                    while (true)
                    {
                        if (turnCount < maxTurnCount)
                        {
                            foreach (var bot in bots)
                            {
                                List<int> cardIds;
                                if (bot.Strategy == BotStrategy.MonteCarlo)
                                    cardIds = bot.SelectCards(game.GetPlayersScores(), game.Players[bot.Id].Hand.Copy(), defaultDeck.Copy());
                                else
                                    cardIds = bot.SelectCards(game.GetPlayersScores(), game.Players[bot.Id].Hand.Copy(), null);
                                //List<GameCard> selectedCards = game.Players[bot.Id].Hand.Cards.Where(card => cardIds.Contains(card.Id)).ToList();
                                game.Players[bot.Id].Hand.Cards.Where(card => cardIds.Contains(card.Id)).ToList().ForEach(card => card.State = CardState.Used);
                            }
                            game.TargetCards();
                            game.Turn();
                            turnCount += 1;
                            if (game.Win)
                            {
                                results[bots.Count()] += turnCount;
                                results[game.Winner - 1] += 1;
                                Console.WriteLine("Task: " + iterator + " Finished: " + Math.Round(((double)i + 1) / ((double)n) * 100, 2) + "%");
                                break;
                            }

                            game.Deal(3);
                            defaultDeck.PlayedCards = deck.PlayedCards;
                        }
                        else
                        {
                            Console.WriteLine("MaxTurnCount on Task: " + iterator);
                            break;
                        }
                    }
                }
                Console.WriteLine("Ready: " + iterator);
                resultCollection.Add(results);
            });
            return resultCollection;
        }

        /// <summary>
        /// Формирование набора стратегий с разным количеством игроков
        /// </summary>
        private static void FillStrategiesList()
        {
            strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[MONTE_CARLO],
                possibleStrategy[RANDOM_STRATEGY]
            });
            /*strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[RANDOM_STRATEGY],
                possibleStrategy[MONTE_CARLO]
            });
            strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[MONTE_CARLO],
                possibleStrategy[RANDOM_STRATEGY],
                possibleStrategy[RANDOM_STRATEGY]
            });
            strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[RANDOM_STRATEGY],
                possibleStrategy[MONTE_CARLO],
                possibleStrategy[RANDOM_STRATEGY]
            });
            strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[RANDOM_STRATEGY],
                possibleStrategy[RANDOM_STRATEGY],
                possibleStrategy[MONTE_CARLO]
            });*/
            //strategiesList.Add(new List<ISelectionStrategy> {
            //    possibleStrategy[MONTE_CARLO],
            //    possibleStrategy[DEFENCE_STRATEGY],
            //    possibleStrategy[DEVELOP_STRATEGY],
            //});
            /*strategiesList.Add(new List<ISelectionStrategy> {
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
            });*/
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
