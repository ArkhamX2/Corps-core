using Corps.Core.Model.Enums;
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
        BotStrategies BotStrategies = new BotStrategies();
        public BotStrategy Strategy { get; set; }
        private SelectedCardsConcurrentDictionary? PreviousResults;
        public Bot(int id, BotStrategy strategy, string username) : base(id) 
        {
            Id = id;
            Strategy = strategy;
            Name = username;
        }
        public Bot(int id, BotStrategy strategy, string username, SelectedCardsConcurrentDictionary previousResults) : this(id, strategy, username) => PreviousResults = previousResults;
        public List<int> SelectCards(List<int>? Scores, PlayerHand? hand, Deck? deck, int? maxTurnCount)
        {
            if (deck != null)
                PrepareDeck(deck);
            var res = new List<int>();
            switch (Strategy)
            {
                case BotStrategy.Random:
                    res = BotStrategies.Random(new List<int>(), hand);
                    break;
                case BotStrategy.Aggressive:
                    res = BotStrategies.Aggressive(new List<int>(), hand);
                    break;
                case BotStrategy.Defensive:
                    res = BotStrategies.Defensive(new List<int>(), hand);
                    break;
                case BotStrategy.Researchive:
                    res = BotStrategies.Researchive(new List<int>(), hand);
                    break;
                case BotStrategy.Clever:
                    res = BotStrategies.Clever(new List<int>(), hand, Scores, Id);
                    break;
                case BotStrategy.MonteCarlo:
                    if (PreviousResults == null)
                        res = BotStrategies.Montecarlo(new List<int>(), hand, Scores, Id, deck, 100, turnLimit: maxTurnCount != null ? (int)maxTurnCount : 100);
                    else
                        res = BotStrategies.Montecarlo(new List<int>(), hand, Scores, Id, deck, 100, PreviousResults, turnLimit: maxTurnCount != null ? (int)maxTurnCount : 100);
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

    public class BotStrategies
    {
        private int WinScore = 10;
        private int MaxSelectedCards = 3;

        public BotStrategies()
        {

        }

        public BotStrategies(int winScore, int maxSelectedCards)
        {
            WinScore = winScore;
            MaxSelectedCards = maxSelectedCards;
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
                    if (target.Value >= (6 + potentionalSelectedAttackCards.Count()) && potentionalSelectedAttackCards.Count < 3)
                        if (target.Key == leftTargetId && target.Key == rightTargetId)
                        {
                            foreach (var card in attackCards)
                                if ((!potentionalSelectedAttackCards.Contains(card)) && ((card as AttackCard).Direction == CardDirection.All || (card as AttackCard).Direction == CardDirection.Allbutnotme || (card as AttackCard).Direction == CardDirection.Left || (card as AttackCard).Direction == CardDirection.Right))
                                    potentionalSelectedAttackCards.Add(card);
                        }
                        else
                        {
                            if (target.Key != leftTargetId && target.Key != rightTargetId)
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
                }
                foreach (var card in potentionalSelectedAttackCards)
                    selectedCardsIds.Add(card.Id);
                if (selectedCardsIds.Count() == 3)
                    return selectedCardsIds;
            }
            if (developerCards.Count() < 3)
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

        public List<int> Montecarlo(List<int> selectedCardsIds, PlayerHand Hand, List<int> Scores, int BotId, Deck deck, int depth, int turnLimit = 100)
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
                                var cards = bot.SelectCards(localGame.GetPlayersScores(), localGame.Players[bot.Id].Hand.Copy(), null, null);
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
                                var turn = 1;
                                GameEngine gameEngine = localGame.Copy();
                                gameEngine.Deck.Shuffle();
                                while (!gameEngine.Win)
                                {
                                    if (turn != turnLimit)
                                    {
                                        foreach (var bot in bots)
                                        {
                                            var cards = bot.SelectCards(gameEngine.GetPlayersScores(), gameEngine.Players[bot.Id].Hand.Copy(), null, null);
                                            foreach (var card in cards)
                                            {
                                                gameEngine.Players[bot.Id].Hand.Cards[gameEngine.Players[bot.Id].Hand.Cards.FindIndex(x => x.Id == card)].State = CardState.Used;
                                            }
                                        }
                                        gameEngine.TargetCards();
                                        gameEngine.Turn();
                                        gameEngine.Deal(3);
                                        turn++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (gameEngine.Winner != -1)
                                {
                                    if (gameEngine.Winner - 1 == BotId)
                                    {
                                        localScore += 1;
                                    }
                                    /*var scores = gameEngine.GetPlayersScores();
                                    localScore += (double)(scores[BotId] - (scores.Sum() - scores[BotId])) / depth;*/
                                }
                                else
                                {
                                    var scores = gameEngine.GetPlayersScores();
                                    localScore += (double)(scores[BotId] - (scores.Sum() - scores[BotId])) / 10;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (localGame.Winner - 1 == BotId)
                            {
                                localScore = depth;
                                var scores = localGame.GetPlayersScores();
                                localScore += (scores[BotId] - (scores.Sum() - scores[BotId]));
                            }/*
                            else
                            {
                                var scores = localGame.GetPlayersScores();
                                localScore = (scores[BotId] - (scores.Sum() - scores[BotId]));
                            }*/
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
            return selectedCardsIds;
        }

        public List<int> Montecarlo(List<int> selectedCardsIds, PlayerHand Hand, List<int> Scores, int BotId, Deck deck, int depth, SelectedCardsConcurrentDictionary previousResults, int turnLimit = 30)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            if (!previousResults.concurrentDictionary.ContainsKey((BotId, Hand, Scores, deck)))
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
                                    var cards = bot.SelectCards(localGame.GetPlayersScores(), localGame.Players[bot.Id].Hand.Copy(), null, null);
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
                                    var turn = 1;
                                    GameEngine gameEngine = localGame.Copy();
                                    gameEngine.Deck.Shuffle();
                                    while (!gameEngine.Win)
                                    {
                                        if (turn != turnLimit)
                                        {
                                            foreach (var bot in bots)
                                            {
                                                var cards = bot.SelectCards(gameEngine.GetPlayersScores(), gameEngine.Players[bot.Id].Hand.Copy(), null, null);
                                                foreach (var card in cards)
                                                {
                                                    gameEngine.Players[bot.Id].Hand.Cards[gameEngine.Players[bot.Id].Hand.Cards.FindIndex(x => x.Id == card)].State = CardState.Used;
                                                }
                                            }
                                            gameEngine.TargetCards();
                                            gameEngine.Turn();
                                            gameEngine.Deal(3);
                                            turn++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    if (gameEngine.Winner != -1)
                                    {
                                        if (gameEngine.Winner - 1 == BotId)
                                            localScore += 1;
                                    }
                                    else
                                    {
                                        var scores = gameEngine.GetPlayersScores();
                                        localScore += (scores[BotId] - (scores.Sum() - scores[BotId]) / (scores.Count-1))  / 10;
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                if (localGame.Winner - 1 == BotId)
                                {
                                    localScore = depth;
                                    var scores = localGame.GetPlayersScores();
                                    localScore += (scores[BotId] - (scores.Sum() - scores[BotId]));
                                }
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
                //Console.WriteLine("WinChance: " + winPercent + "%");
                previousResults.concurrentDictionary.TryAdd((BotId, Hand, Scores, deck),(selectedCardsIds, winPercent));
                //stopwatch.Stop();
                //Console.WriteLine("Strategy: MontecarloAlt ElapsedSeconds: " + stopwatch.ElapsedMilliseconds / 1000 + " Chances:"+(double)maxLocalScore);
            }
            else
            {
                selectedCardsIds = previousResults.concurrentDictionary[(BotId, Hand, Scores, deck)].Item1;
            }
            return selectedCardsIds;
        }

        public List<int> Neural(List<int> selectedCardsIds, PlayerHand Hand)
        {

            return selectedCardsIds;
        }
    }
}
