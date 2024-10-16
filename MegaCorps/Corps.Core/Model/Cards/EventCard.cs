﻿namespace MegaCorps.Core.Model.Cards
{
    public class EventCard : GameCard
    {
        public EventCard(int id) : base(id)
        {
        }
    }

    public class ScoreEventCard : EventCard
    {
        public int Power { get; set; }
        public ScoreEventCard(int id, int power) : base(id)
        {
            Power = power;
        }
    }

    public class NeighboursEventCards : EventCard
    {
        public int Power { get; set; }
        public NeighboursEventCards(int id, int power) : base(id)
        {
            Power = power;
        }
    }

    public class SwapEventCard : EventCard
    {
        public SwapEventCard(int id) : base(id)
        {
        }
    }

    public class AllLosingCard : EventCard
    {
        public int Power { get; set; }
        public AllLosingCard(int id, int power) : base(id)
        {
            Power = power;
        }
    }

}
