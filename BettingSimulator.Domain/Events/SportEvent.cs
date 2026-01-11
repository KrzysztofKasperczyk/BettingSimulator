using BettingSimulator.Domain.Common;
using BettingSimulator.Domain.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Events
{
    public class SportEvent : Entity
    {
        public string Name { get; }
        public DateTime StartTime { get; }
        public TimeSpan PlannedDuration { get; }              // NOWE
        public DateTime PlannedEndTime => StartTime + PlannedDuration;  // NOWE

        public EventState State { get; private set; }
        public Score Score { get; private set; }

        public DateTime? LiveStartedAt { get; private set; }
        public DateTime? FinishedAt { get; private set; }

        public IReadOnlyList<Market> Markets => _markets;
        private readonly List<Market> _markets = new();

        public SportEvent(string name, DateTime startTime, TimeSpan plannedDuration, Guid? id = null) : base(id)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Event name cannot be empty.");
            if (plannedDuration <= TimeSpan.Zero)
                throw new DomainException("PlannedDuration must be greater than zero.");

            Name = name.Trim();
            StartTime = startTime;
            PlannedDuration = plannedDuration;

            State = EventState.Scheduled;
            Score = new Score(0, 0);
        }

        public void StartAt(DateTime startedAt)
        {
            if (State != EventState.Scheduled)
                throw new DomainException("Only scheduled events can be started.");

            State = EventState.Live;
            LiveStartedAt = startedAt;
        }

        public void FinishAt(DateTime finishedAt)
        {
            if (State != EventState.Live)
                throw new DomainException("Only live events can be finished.");

            State = EventState.Finished;
            FinishedAt = finishedAt;

            foreach (var market in _markets)
            {
                if (market.State is MarketState.Open or MarketState.Suspended)
                    market.Close();
            }
        }

        public void UpdateScore(Score newScore)
        {
            if (State != EventState.Live)
                throw new DomainException("Score can be updated only during live event.");

            Score = newScore;
        }

        public void AddMarket(Market market)
        {
            _markets.Add(market);
        }
    }
}
