using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Bets
{
    public enum BetStatus
    {
        Draft,
        Placed,
        SettledWon,
        SettledLost,
        Cancelled,
        Voided
    }

}
