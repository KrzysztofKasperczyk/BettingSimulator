using BettingSimulator.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Infrastructure.Simulation
{
    public sealed class TickClock : IClock
    {
        public DateTime Now { get; private set; }

        public TickClock(DateTime start)
        {
            Now = start;
        }

        public void Advance(TimeSpan delta)
        {
            if (delta < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delta));
            Now = Now.Add(delta);
        }
    }

}
