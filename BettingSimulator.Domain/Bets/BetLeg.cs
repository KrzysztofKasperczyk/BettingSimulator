using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BettingSimulator.Domain.Common;
using BettingSimulator.Domain.Markets;

namespace BettingSimulator.Domain.Bets
{
    public class BetLeg : Entity
    {
        public Guid EventId { get; }
        public Guid MarketId { get; }
        public Guid SelectionId { get; }

        public string SelectionCode { get; }
        public string SelectionName { get; }

        public Odds OddsAtPlacement { get; }

        public BetLeg(Guid eventId, Market market, Selection selection, Odds oddsAtPlacement, Guid? id = null) : base(id)
        {
            EventId = eventId;
            MarketId = market.Id;
            SelectionId = selection.Id;

            SelectionCode = selection.Code;
            SelectionName = selection.Name;

            OddsAtPlacement = oddsAtPlacement;
        }
    }

}
