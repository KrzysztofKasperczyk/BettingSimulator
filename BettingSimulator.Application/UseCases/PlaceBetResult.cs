using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Application.UseCases
{
    public sealed record PlaceBetResult(
        Guid BetSlipId,
        decimal OddsAtPlacement,
        decimal PotentialPayout,
        decimal NewBalance
    );

}
