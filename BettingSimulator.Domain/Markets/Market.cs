using BettingSimulator.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Markets
{
    public class Market : Entity
    {
        public MarketType Type { get; }
        public string Name { get; }
        public MarketState State { get; private set; }

        public IReadOnlyList<Selection> Selections => _selections;
        private readonly List<Selection> _selections = new();

        public Market(MarketType type, string name, IEnumerable<Selection> selections, Guid? id = null) : base(id)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Market name cannot be empty.");

            Type = type;
            Name = name.Trim();

            var list = selections?.ToList() ?? throw new DomainException("Selections cannot be null.");
            if (list.Count < 2)
                throw new DomainException("Market must have at least 2 selections.");

            // Unikalne Code
            var dup = list.GroupBy(s => s.Code).FirstOrDefault(g => g.Count() > 1);
            if (dup is not null)
                throw new DomainException($"Duplicate selection code: {dup.Key}");

            _selections.AddRange(list);

            State = MarketState.Open;
        }

        public void Open()
        {
            if (State is MarketState.Settled or MarketState.Voided)
                throw new DomainException("Cannot open a settled/voided market.");

            State = MarketState.Open;
        }

        public void Suspend()
        {
            if (State != MarketState.Open)
                throw new DomainException("Only open market can be suspended.");

            State = MarketState.Suspended;
        }

        public void Close()
        {
            if (State is MarketState.Settled or MarketState.Voided)
                throw new DomainException("Cannot close a settled/voided market.");

            State = MarketState.Closed;
        }

        public void Settle()
        {
            if (State != MarketState.Closed)
                throw new DomainException("Market must be closed before settling.");

            State = MarketState.Settled;
        }

        public void Void()
        {
            if (State == MarketState.Settled)
                throw new DomainException("Cannot void a settled market.");

            State = MarketState.Voided;
        }

        public Selection GetSelectionByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("Selection code cannot be empty.");

            var normalized = code.Trim().ToUpperInvariant();
            var sel = _selections.FirstOrDefault(s => s.Code == normalized);
            return sel ?? throw new DomainException($"Selection not found: {normalized}");
        }
    }

}
