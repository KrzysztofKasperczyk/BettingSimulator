using BettingSimulator.Domain.Events;
using BettingSimulator.Domain.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using DomainOdds = BettingSimulator.Domain.Common.Odds;

namespace BettingSimulator.Infrastructure.Demo
{
    public static class DemoDataSeeder
    {
        private static readonly Random _random = new Random();

        public static SportEvent CreateSingleDemoEvent(DateTime now)
        {
            var ev = new SportEvent(
                name: "Lions vs Tigers",
                startTime: now.AddMinutes(1),
                plannedDuration: TimeSpan.FromMinutes(10)
            );

            // 1. Losujemy kurs na HOME (1.20 - 5.00)
            double homeOddsVal = 1.20 + (_random.NextDouble() * (5.00 - 1.20));
            homeOddsVal = Math.Round(homeOddsVal, 2);

            double pHome = 1.0 / homeOddsVal;
            double totalP = 1.06; // 6% marży
            double remainingP = Math.Max(0.10, totalP - pHome);

            // 2. Rozdzielamy pozostałe szanse
            // Zmieniamy logikę: Remis musi dostać solidną część, 
            // aby kurs był niższy niż na underdoga.
            double pDraw, pAway;

            if (homeOddsVal < 2.0)
            {
                // Scenariusz: Home jest wyraźnym faworytem. 
                // Remis musi być bardziej prawdopodobny niż wygrana Away.
                // pDraw będzie stanowić np. 60-70% pozostałej puli.
                double drawShare = 0.60 + (_random.NextDouble() * 0.10);
                pDraw = remainingP * drawShare;
                pAway = remainingP - pDraw;
            }
            else
            {
                // Scenariusz: Mecz wyrównany lub Home jest underdogiem.
                // Dzielimy pozostałe P po równo z lekkim losowaniem, 
                // ale pilnujemy, by pDraw >= pAway.
                pDraw = remainingP * 0.55;
                pAway = remainingP * 0.45;
            }

            // 3. Ostateczna weryfikacja hierarchii kursów (Kurs: Draw < Away <=> P: Draw > Away)
            if (pDraw < pAway)
            {
                var temp = pDraw;
                pDraw = pAway;
                pAway = temp;
            }

            // 4. Tworzenie selekcji
            var home = new Selection("HOME", "Home Win", new DomainOdds((decimal)homeOddsVal));
            var draw = new Selection("DRAW", "Draw", new DomainOdds((decimal)Math.Round(1.0 / pDraw, 2)));
            var away = new Selection("AWAY", "Away Win", new DomainOdds((decimal)Math.Round(1.0 / pAway, 2)));

            var market = new Market(
                type: MarketType.ThreeWay_1X2,
                name: "Match Result (1X2)",
                selections: new[] { home, draw, away });

            ev.AddMarket(market);
            return ev;
        }
    }
}