﻿using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;
using MegaCorps.Core.Model.GameUtils;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model
{
    public class GameEngine
    {
        private Deck _deck;
        public Deck Deck { get => _deck; set => _deck = value; }
        public List<Player> Players { get => _players; set => _players = value; }
        public bool Win { get => _win; set => _win = value; }
        public int Winner { get => _winner; set => _winner = value; }
        public int NumberOfPlayers { get; }

        private int _winner;

        private bool _win;

        private List<Player> _players;

        public GameEngine()
        {
            Deck = DeckBuilder.GetDeck();
            Deck.Shuffle();
            Players = new List<Player> { new Player(), new Player(), new Player(), new Player() };
            _win = false;
        }

        public GameEngine(int numberOfPlayers)
        {
            NumberOfPlayers = numberOfPlayers;
            Deck = DeckBuilder.GetDeck();
            Deck.Shuffle();
            Players = Enumerable.Repeat(new Player(), NumberOfPlayers).ToList();
            _win = false;
        }

        public void Deal(int dealCount)
        {
            List<List<GameCard>> hands = Deck.Deal(dealCount, NumberOfPlayers);

            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Hand.Cards.AddRange(hands[i]);
            }
        }

        public void Reset()
        {
            Deck = DeckBuilder.GetDeck();
            Deck.Shuffle();
            Players = Enumerable.Repeat(new Player(), NumberOfPlayers).ToList();
            _win = false;
        }

        public List<List<GameCard>> GetPlayersHands()
        {
            List<List<GameCard>> hands = new List<List<GameCard>>();

            for (int i = 0; i < Players.Count; i++)
            {
                hands.Add(Players[i].Hand.Cards);
            }
            
            return hands;
        }

        public void Turn()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Targeted = Players[i == 0 ? Players.Count() - 1 : i - 1].Hand.Cards.Where((card) => card.State == CardState.Used && card.Color == "Red").ToList();
                Players[i].PlayHand();
            }
            for (int i = 0; i < Players.Count; i++)
            {
                Deck.PlayedCards.AddRange(Players[i].Hand.Cards.Where((card) => card.State == CardState.Used));
                Players[i].Hand.Cards.RemoveAll((card) => card.State == CardState.Used);
                Players[i].Targeted.Clear();
                Players[i].Selected.Clear();
            }

            Win = Players.Any(player => player.Score >= 10);
            Winner = Players.FindIndex(player => player.Score == Players.Max((item) => item.Score))+1;

        }

        public void SelectCards(List<List<int>> hands)
        {
            for (int i = 0; i < hands.Count; i++)
            {
                foreach (var card in hands[i])
                {
                    Players[i].Hand.Cards[card].State = CardState.Used;
                }
            }
        }
    }
}
