using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Common
{
    public static class OddsLimits
    {
        public const decimal Min = 1.05m;
        public const decimal Max = 50.00m;

        public static bool IsWithinRange(decimal odds) => odds >= Min && odds <= Max;
    }

}
