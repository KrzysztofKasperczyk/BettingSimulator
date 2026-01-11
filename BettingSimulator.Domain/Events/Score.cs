using BettingSimulator.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Events
{
    public readonly record struct Score
    {
        public int Home { get; }
        public int Away { get; }

        public Score(int home, int away)
        {
            if (home < 0 || away < 0)
                throw new DomainException("Score cannot be negative.");

            Home = home;
            Away = away;
        }

        public override string ToString() => $"{Home}:{Away}";
    }
}
