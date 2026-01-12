using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.UI.ViewModels
{
    public sealed class BetListItemViewModel
    {
        public Guid BetSlipId { get; init; }
        public string EventName { get; init; } = "";
        public string SelectionCode { get; init; } = "";
        public decimal Odds { get; init; }
        public decimal Stake { get; init; }
        public decimal PotentialPayout { get; init; }
        public string Status { get; init; } = "";
        public DateTime PlacedAt { get; init; }
    }

}
