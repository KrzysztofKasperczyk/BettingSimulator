using BettingSimulator.Application.Interfaces;
using BettingSimulator.Domain.Bets;
using BettingSimulator.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Application.Services
{
    public sealed class SettlementService : ISettlementService
    {
        public bool IsWinningBet(BetSlip betSlip, SportEvent sportEvent)
        {
            // MVP: single only
            var leg = betSlip.Legs.Single();

            // MVP: tylko rynek 1X2 po kodach HOME/DRAW/AWAY
            var home = sportEvent.Score.Home;
            var away = sportEvent.Score.Away;

            var resultCode =
                home > away ? "HOME" :
                home < away ? "AWAY" :
                "DRAW";

            return leg.SelectionCode == resultCode;
        }
    }

}
