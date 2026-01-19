using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BettingSimulator.Domain.Bets;
using BettingSimulator.Domain.Events;

namespace BettingSimulator.Application.Interfaces
{
    public interface ISettlementService
    {
        /// Rozlicza kupon na podstawie końcowego wyniku wydarzenia.
        bool IsWinningBet(BetSlip betSlip, SportEvent sportEvent);
    }

}
