using BettingSimulator.Application.Interfaces;
using BettingSimulator.Domain.Events;
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

        public void RecalculateForEvent(SportEvent sportEvent)
        {
            foreach (var market in sportEvent.Markets)
            {
                _calculator.RecalculateOdds(sportEvent, market);
            }
        }
    }

}
