using BettingSimulator.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Markets
{
    public class Selection : Entity
    {
        public string Code { get; }     // np. "HOME", "DRAW", "AWAY", "OVER", "UNDER"
        public string Name { get; }     // np. "Home Win"
        public Odds CurrentOdds { get; private set; }

        public Selection(string code, string name, Odds initialOdds, Guid? id = null) : base(id)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("Selection code cannot be empty.");
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Selection name cannot be empty.");

            Code = code.Trim().ToUpperInvariant();
            Name = name.Trim();
            CurrentOdds = initialOdds;
        }

        public void UpdateOdds(Odds newOdds)
        {
            CurrentOdds = newOdds;
        }
    }

}
