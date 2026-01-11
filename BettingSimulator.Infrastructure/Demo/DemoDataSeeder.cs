using BettingSimulator.Domain.Events;
using BettingSimulator.Domain.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Infrastructure.Demo
{
    public static class DemoDataSeeder
    {
        public static SportEvent CreateSingleDemoEvent(DateTime now)
        {
            // Start za 2 minuty (symulacja czasu dojdzie później)
            var ev = new SportEvent(
                name: "Lions vs Tigers",
                startTime: now.AddMinutes(2));

            // Rynek 1X2
            var home = new Selection("HOME", "Home Win", new BettingSimulator.Domain.Common.Odds(2.10m));
            var draw = new Selection("DRAW", "Draw", new BettingSimulator.Domain.Common.Odds(3.30m));
            var away = new Selection("AWAY", "Away Win", new BettingSimulator.Domain.Common.Odds(3.60m));

            var market = new Market(
                type: MarketType.ThreeWay_1X2,
                name: "Match Result (1X2)",
                selections: new[] { home, draw, away });

            ev.AddMarket(market);

            return ev;
        }
    }
}
