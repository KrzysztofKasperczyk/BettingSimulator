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

        //przechodzi przez wszystkie ryynki i przelicza kursy
        public void RecalculateForEvent(SportEvent sportEvent)
        {
            foreach (var market in sportEvent.Markets)
            {
                
                if (market.State == MarketState.Settled || market.State == MarketState.Closed)
                    continue;

                _calculator.RecalculateOdds(sportEvent, market);
            }
        }
    }

}
