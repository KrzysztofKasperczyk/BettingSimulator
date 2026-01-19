using BettingSimulator.Application.Interfaces;
using BettingSimulator.Application.Services;
using BettingSimulator.Domain.Common;
using BettingSimulator.Domain.Events;
using BettingSimulator.Domain.Markets;
using System;
using System.Linq;

namespace BettingSimulator.Application.UseCases
{
    public sealed class TickSimulationUseCase
    {
        private readonly IEventRepository _eventRepository;
        private readonly IClock _clock;
        private readonly OddsService _oddsService;
        private readonly Random _random;

        // Seed dla deterministycznej symulacji
        private const int SimulationSeed = 12345;

        public TickSimulationUseCase(
            IEventRepository eventRepository,
            IClock clock,
            OddsService oddsService)
        {
            _eventRepository = eventRepository;
            _clock = clock;
            _oddsService = oddsService;
            _random = new Random(SimulationSeed);
        }

        public void Execute()
        {
            var events = _eventRepository.GetAll();

            foreach (var ev in events)
            {
                // Aktualny czas symulacji
                ev.MarkSimTime(_clock.Now);

                // --- START EVENTU ---
                if (ev.State == EventState.Scheduled && ev.StartTime <= _clock.Now)
                {
                    ev.StartAt(_clock.Now);
                    _oddsService.RecalculateForEvent(ev);
                    ev.MarkScoreSnapshot();
                    _eventRepository.Update(ev);
                    continue;
                }

                if (ev.State != EventState.Live)
                    continue;

                if (ev.LiveStartedAt is null)
                    throw new DomainException("Live event must have LiveStartedAt.");

                // --- KONIEC EVENTU ---
                if (_clock.Now - ev.LiveStartedAt.Value >= ev.PlannedDuration)
                {
                    ev.FinishAt(_clock.Now);
                    ev.MarkScoreSnapshot();
                    _eventRepository.Update(ev);
                    continue;
                }

                // --- SYMULACJA WYNIKU OPARTA NA KURSACH ---

                // Pobieramy rynek 1X2, aby określić siłę drużyn na podstawie kursów otwarcia
                var market1X2 = ev.Markets.FirstOrDefault(m => m.Type == MarketType.ThreeWay_1X2);

                double homeStrength = 0.5; // Domyślnie 50/50 jeśli brak rynku
                if (market1X2 != null)
                {
                    double hOdds = (double)market1X2.GetSelectionByCode("HOME").OpeningOdds.Value;
                    double aOdds = (double)market1X2.GetSelectionByCode("AWAY").OpeningOdds.Value;

                    // Obliczamy prawdopodobieństwo implikowane (bez remisu i marży)
                    double pHone = 1.0 / hOdds;
                    double pAway = 1.0 / aOdds;
                    homeStrength = pHone / (pHone + pAway);
                }

                var before = ev.Score;
                var after = SimulateScoreTick(before, homeStrength);

                var scoreChanged = !Equals(before, after);

                if (scoreChanged)
                {
                    ev.UpdateScore(after);

                    // Zawieszenie rynku po bramce
                    foreach (var market in ev.Markets.Where(m => m.State == MarketState.Open))
                        market.Suspend();
                }

                // Aktualizacja kursów (zawsze - dla płynnego driftu)
                _oddsService.RecalculateForEvent(ev);

                if (scoreChanged)
                {
                    // Otwarcie rynków po przeliczeniu kursów
                    foreach (var market in ev.Markets.Where(m => m.State == MarketState.Suspended))
                        market.Open();
                }

                ev.MarkScoreSnapshot();
                _eventRepository.Update(ev);
            }
        }

        /// <summary>
        /// Symuluje punkt w ticku, biorąc pod uwagę względną siłę faworyta.
        /// </summary>
        /// <param name="homeStrength">Wartość 0-1 określająca szansę, że to gospodarz strzeli, jeśli padnie gol.</param>
        private Score SimulateScoreTick(Score score, double homeStrength)
        {
            var r = _random.NextDouble();

            // Bazowa szansa na jakiegokolwiek gola w ticku (np. 3%)
            // 0.03 przy ticku 1-minutowym daje średnio 2.7 gola na mecz.
            const double chanceOfAnyGoal = 0.03;

            if (r < chanceOfAnyGoal)
            {
                // Drugi los decyduje, która drużyna strzeliła (proporcjonalnie do siły)
                var sideRandom = _random.NextDouble();

                if (sideRandom < homeStrength)
                    return new Score(score.Home + 1, score.Away);
                else
                    return new Score(score.Home, score.Away + 1);
            }

            return score;
        }
    }
}