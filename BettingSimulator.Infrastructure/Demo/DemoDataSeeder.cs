using BettingSimulator.Domain.Events;
using BettingSimulator.Domain.Markets;
using System;
using System.Collections.Generic;
using DomainOdds = BettingSimulator.Domain.Common.Odds;

namespace BettingSimulator.Infrastructure.Demo
{
    public static class DemoDataSeeder
    {
        private static readonly Random _random = new Random();

       
        private static readonly List<string> _teamNames = new()
        {
            "Real Madrid", "FC Barcelona", "Manchester City", "Liverpool",
            "Bayern Munich", "Dortmund", "Juventus", "AC Milan",
            "PSG", "Marsylia", "Legia Warszawa", "Lech Poznań",
            "Arsenal", "Chelsea", "Napoli", "Inter"
        };

        public static SportEvent CreateRandomMatch(DateTime now)
        {
            // losuj druzyny
            string teamA = _teamNames[_random.Next(_teamNames.Count)];
            string teamB;
            do
            {
                teamB = _teamNames[_random.Next(_teamNames.Count)];
            } while (teamA == teamB);

            // losowanie za ile zacznie sie mecz
            var delaySeconds = _random.Next(10, 180);
            var startTime = now.AddSeconds(delaySeconds);

            var ev = new SportEvent(
                name: $"{teamA} vs {teamB}",
                startTime: startTime,
                plannedDuration: TimeSpan.FromMinutes(10) //10 minut w symulacji
            );

            // losowe kursy
            AddRandomOdds(ev);

            return ev;
        }

        private static void AddRandomOdds(SportEvent ev)
        {
            double homeOddsVal = 1.20 + (_random.NextDouble() * (4.50 - 1.20));
            homeOddsVal = Math.Round(homeOddsVal, 2);

            double pHome = 1.0 / homeOddsVal;
            double totalP = 1.06;
            double remainingP = Math.Max(0.10, totalP - pHome);

            double pDraw, pAway;

            if (homeOddsVal < 2.0)
            {
                double drawShare = 0.60 + (_random.NextDouble() * 0.15);
                pDraw = remainingP * drawShare;
                pAway = remainingP - pDraw;
            }
            else
            {
                pDraw = remainingP * 0.55;
                pAway = remainingP * 0.45;
            }

            if (pDraw < pAway) (pDraw, pAway) = (pAway, pDraw);

            var home = new Selection("HOME", "1", new DomainOdds((decimal)homeOddsVal));
            var draw = new Selection("DRAW", "X", new DomainOdds((decimal)Math.Round(1.0 / pDraw, 2)));
            var away = new Selection("AWAY", "2", new DomainOdds((decimal)Math.Round(1.0 / pAway, 2)));

            var market = new Market(
                type: MarketType.ThreeWay_1X2,
                name: "Wynik Meczu (1X2)",
                selections: new[] { home, draw, away });

            ev.AddMarket(market);
        }
    }
}