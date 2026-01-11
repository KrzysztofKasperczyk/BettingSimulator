using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Application.UseCases
{
    public sealed record PlaceBetRequest(
        Guid UserId,
        string UserName,
        Guid EventId,
        Guid MarketId,
        string SelectionCode,
        decimal StakeAmount,
        string Currency = "PLN"
    );

}
