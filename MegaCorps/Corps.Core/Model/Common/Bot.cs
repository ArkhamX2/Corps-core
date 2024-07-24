using MegaCorps.Core.Model;
using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using MegaCorps.Core.Model.GameUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Corps.Core.Model.Common
{
    public class Bot : Player
    {
        Strategies strategies = new Strategies();
        public int StrategyId { get; set; }
        private ConcurrentDictionary<(int, PlayerHand, List<int>, Deck), (List<int>, double)>? PreviousResults;
        public Bot(int id, int strategy, string username) : base(id) 
        {
            Id = id;
            StrategyId = strategy;
            Name = username;
        }
        public Bot(int id, int strategy, string username, ConcurrentDictionary<(int, PlayerHand, List<int>, Deck), (List<int>,double)> previousResults) : this(id, strategy, username) => PreviousResults = previousResults;
        public List<int> SelectCards(List<int>? Scores, PlayerHand? hand, Deck? deck)
        {
            if (deck != null)
                PrepareDeck(deck);
            var res = new List<int>();
            switch (StrategyId)
            {
                case 0:
                    res = strategies.Random(new List<int>(), hand);
                    break;
                case 1:
                    res = strategies.Aggressive(new List<int>(), hand);
                    break;
                case 2:
                    res = strategies.Defensive(new List<int>(), hand);
                    break;
                case 3:
                    res = strategies.Researchive(new List<int>(), hand);
                    break;
                case 4:
                    res = strategies.Clever(new List<int>(), hand, Scores, Id);
                    break;
                case 5:
                    //res = strategies.MinMax(new List<int>(), hand, Scores, Id, Deck);
                    break;
                case 6:
                    res = strategies.Montecarlo(new List<int>(), hand, Scores, Id, deck, 4).Item1;
                    break;
                case 7:
                    res = strategies.MonecarloAlt(new List<int>(), hand, Scores, Id, deck, 1000, PreviousResults);
                    break;
                case 8:
                    res = strategies.Neural(new List<int>(), hand);
                    break;
            }
            return res;
        }

        private void PrepareDeck(Deck deck)
        {
            foreach (var p in deck.PlayedCards)
            {
                deck.UnplayedCards.Remove(deck.UnplayedCards.Find((a)=>a.Id==p.Id));
            }
        }
    }

    public class Strategies
    {
        private int WinScore = 10;
        private int MaxSelectedCards = 3;

        public Strategies()
        {

        }

        public List<int> Random(List<int> selectedCardsIds, PlayerHand Hand)
        {            
            int idSum = 0;
            foreach (var card in Hand.Cards)
            {
                idSum += card.Id;
            }
            var rand = new Random(idSum+(int)DateTime.Now.TimeOfDay.TotalMilliseconds);

            while (selectedCardsIds.Count < MaxSelectedCards)
            {
                int id = rand.Next(0, Hand.Cards.Count);
                if (!selectedCardsIds.Contains(Hand.Cards[id].Id))
                    selectedCardsIds.Add(Hand.Cards[id].Id);
            }
            return selectedCardsIds;
        }
        public List<int> Aggressive(List<int> selectedCardsIds, PlayerHand Hand)
        {
            List<GameCard> attackCards = Hand.Cards.Where((card) => card is AttackCard).ToList();
            if (attackCards.Count > 3)
            {
                while (selectedCardsIds.Count < MaxSelectedCards)
                {
                    int maxDamage = -1;
                    int id = -1;
                    foreach (var card in attackCards)
                    {
                        if (!selectedCardsIds.Contains(card.Id))
                        {
                            if (maxDamage < (card as AttackCard).Damage)
                            {
                                maxDamage = (card as AttackCard).Damage;
                                id = card.Id;
                            }
                        }
                    }
                    selectedCardsIds.Add(id);
                }
            }
            else
            {
                foreach (var card in attackCards)
                {
                    selectedCardsIds.Add(card.Id);
                    if (selectedCardsIds.Count == MaxSelectedCards)
                        return selectedCardsIds;
                }
                selectedCardsIds = Random(selectedCardsIds, Hand);
            }
            return selectedCardsIds;
        }
        public List<int> Defensive(List<int> selectedCardsIds, PlayerHand Hand)
        {
            List<GameCard> defenseCards = Hand.Cards.Where((card) => card is DefenceCard).ToList();
            if (defenseCards.Count > 3)
            {
                List<AttackType> attack_types = new List<AttackType>();
                while (selectedCardsIds.Count < MaxSelectedCards)
                {
                    int maxCount = -1;
                    int id = -1;
                    foreach (var card in defenseCards)
                    {
                        if (!selectedCardsIds.Contains(card.Id))
                        {
                            int counter = 0;
                            foreach (AttackType attackType in (card as DefenceCard).AttackTypes)
                                if (!attack_types.Contains(attackType))
                                    counter++;
                            if (maxCount < counter)
                            {
                                maxCount = counter;
                                id = card.Id;
                                foreach (AttackType attackType in (card as DefenceCard).AttackTypes)
                                    attack_types.Add(attackType);
                            }
                        }
                    }
                    selectedCardsIds.Add(id);
                }
            }
            else
            {
                foreach (var card in defenseCards)
                {
                    selectedCardsIds.Add(card.Id);
                    if (selectedCardsIds.Count == MaxSelectedCards)
                        return selectedCardsIds;
                }                
                selectedCardsIds = Random(selectedCardsIds, Hand);
            }
            return selectedCardsIds;
        }
        public List<int> Researchive(List<int> selectedCardsIds, PlayerHand Hand)
        {
            List<GameCard> developerCards = Hand.Cards.Where((card) => card is DeveloperCard).ToList();
            if (developerCards.Count > 3)
            {
                while (selectedCardsIds.Count < MaxSelectedCards)
                {
                    int maxDevelopmentPoint = -1;
                    int id = -1;
                    foreach (var card in developerCards)
                    {
                        if (!selectedCardsIds.Contains(card.Id))
                        {
                            if (maxDevelopmentPoint < (card as DeveloperCard).DevelopmentPoint)
                            {
                                maxDevelopmentPoint = (card as DeveloperCard).DevelopmentPoint;
                                id = card.Id;
                            }
                        }
                    }
                    selectedCardsIds.Add(id);
                }
            }
            else
            {
                foreach (var card in developerCards)
                {
                    selectedCardsIds.Add(card.Id);
                    if (selectedCardsIds.Count == MaxSelectedCards)
                        return selectedCardsIds;
                }
                selectedCardsIds = Random(selectedCardsIds, Hand);
            }
            return selectedCardsIds;
        }
        public List<int> Clever(List<int> selectedCardsIds, PlayerHand Hand, List<int> Scores, int BotId)
        {
            List<GameCard> attackCards = Hand.Cards.Where((card) => card is AttackCard).ToList();
            List<GameCard> defenseCards = Hand.Cards.Where((card) => card is DefenceCard).ToList();
            List<GameCard> developerCards = Hand.Cards.Where((card) => card is DeveloperCard).ToList();
            if (developerCards.Count > 0)
            {
                List<GameCard> potentionalSelectedDeveloperCards = new List<GameCard>();
                foreach (var card in developerCards)
                {
                    if (potentionalSelectedDeveloperCards.Count < 3)
                        potentionalSelectedDeveloperCards.Add(card);
                    else
                    {
                        foreach (var c in potentionalSelectedDeveloperCards)
                            if ((c as DeveloperCard).DevelopmentPoint < (card as DeveloperCard).DevelopmentPoint)
                            {
                                potentionalSelectedDeveloperCards[potentionalSelectedDeveloperCards.IndexOf(c)] = card;
                                break;
                            }
                    }
                }
                int maxDevelopmentPoint = 0;
                foreach (var card in potentionalSelectedDeveloperCards)
                    maxDevelopmentPoint += (card as DeveloperCard).DevelopmentPoint;
                if (maxDevelopmentPoint + Scores[BotId] >= WinScore)
                {
                    foreach (var card in potentionalSelectedDeveloperCards)
                        selectedCardsIds.Add(card.Id);
                    return selectedCardsIds;
                }
            }
            if (attackCards.Count > 0)
            {
                var scores = new Dictionary<int, int>();
                for (int i = 0; i < Scores.Count; i++)
                {
                    if (i != BotId)
                        scores.Add(i, Scores[i]);
                }
                var priorityTargets = scores.ToList();
                priorityTargets.Sort((a, b) => a.Value.CompareTo(b.Value));
                var leftTargetId = BotId == 0 ? Scores.Count() - 1 : BotId - 1;
                var rightTargetId = BotId == Scores.Count() - 1 ? 0 : BotId + 1;
                var potentionalSelectedAttackCards = new List<GameCard>();
                foreach (var target in priorityTargets)
                {
                    if (target.Value >= (7 + potentionalSelectedAttackCards.Count()) && potentionalSelectedAttackCards.Count < 3)
                        if (target.Key != leftTargetId || target.Key != rightTargetId)
                        {
                            foreach (var card in attackCards)
                                if ((!potentionalSelectedAttackCards.Contains(card)) && ((card as AttackCard).Direction == CardDirection.All || (card as AttackCard).Direction == CardDirection.Allbutnotme))
                                    potentionalSelectedAttackCards.Add(card);
                        }
                        else
                        {
                            if (target.Key == leftTargetId)
                            {
                                foreach (var card in attackCards)
                                    if ((!potentionalSelectedAttackCards.Contains(card)) && ((card as AttackCard).Direction == CardDirection.All || (card as AttackCard).Direction == CardDirection.Allbutnotme || (card as AttackCard).Direction == CardDirection.Left))
                                        potentionalSelectedAttackCards.Add(card);
                            }
                            else
                            {
                                foreach (var card in attackCards)
                                    if ((!potentionalSelectedAttackCards.Contains(card)) && ((card as AttackCard).Direction == CardDirection.All || (card as AttackCard).Direction == CardDirection.Allbutnotme || (card as AttackCard).Direction == CardDirection.Right))
                                        potentionalSelectedAttackCards.Add(card);
                            }
                        }
                }
                foreach (var card in potentionalSelectedAttackCards)
                    selectedCardsIds.Add(card.Id);
                if (selectedCardsIds.Count() == 3)
                    return selectedCardsIds;
            }
            if (developerCards.Count() < 3 && Scores[BotId] == 1)
            {
                foreach (var card in attackCards)
                {
                    selectedCardsIds.Add(card.Id);
                    if (selectedCardsIds.Count() == 3)
                        return selectedCardsIds;
                }
            }
            selectedCardsIds = Defensive(selectedCardsIds, Hand);
            return selectedCardsIds;
        }
        public (List<int>, PlayerHand, List<int>, int, Deck, int, GameEngine, double) Montecarlo(List<int> selectedCardsIds, PlayerHand Hand, List<int> Scores, int BotId, Deck deck, int depth, GameEngine Game = null, double localScore = 0)
        {
            List<string> names = new List<string>();
            for (int i = 0; i < Scores.Count(); i++)
            {
                names.Add(i.ToString());
            }
            List<Bot> bots = new List<Bot>();
            for (int i = 0; i < Scores.Count(); i++)
            {
                bots.Add(new Bot(i, 4, names[i]));
                bots[i].Score = Scores[i];
            }
            if (depth > 0) 
            {
                GameEngine game;
                double maxLocalScore = double.MinValue;
                if (Game == null)
                {
                    game = new GameEngine(deck, names);
                    game.DealExcept(6, BotId, Hand);
                    foreach (var bot in bots)
                    {
                        game.Players[bot.Id].Score = bot.Score;
                    }
                    Game = game.Copy();
                }
                for (int i = 0; i < Hand.Cards.Count(); i++)
                {
                    for (int j = i + 1; j < Hand.Cards.Count(); j++)
                    {
                        for (int k = j + 1; k < Hand.Cards.Count(); k++)
                        {
                            game = Game.Copy();
                            List<int> currentSelectedCardsIds = new List<int> { game.Players[BotId].Hand.Cards[i].Id, game.Players[BotId].Hand.Cards[j].Id, game.Players[BotId].Hand.Cards[k].Id };
                            foreach (var bot in bots)
                            {
                                if (bot.Id != BotId)
                                {
                                    var cards = bot.SelectCards(game.GetPlayersScores(), game.Players[bot.Id].Hand.Copy(), null);
                                    foreach (var card in cards)
                                    {
                                        game.Players[bot.Id].Hand.Cards[game.Players[bot.Id].Hand.Cards.FindIndex(x => x.Id == card)].State = CardState.Used;
                                    }
                                }
                                else
                                {
                                    foreach (var card in currentSelectedCardsIds)
                                    {
                                        game.Players[bot.Id].Hand.Cards[game.Players[bot.Id].Hand.Cards.FindIndex(x => x.Id == card)].State = CardState.Used;
                                    }
                                }
                            }
                            game.TargetCards();
                            game.Turn();
                            game.Deal(3);
                            if (game.Win)
                                if (game.Winner - 1 == BotId)
                                {
                                    return (currentSelectedCardsIds, Hand, game.GetPlayersScores(), BotId, deck.Copy(), 0, game.Copy(), localScore + 1);
                                }
                                else
                                {
                                    return (currentSelectedCardsIds, Hand, game.GetPlayersScores(), BotId, deck.Copy(), 0, game.Copy(), localScore - 1);
                                }
                            else
                            {
                                localScore += (double)(game.GetPlayersScores()[BotId] - (game.GetPlayersScores().Sum() - game.GetPlayersScores()[BotId]) / (bots.Count() - 1)) / (double)(20^4);
                                /*var tmp1 = 0;
                                var tmp2 = 0;
                                foreach (var bot in bots)
                                {
                                    if (bot.Id != BotId)
                                    {
                                        tmp1 += game.Players[BotId].Hand.Cards.Where((card) => card is DeveloperCard).ToList().Sum((a) => (a as DeveloperCard).DevelopmentPoint);
                                    }
                                    else
                                    {
                                        tmp2 += game.Players[BotId].Hand.Cards.Where((card) => card is DeveloperCard).ToList().Sum((a) => (a as DeveloperCard).DevelopmentPoint);
                                    }
                                }
                                localScore += (tmp2 - tmp1 / (bots.Count() - 1));*/
                            }
                            localScore = Montecarlo(new List<int>(), game.Players[BotId].Hand.Copy(), game.GetPlayersScores(), BotId, deck.Copy(), depth - 1, game.Copy(), localScore).Item8;
                            if (localScore > maxLocalScore)
                            {
                                maxLocalScore = localScore;
                                selectedCardsIds = currentSelectedCardsIds;
                            }
                        }
                    }
                }
            }
            return (selectedCardsIds, Hand, Scores, BotId, deck.Copy(), depth, Game, localScore);
        }

        public List<int> MonecarloAlt(List<int> selectedCardsIds, PlayerHand Hand, List<int> Scores, int BotId, Deck deck, int depth, ConcurrentDictionary<(int, PlayerHand, List<int>, Deck), (List<int>, double)> previousResults)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            if (!previousResults.ContainsKey((BotId, Hand, Scores, deck)))
            {
                List<string> names = new List<string>();
                for (int i = 0; i < Scores.Count(); i++)
                {
                    names.Add(i.ToString());
                }
                List<Bot> bots = new List<Bot>();
                for (int i = 0; i < Scores.Count(); i++)
                {
                    if (i != BotId)
                        bots.Add(new Bot(i, 0, names[i]));
                    else
                        bots.Add(new Bot(i, 0, names[i]));
                    bots[i].Score = Scores[i];
                }
                GameEngine game;
                game = new GameEngine(deck.Copy(), names);
                game.Deck.Shuffle();
                game.DealExcept(6, BotId, Hand);
                foreach (var bot in bots)
                {
                    game.Players[bot.Id].Score = bot.Score;
                }
                double maxLocalScore = double.MinValue;
                for (int i = 0; i < Hand.Cards.Count(); i++)
                {
                    for (int j = i + 1; j < Hand.Cards.Count(); j++)
                    {
                        for (int k = j + 1; k < Hand.Cards.Count(); k++)
                        {
                            GameEngine localGame = game.Copy();
                            List<int> currentSelectedCardsIds = new List<int> { localGame.Players[BotId].Hand.Cards[i].Id, localGame.Players[BotId].Hand.Cards[j].Id, localGame.Players[BotId].Hand.Cards[k].Id };
                            foreach (var bot in bots)
                            {
                                if (bot.Id != BotId)
                                {
                                    var cards = bot.SelectCards(localGame.GetPlayersScores(), localGame.Players[bot.Id].Hand.Copy(), null);
                                    foreach (var card in cards)
                                    {
                                        localGame.Players[bot.Id].Hand.Cards[localGame.Players[bot.Id].Hand.Cards.FindIndex(x => x.Id == card)].State = CardState.Used;
                                    }
                                }
                                else
                                {
                                    foreach (var card in currentSelectedCardsIds)
                                    {
                                        localGame.Players[bot.Id].Hand.Cards[localGame.Players[bot.Id].Hand.Cards.FindIndex(x => x.Id == card)].State = CardState.Used;
                                    }
                                }
                            }
                            localGame.TargetCards();
                            localGame.Turn();
                            localGame.Deal(3);
                            double localScore = 0;
                            if (!localGame.Win)
                            {
                                for (int n = 0; n < depth; n++)
                                {
                                    var counter = 0;
                                    GameEngine gameEngine = localGame.Copy();
                                    gameEngine.Deck.Shuffle();
                                    while (!gameEngine.Win)
                                    {
                                        counter++;
                                        foreach (var bot in bots)
                                        {
                                            var cards = bot.SelectCards(gameEngine.GetPlayersScores(), gameEngine.Players[bot.Id].Hand.Copy(), null);
                                            foreach (var card in cards)
                                            {
                                                gameEngine.Players[bot.Id].Hand.Cards[gameEngine.Players[bot.Id].Hand.Cards.FindIndex(x => x.Id == card)].State = CardState.Used;
                                            }
                                        }
                                        gameEngine.TargetCards();
                                        gameEngine.Turn();
                                        gameEngine.Deal(3);
                                    }
                                    if (gameEngine.Winner != -1)
                                    {
                                        //Домножать глубину на очки (чем дальше от начала тем меньше шансов на победу)
                                        if (gameEngine.Winner - 1 == BotId)
                                            localScore += 1;
                                    }
                                }
                            }
                            else
                            {
                                if (localGame.Winner - 1 == BotId)
                                    localScore = depth;
                            }

                            if (localScore > maxLocalScore)
                            {
                                maxLocalScore = localScore;
                                selectedCardsIds = currentSelectedCardsIds;
                            }
                        }
                    }
                }
                var winPercent = Math.Round((double)(maxLocalScore / depth) * 100, 2);
                Console.WriteLine("WinChance: " + winPercent + "%");
                previousResults.TryAdd((BotId, Hand, Scores, deck),(selectedCardsIds, winPercent));
                //stopwatch.Stop();
                //Console.WriteLine("Strategy: MonecarloAlt ElapsedSeconds: " + stopwatch.ElapsedMilliseconds / 1000 + " Chances:"+(double)maxLocalScore);
            }
            else
            {
                selectedCardsIds = previousResults[(BotId, Hand, Scores, deck)].Item1;
            }
            return selectedCardsIds;
        }

        public List<int> MinMax(List<int> selectedCardsIds, PlayerHand Hand, List<int> Scores, int BotId, Deck deck)
        {
            List<List<int>> simScores = new List<List<int>>();
            List<List<int>> simSelectedCardsIds = new List<List<int>>();
            for (int i = 0; i < Hand.Cards.Count(); i++)
            {
                for (int j = i + 2; j < Hand.Cards.Count(); j++)
                {
                    List<int> currentSelectedCardsIds = new List<int> { Hand.Cards[i].Id, Hand.Cards[j -1].Id, Hand.Cards[j].Id };
                    simSelectedCardsIds.Add(currentSelectedCardsIds);
                    simScores.Add(SimulateTurn(currentSelectedCardsIds, Hand, Scores, BotId, deck));
                }
            }
            var worstAverageScore = 10;
            var bestPersonalScore = 0;
            var bestSimId = -1;
            for (int j = 0; j < simScores.Count(); j++) 
            {
                var averageScore = 0;
                var personalScore = 0;
                for (int i = 0; i < Scores.Count(); i++)
                {
                    if (i != BotId)
                    {
                        averageScore += simScores[j][i];
                    }
                    else
                    {
                        personalScore = simScores[j][i];
                    }
                }
                if (personalScore > bestPersonalScore && averageScore < worstAverageScore)
                {
                    bestPersonalScore = personalScore;
                    worstAverageScore = averageScore;
                    bestSimId = j;
                }
            }
            return simSelectedCardsIds[bestSimId];
        }
        private List<int> SimulateTurn(List<int> selectedCardsIds, PlayerHand Hand, List<int> Scores, int BotId, Deck deck)
        {
            List<string> names = new List<string>();
            for (int i = 0; i < Scores.Count(); i++)
            {
                names.Add(i.ToString());
            }
            List<Bot> bots = new List<Bot>();
            for (int i = 0; i < Scores.Count(); i++)
            {
                bots.Add(new Bot(i, 0, names[i]));
            }
            GameEngine game = new GameEngine(deck, names);
            game.DealExcept(6, BotId, Hand);
            foreach (var bot in bots)
            {
                if (bot.Id != BotId)
                {
                    var cards = bot.SelectCards(game.GetPlayersScores(), game.Players[bot.Id].Hand.Copy(), deck.Copy());
                    foreach (var card in cards)
                    {
                        game.Players[bot.Id].Hand.Cards[game.Players[bot.Id].Hand.Cards.FindIndex(x => x.Id == card)].State = CardState.Used;
                    }
                }
                else
                {
                    foreach (var card in selectedCardsIds)
                    {
                        game.Players[bot.Id].Hand.Cards[game.Players[bot.Id].Hand.Cards.FindIndex(x => x.Id == card)].State = CardState.Used;
                    }
                }
            }
            game.TargetCards();
            game.Turn();
            var simScores = game.GetPlayersScores();
            for (int i = 0; i < bots.Count(); i++)
            {
                Scores[i] += simScores[i]-Scores[i];
            }
            return Scores;
        }
        public List<int> Neural(List<int> selectedCardsIds, PlayerHand Hand)
        {

            return selectedCardsIds;
        }
    }
}
