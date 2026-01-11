using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Simulation
{
    public class SimulationClock
    {
        public SimulationTime Now { get; private set; }
        public int SpeedMultiplier { get; private set; } = 1;

        public SimulationClock(SimulationTime startTime, int speedMultiplier = 1)
        {
            if (speedMultiplier <= 0) throw new ArgumentOutOfRangeException(nameof(speedMultiplier));
            Now = startTime;
            SpeedMultiplier = speedMultiplier;
        }

        public void SetSpeed(int multiplier)
        {
            if (multiplier <= 0) throw new ArgumentOutOfRangeException(nameof(multiplier));
            SpeedMultiplier = multiplier;
        }

        /// <summary>
        /// Przesuwa czas symulacji o delta * SpeedMultiplier.
        /// </summary>
        public void Tick(TimeSpan realDelta)
        {
            if (realDelta < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(realDelta));
            var simulatedDelta = TimeSpan.FromTicks(realDelta.Ticks * SpeedMultiplier);
            Now = Now.Add(simulatedDelta);
        }
    }

}
