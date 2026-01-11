using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BettingSimulator.Domain.Common;

namespace BettingSimulator.Domain.Bets
{
    public class BetSlip : Entity
    {
        public Guid UserId { get; }
        public Money Stake { get; private set; }              // stawka
        public BetStatus Status { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime? PlacedAt { get; private set; }
        public DateTime? SettledAt { get; private set; }

        public IReadOnlyList<BetLeg> Legs => _legs;
        private readonly List<BetLeg> _legs = new();

        public BetSlip(Guid userId, DateTime createdAt, Guid? id = null) : base(id)
        {
            UserId = userId;
            CreatedAt = createdAt;

            Stake = Money.Zero();
            Status = BetStatus.Draft;
        }

        public void AddLeg(BetLeg leg)
        {
            if (Status != BetStatus.Draft)
                throw new DomainException("Cannot add legs after bet slip is placed.");

            _legs.Add(leg);
        }

        public void SetStake(Money stake)
        {
            if (Status != BetStatus.Draft)
                throw new DomainException("Cannot change stake after bet slip is placed.");

            if (stake.Amount <= 0m)
                throw new DomainException("Stake must be greater than zero.");

            Stake = stake;
        }

        public Odds GetCombinedOdds()
        {
            if (_legs.Count == 0)
                throw new DomainException("Bet slip has no legs.");

            decimal product = 1m;
            foreach (var leg in _legs)
                product *= leg.OddsAtPlacement.Value;

            return new Odds(product);
        }

        public Money GetPotentialPayout()
        {
            var combinedOdds = GetCombinedOdds();
            return Stake * combinedOdds.Value;
        }

        public void Place(DateTime placedAt)
        {
            if (Status != BetStatus.Draft)
                throw new DomainException("Bet slip is not in Draft state.");

            if (_legs.Count == 0)
                throw new DomainException("Cannot place bet slip with no legs.");

            if (Stake.Amount <= 0m)
                throw new DomainException("Cannot place bet slip with zero stake.");

            Status = BetStatus.Placed;
            PlacedAt = placedAt;
        }

        public void SettleWon(DateTime settledAt)
        {
            if (Status != BetStatus.Placed)
                throw new DomainException("Only placed bets can be settled.");

            Status = BetStatus.SettledWon;
            SettledAt = settledAt;
        }

        public void SettleLost(DateTime settledAt)
        {
            if (Status != BetStatus.Placed)
                throw new DomainException("Only placed bets can be settled.");

            Status = BetStatus.SettledLost;
            SettledAt = settledAt;
        }

        public void Cancel(DateTime when)
        {
            if (Status is BetStatus.SettledWon or BetStatus.SettledLost)
                throw new DomainException("Cannot cancel a settled bet.");

            Status = BetStatus.Cancelled;
            SettledAt = when;
        }

        public void Void(DateTime when)
        {
            if (Status is BetStatus.SettledWon or BetStatus.SettledLost)
                throw new DomainException("Cannot void a settled bet.");

            Status = BetStatus.Voided;
            SettledAt = when;
        }
    }

}
