using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Simulation
{
    public readonly record struct SimulationTime(DateTime Value)
    {
        public SimulationTime Add(TimeSpan delta) => new(Value.Add(delta));
        public override string ToString() => Value.ToString("yyyy-MM-dd HH:mm:ss");
    }

}
