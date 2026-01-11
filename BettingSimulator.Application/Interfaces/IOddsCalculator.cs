using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BettingSimulator.Domain.Events;
using BettingSimulator.Domain.Markets;

namespace BettingSimulator.Application.Interfaces
{
    public interface IOddsCalculator
    {
        /// <summary>
        /// Aktualizuje kursy dla rynku (modyfikuje Selection.CurrentOdds).
        /// </summary>
        void RecalculateOdds(SportEvent sportEvent, Market market);
    }

}
