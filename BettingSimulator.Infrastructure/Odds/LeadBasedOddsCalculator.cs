using BettingSimulator.Application.Interfaces;
using BettingSimulator.Domain.Events;
using BettingSimulator.Domain.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainOdds = BettingSimulator.Domain.Common.Odds;

namespace BettingSimulator.Infrastructure.Odds
{
    public sealed class LeadBasedOddsCalculator : IOddsCalculator
    {
        public void RecalculateOdds(SportEvent sportEvent, Market market)
        {
            if (market.Type != MarketType.ThreeWay_1X2)
                return;

            // bazowe prawdopodobieństwa (sumują się do 1)
            decimal pHome = 0.45m;
            decimal pDraw = 0.25m;
            decimal pAway = 0.30m;

            // wpływ przewagi punktowej
            var diff = sportEvent.Score.Home - sportEvent.Score.Away; // dodatni = home prowadzi
            var shift = Math.Clamp(diff, -10, 10) * 0.03m;           // max +-0.30

            pHome = Clamp01(pHome + shift);
            pAway = Clamp01(pAway - shift);

            // draw maleje, gdy różnica rośnie
            pDraw = Clamp01(1m - (pHome + pAway));

            // normalizacja, gdyby clampy coś zepsuły
            var sum = pHome + pDraw + pAway;
            if (sum <= 0m)
            {
                pHome = 0.45m; pDraw = 0.25m; pAway = 0.30m;
                sum = 1m;
            }
            pHome /= sum; pDraw /= sum; pAway /= sum;

            // marża bukmachera (prosta): zmniejszamy prawdopodobieństwa o 5% i renormalizujemy
            const decimal margin = 0.05m;
            pHome *= (1m - margin);
            pDraw *= (1m - margin);
            pAway *= (1m - margin);

            var sum2 = pHome + pDraw + pAway;
            pHome /= sum2; pDraw /= sum2; pAway /= sum2;

            // odds = 1/p
            market.GetSelectionByCode("HOME").UpdateOdds(DomainOdds.FromProbability(pHome));
            market.GetSelectionByCode("DRAW").UpdateOdds(DomainOdds.FromProbability(pDraw));
            market.GetSelectionByCode("AWAY").UpdateOdds(DomainOdds.FromProbability(pAway));
        }

        private static decimal Clamp01(decimal v) => v < 0.01m ? 0.01m : (v > 0.98m ? 0.98m : v);
    }
}
