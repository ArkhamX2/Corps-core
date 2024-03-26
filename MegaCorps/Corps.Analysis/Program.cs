using MegaCorps.Core.Model;
using MegaCorps.Core.Model.Cards;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Corps.Analysis
{
    internal class Program
    {
        private const string BEST_STRATEGY = "BestStrategy";
        private const string RANDOM_STRATEGY = "RandomStrategy";
        private const string ATTACK_STRATEGY = "AttackStrategy";
        private const string DEFENCE_STRATEGY = "DefenseStrategy";
        private const string DEVELOP_STRATEGY = "DeveloperStrategy";
        private const int ITERATION_COUNT = 100000;
        private static Dictionary<string, ISelectionStrategy> possibleStrategy = new Dictionary<string, ISelectionStrategy>() {
            {BEST_STRATEGY ,new BestSelectStrategy() },
            {RANDOM_STRATEGY ,new RandomSelectStrategy() },
            {ATTACK_STRATEGY ,new AgressiveSelectStrategy() },
            {DEFENCE_STRATEGY ,new DefenciveSelectStrategy() },
            {DEVELOP_STRATEGY ,new DevelopSelectStrategy() }
        };
        private static List<List<ISelectionStrategy>> strategiesList = new List<List<ISelectionStrategy>> { };

        static void Main(string[] args)
        {
            FillStrategiesList();
            strategiesList.Add(Shuffle(strategiesList[4]));
            strategiesList.Add(Shuffle(strategiesList[4]));
            strategiesList.Add(Shuffle(strategiesList[4]));
            strategiesList.Add(Shuffle(strategiesList[4]));
            strategiesList.Add(Shuffle(strategiesList[4]));
            //AnalizeOneGame(strategiesList[4]);
            TestBestStrategy();
            Console.ReadKey();
        }

        private static void FillStrategiesList()
        {
            strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[ATTACK_STRATEGY],
                possibleStrategy[DEFENCE_STRATEGY]
            });
            strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[ATTACK_STRATEGY],
                possibleStrategy[DEFENCE_STRATEGY],
                possibleStrategy[DEVELOP_STRATEGY],
            });
            strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[ATTACK_STRATEGY],
                possibleStrategy[DEFENCE_STRATEGY],
                possibleStrategy[DEVELOP_STRATEGY],
                possibleStrategy[RANDOM_STRATEGY],
            });
            strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[ATTACK_STRATEGY],
                possibleStrategy[DEFENCE_STRATEGY],
                possibleStrategy[DEVELOP_STRATEGY],
                possibleStrategy[RANDOM_STRATEGY],
                possibleStrategy[ATTACK_STRATEGY],
            });
            strategiesList.Add(new List<ISelectionStrategy> {
                possibleStrategy[ATTACK_STRATEGY],
                possibleStrategy[DEFENCE_STRATEGY],
                possibleStrategy[DEVELOP_STRATEGY],
                possibleStrategy[ATTACK_STRATEGY],
                possibleStrategy[DEFENCE_STRATEGY],
                possibleStrategy[DEVELOP_STRATEGY],
            });
        }
        public static List<ISelectionStrategy> Shuffle(List<ISelectionStrategy> basestrategyList)
        {
            List<ISelectionStrategy> strategyList = new List<ISelectionStrategy>(basestrategyList);
            Random r = new Random();
            for (int n = strategyList.Count - 1; n > 0; --n)
            {
                int k = r.Next(n + 1);
                ISelectionStrategy temp = strategyList[n];
                strategyList[n] = strategyList[k];
                strategyList[k] = temp;
            }
            if (strategiesList.Contains(strategyList))
            return Shuffle(strategyList);
            else
            return strategyList;
        }

        private static void AnalizeOneGame(List<ISelectionStrategy> strategyList)
        {
            Analizer analizer = new Analizer(strategyList);
            analizer.Run(1);
        }

        private static void TestBestStrategy()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Parallel.ForEach(strategiesList, AnalizeGame);
            stopwatch.Stop();
            Console.WriteLine("Заняло времени: "+stopwatch.Elapsed);
        }

        private static void AnalizeGame(List<ISelectionStrategy> strategyList)
        {
            int localIterationCount = ITERATION_COUNT/1000;
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
                resultList.Add(analizer.Run(1000));
            });
            foreach (AnalizerResult analizerResult in resultList)
            {
                averageTurnCount += analizerResult.averageTurnCount;
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    averageWins[i] += analizerResult.averageWins[i];
                }
            }
            averageTurnCount=averageTurnCount / localIterationCount;
            for (int i = 0; i < numberOfPlayers; i++)
            {
                averageWins[i] = averageWins[i] / localIterationCount;
            }
            Console.WriteLine($"Количество итераций: {localIterationCount * 1000}; Игроков: {numberOfPlayers}; Среднее количество ходов: {averageTurnCount};\n\tСреднее количество выигрышей: \n{WinsToString(averageWins, strategyList)}");
        }
        private static object WinsToString(List<float> averageWins, List<ISelectionStrategy> strategyList)
        {
            string ans = "";
            for (int i = 0; i < averageWins.Count; i++)
            {
                float wins = averageWins[i];
                ans += $"\t\t{i + 1} Игрок:" + averageWins[i].ToString() + " " + strategyList[i].Print() + "\n";
            }
            return ans;
        }
    }
}
