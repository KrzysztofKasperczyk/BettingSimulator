using BettingSimulator.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Infrastructure.Simulation
{
    public sealed class SystemClockAdapter : IClock
    {
        public DateTime Now => DateTime.Now;
    }

}
