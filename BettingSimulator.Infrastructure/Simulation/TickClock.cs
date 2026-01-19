using BettingSimulator.Application.Interfaces;
using System;

namespace BettingSimulator.Infrastructure.Simulation
{
    public sealed class TickClock : IClock
    {
        public DateTime Now { get; private set; }

        // Domyślnie 1.0 (czas rzeczywisty), ale możesz zmienić na 60, 1000 itd.
        public double SpeedMultiplier { get; set; } = 1.0;

        public TickClock(DateTime start)
        {
            Now = start;
        }

        public void Advance(TimeSpan realDelta)
        {
            if (realDelta < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(realDelta));

            // KLUCZOWA ZMIANA:
            // Czas symulowany = Czas rzeczywisty * Mnożnik
            // Np. 1 sekunda rzeczywista * 60 = 60 sekund symulowanych (1 minuta)
            long simulatedTicks = (long)(realDelta.Ticks * SpeedMultiplier);
            var simulatedDelta = TimeSpan.FromTicks(simulatedTicks);

            Now = Now.Add(simulatedDelta);
        }
    }
}