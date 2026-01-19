using BettingSimulator.Application.Interfaces;
using BettingSimulator.Domain.Events;
using BettingSimulator.Domain.Markets;
using System;
using DomainOdds = BettingSimulator.Domain.Common.Odds;

namespace BettingSimulator.Infrastructure.Odds
{
    public sealed class LeadBasedOddsCalculator : IOddsCalculator
    {
        private const decimal Margin = 0.06m;
        private const double ScoreK = 0.35;
        private const double TimeBoostMax = 2.5;
        private const double TimePower = 1.8;
        private const double DriftPerMinute = 0.007;

        public void RecalculateOdds(SportEvent sportEvent, Market market)
        {
            if (market.Type != MarketType.ThreeWay_1X2)
                return;

            var homeSel = market.GetSelectionByCode("HOME");
            var drawSel = market.GetSelectionByCode("DRAW");
            var awaySel = market.GetSelectionByCode("AWAY");

            // 1) Prawdopodobieństwa bazowe z kursów otwarcia
            var (p0Home, p0Draw, p0Away) = BaseFromOpeningOdds(homeSel, drawSel, awaySel);

            // 2) Czas symulacji
            var t = GetProgress01(sportEvent);
            var timeBoost = 1.0 + (TimeBoostMax - 1.0) * Math.Pow(t, TimePower);

            // 3) Analiza wyniku i oporu faworyta
            var diff = sportEvent.Score.Home - sportEvent.Score.Away;
            var absDiff = Math.Abs(diff);

            // Sprawdzamy, czy faworyt traci punkt
            bool homeWasFavorite = p0Home > p0Away;
            double resistance = 1.0;

            // Jeśli faworyt przegrywa, osłabiamy wpływ wyniku (np. o 35%)
            if ((homeWasFavorite && diff < 0) || (!homeWasFavorite && diff > 0))
            {
                resistance = 0.65;
            }

            var effectiveDiff = Math.Sign(diff) * Math.Sqrt(absDiff) * resistance;

            // 4) Obliczenie szansy na Remis
            var pDraw = (double)p0Draw * Math.Pow(0.55, absDiff);

            if (absDiff <= 1)
            {
                var closenessBonus = (1.0 - absDiff * 0.5) * Math.Pow(t, 2.5);
                pDraw += 0.18 * closenessBonus;
            }
            pDraw = Math.Clamp(pDraw, 0.02, 0.90);

            // 5) Podział szans Home/Away
            var remaining = 1.0 - pDraw;
            var baseLogit = Math.Log((double)p0Home / (double)p0Away);
            var currentTilt = effectiveDiff * ScoreK * timeBoost;

            var homeShare = 1.0 / (1.0 + Math.Exp(-(baseLogit + currentTilt)));

            var pHome = remaining * homeShare;
            var pAway = remaining * (1.0 - homeShare);

            // 6) Reguła hierarchii: Remis > Goniący
            const double SafetyGap = 1.25;

            if (diff > 0) // Home prowadzi, Away goni
            {
                double maxAwayProb = pDraw / SafetyGap;
                if (pAway > maxAwayProb)
                {
                    double excess = pAway - maxAwayProb;
                    pAway = maxAwayProb;
                    pHome += excess;
                }
            }
            else if (diff < 0) // Away prowadzi, Home goni
            {
                double maxHomeProb = pDraw / SafetyGap;
                if (pHome > maxHomeProb)
                {
                    double excess = pHome - maxHomeProb;
                    pHome = maxHomeProb;
                    pAway += excess;
                }
            }

            // 7) Drift czasowy
            ApplySimulationDrift(sportEvent, ref pHome, ref pDraw, ref pAway, t);

            // 8) Finalna normalizacja i marża
            var sum = pHome + pDraw + pAway;
            pHome /= sum; pDraw /= sum; pAway /= sum;

            var overround = 1.0 + (double)Margin;
            homeSel.UpdateOdds(ToOdds(pHome, overround));
            drawSel.UpdateOdds(ToOdds(pDraw, overround));
            awaySel.UpdateOdds(ToOdds(pAway, overround));
        }

        private void ApplySimulationDrift(SportEvent ev, ref double ph, ref double pd, ref double pa, double t)
        {
            double tickStrength = (DriftPerMinute / 12.0) * (1.0 + t * 2.0);
            var diff = ev.Score.Home - ev.Score.Away;

            if (diff == 0)
            {
                Transfer(ref ph, ref pd, tickStrength * 0.5, 0.02);
                Transfer(ref pa, ref pd, tickStrength * 0.5, 0.02);
            }
            else if (diff > 0)
            {
                Transfer(ref pa, ref ph, tickStrength * 0.75, 0.01);
                Transfer(ref pd, ref ph, tickStrength * 0.25, 0.02);
            }
            else
            {
                Transfer(ref ph, ref pa, tickStrength * 0.75, 0.01);
                Transfer(ref pd, ref pa, tickStrength * 0.25, 0.02);
            }
        }

        private static (decimal pHome, decimal pDraw, decimal pAway) BaseFromOpeningOdds(Selection h, Selection d, Selection a)
        {
            var ph = 1m / Math.Max(h.OpeningOdds.Value, 1.01m);
            var pd = 1m / Math.Max(d.OpeningOdds.Value, 1.01m);
            var pa = 1m / Math.Max(a.OpeningOdds.Value, 1.01m);
            var sum = ph + pd + pa;
            return (ph / sum, pd / sum, pa / sum);
        }

        private static double GetProgress01(SportEvent ev)
        {
            if (ev.LiveStartedAt is null) return 0.0;
            var elapsed = (ev.LastSimTime - ev.LiveStartedAt.Value).TotalSeconds;
            var total = ev.PlannedDuration.TotalSeconds;
            return Math.Clamp(elapsed / total, 0.0, 1.0);
        }

        private static DomainOdds ToOdds(double p, double over)
        {
            var pWithMargin = Math.Clamp(p * over, 0.01, 0.99);
            return new DomainOdds((decimal)Math.Round(1.0 / pWithMargin, 2, MidpointRounding.AwayFromZero));
        }

        private static double Transfer(ref double from, ref double to, double amount, double minFrom)
        {
            var available = from - minFrom;
            if (available <= 0) return 0;
            var take = Math.Min(available, amount);
            from -= take;
            to += take;
            return take;
        }
    }
}