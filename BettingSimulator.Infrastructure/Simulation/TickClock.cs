using BettingSimulator.Application.Interfaces;
using System;

namespace BettingSimulator.Infrastructure.Simulation
{
    public sealed class TickClock : IClock
    {
        public DateTime Now { get; private set; }

        public double SpeedMultiplier { get; set; } = 1.0;

        public TickClock(DateTime start)
        {
            Now = start;
        }

        public void Advance(TimeSpan realDelta)
        {
            if (realDelta < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(realDelta));

            long simulatedTicks = (long)(realDelta.Ticks * SpeedMultiplier);
            var simulatedDelta = TimeSpan.FromTicks(simulatedTicks);

            Now = Now.Add(simulatedDelta);
        }
    }
}