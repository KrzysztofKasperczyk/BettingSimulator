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
        /// <summary>
        /// Rozlicza kupon na podstawie końcowego wyniku wydarzenia.
        /// Zwraca true jeśli wygrany, false jeśli przegrany (void/cancel obsłużymy później).
        /// </summary>
        bool IsWinningBet(BetSlip betSlip, SportEvent sportEvent);
    }

}
