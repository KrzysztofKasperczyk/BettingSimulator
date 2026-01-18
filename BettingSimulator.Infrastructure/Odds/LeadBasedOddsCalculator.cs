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
        // marża bukmacherska
        private const decimal Margin = 0.06m;

        // jak silnie wpływa 1 punkt różnicy (bazowo)
        private const double ScoreK = 0.55;

        // jak mocno czas wzmacnia wpływ wyniku (koncówka)
        private const double TimePower = 2.3;
        private const double TimeBoostMax = 4.0;

        // remis: bazowa część (z opening odds) + regulacja wynikiem i czasem
        private const double DrawDiffPenalty = 0.60;  // im większa różnica, tym mniejszy remis
        private const double DrawLateBoost = 0.70;    // w końcówce przy małej różnicy remis rośnie

        public void RecalculateOdds(SportEvent sportEvent, Market market)
        {
            if (market.Type != MarketType.ThreeWay_1X2)
                return;

            var homeSel = market.GetSelectionByCode("HOME");
            var drawSel = market.GetSelectionByCode("DRAW");
            var awaySel = market.GetSelectionByCode("AWAY");

            // 1) Opening probabilities (STAŁE!)
            var (p0Home, p0Draw, p0Away) = BaseFromOpeningOdds(homeSel, drawSel, awaySel);

            // 2) Czas 0..1 (TickClock -> LastSimTime)
            var t = GetProgress01(sportEvent);

            // 3) Wpływ czasu: na początku mały, pod koniec duży
            var timeBoost = 1.0 + (TimeBoostMax - 1.0) * Math.Pow(t, TimePower);

            // 4) Wynik: diff > 0 = home prowadzi, diff < 0 = away prowadzi
            var diff = sportEvent.Score.Home - sportEvent.Score.Away;
            var absDiff = Math.Abs(diff);

            // 5) Najpierw wyliczamy "tilt" wyniku w przestrzeni logit (stabilne i monotoniczne)
            // home prowadzi -> tilt dodatni -> rośnie P(home), maleje P(away)
            var tilt = diff * ScoreK * timeBoost;

            // 6) Remis:
            // start = p0Draw, spada z abs(diff), w końcówce rośnie gdy różnica mała
            var pDraw = (double)p0Draw;
            pDraw *= Math.Exp(-DrawDiffPenalty * absDiff);

            var closeness = 1.0 / (1.0 + absDiff); // 1.0 dla 0, 0.5 dla 1, 0.33 dla 2...
            pDraw *= 1.0 + DrawLateBoost * closeness * Math.Pow(t, TimePower);

            // clamp, żeby remis nie znikł ani nie zdominował
            pDraw = Clamp(pDraw, 0.03, 0.60);

            // 7) Pozostałe prawdopodobieństwo dzielimy HOME/AWAY.
            // Klucz: startowy "bias" z opening odds + tilt z wyniku.
            var remaining = 1.0 - pDraw;

            // opening bias w logit: jeśli home faworyt, to dodatni
            var baseBias = LogitShare((double)p0Home, (double)p0Away);

            // final logit = baseBias + tilt
            var finalLogit = baseBias + tilt;

            // udział home w puli (home+away), bez remisu
            var homeShareNoDraw = Sigmoid(finalLogit);
            var awayShareNoDraw = 1.0 - homeShareNoDraw;

            var pHome = remaining * homeShareNoDraw;
            var pAway = remaining * awayShareNoDraw;

            // 8) Normalizacja (sanity)
            var sum = pHome + pDraw + pAway;
            pHome /= sum; pDraw /= sum; pAway /= sum;

            // --- REGUŁA PRODUKTOWA: gdy ktoś prowadzi, remis ma być WYRAŹNIE bardziej prawdopodobny niż wygrana przegrywającego ---
            // czyli pDraw >= pTrailingWin * (1 + gapFactor)
            const double gapFactor = 0.35;  // 0.35 = remis min. 35% bardziej prawdopodobny niż comeback przegrywającego
            const double pMin = 0.01;       // minimalna masa na każde zdarzenie (żeby nie wyzerować)

            if (diff > 0) // home prowadzi => przegrywa away
            {
                var target = Math.Min(0.60, pAway * (1.0 + gapFactor));
                if (pDraw < target)
                {
                    var need = target - pDraw;

                    // Zabieramy najpierw z prowadzącego (home), bo to najbardziej logiczne:
                    // podnosząc draw, "karzemy" niepewność co do utrzymania prowadzenia.
                    var takeFromHome = Math.Min(pHome - pMin, need);
                    pHome -= takeFromHome;
                    pDraw += takeFromHome;

                    need = target - pDraw;
                    if (need > 0)
                    {
                        // jeśli nadal brakuje, dobieramy z przegrywającego (away), ale zostawiamy minimum
                        var takeFromAway = Math.Min(pAway - pMin, need);
                        pAway -= takeFromAway;
                        pDraw += takeFromAway;
                    }
                }
            }
            else if (diff < 0) // away prowadzi => przegrywa home
            {
                var target = Math.Min(0.60, pHome * (1.0 + gapFactor));
                if (pDraw < target)
                {
                    var need = target - pDraw;

                    var takeFromAway = Math.Min(pAway - pMin, need);
                    pAway -= takeFromAway;
                    pDraw += takeFromAway;

                    need = target - pDraw;
                    if (need > 0)
                    {
                        var takeFromHome = Math.Min(pHome - pMin, need);
                        pHome -= takeFromHome;
                        pDraw += takeFromHome;
                    }
                }
            }

            // ponowna normalizacja po regule
            var sum2 = pHome + pDraw + pAway;
            pHome /= sum2; pDraw /= sum2; pAway /= sum2;

            // 9) Overround (marża)
            var over = 1.0 + (double)Margin;

            homeSel.UpdateOdds(ToOdds(pHome, over));
            drawSel.UpdateOdds(ToOdds(pDraw, over));
            awaySel.UpdateOdds(ToOdds(pAway, over));
        }

        // --- Helpers ---

        private static (decimal pHome, decimal pDraw, decimal pAway) BaseFromOpeningOdds(Selection home, Selection draw, Selection away)
        {
            var pH = 1m / home.OpeningOdds.Value;
            var pD = 1m / draw.OpeningOdds.Value;
            var pA = 1m / away.OpeningOdds.Value;

            var sum = pH + pD + pA;
            if (sum <= 0m) return (0.40m, 0.25m, 0.35m);

            pH /= sum; pD /= sum; pA /= sum;
            return (pH, pD, pA);
        }

        private static double GetProgress01(SportEvent ev)
        {
            if (ev.LiveStartedAt is null) return 0.0;

            var elapsed = (ev.LastSimTime - ev.LiveStartedAt.Value).TotalSeconds;
            var total = Math.Max(1.0, ev.PlannedDuration.TotalSeconds);
            return Clamp(elapsed / total, 0.0, 1.0);
        }

        private static DomainOdds ToOdds(double p, double over)
        {
            p = Clamp(p, 0.01, 0.99);
            var pWithMargin = Clamp(p * over, 0.01, 0.99);

            var odds = 1.0 / pWithMargin;
            return new DomainOdds((decimal)Math.Round(odds, 2, MidpointRounding.AwayFromZero));
        }

        // logit udziału home vs away: ln(pHome/pAway)
        private static double LogitShare(double pHome, double pAway)
        {
            pHome = Clamp(pHome, 0.001, 0.999);
            pAway = Clamp(pAway, 0.001, 0.999);
            return Math.Log(pHome / pAway);
        }

        private static double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
        private static double Clamp(double v, double min, double max) => v < min ? min : (v > max ? max : v);
    }

}
