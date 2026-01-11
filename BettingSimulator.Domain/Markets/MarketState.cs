using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Markets
{
    public enum MarketState
    {
        Open,
        Suspended,
        Closed,
        Settled,
        Voided
    }

}
