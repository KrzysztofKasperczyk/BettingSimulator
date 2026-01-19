using BettingSimulator.Application.Interfaces;
using BettingSimulator.Domain.Events;
using BettingSimulator.Domain.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Application.Services
{
    public sealed class OddsService
    {
        private readonly IOddsCalculator _calculator;

        public OddsService(IOddsCalculator calculator)
        {
            _calculator = calculator;
        }

        //Przejście przez wszystkie rynki danego meczu i nakazanie ich przeliczenia
        public void RecalculateForEvent(SportEvent sportEvent)
        {
            foreach (var market in sportEvent.Markets)
            {
                // Zabezpieczenie
                if (market.State == MarketState.Settled || market.State == MarketState.Closed)
                    continue;

                _calculator.RecalculateOdds(sportEvent, market);
            }
        }
    }

}
