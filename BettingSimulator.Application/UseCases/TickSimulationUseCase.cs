using BettingSimulator.Application.Interfaces;
using BettingSimulator.Application.Services;
using BettingSimulator.Domain.Common;
using BettingSimulator.Domain.Events;
using BettingSimulator.Domain.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Application.UseCases
{
    public sealed class TickSimulationUseCase
    {
        private readonly IEventRepository _eventRepository;
        private readonly IClock _clock;
        private readonly OddsService _oddsService;

        public TickSimulationUseCase(IEventRepository eventRepository, IClock clock, OddsService oddsService)
        {
            _eventRepository = eventRepository;
            _clock = clock;
            _oddsService = oddsService;
        }

        public void Execute()
        {
            var events = _eventRepository.GetAll();

            foreach (var ev in events)
            {
                // Start eventu
                if (ev.State == EventState.Scheduled && ev.StartTime <= _clock.Now)
                {
                    ev.StartAt(_clock.Now);
                    _oddsService.RecalculateForEvent(ev);
                    _eventRepository.Update(ev);
                    continue;
                }

                if (ev.State != EventState.Live)
                    continue;

                if (ev.LiveStartedAt is null)
                    throw new DomainException("Live event must have LiveStartedAt.");

                // Finish po planned duration
                if (_clock.Now - ev.LiveStartedAt.Value >= ev.PlannedDuration)
                {
                    ev.FinishAt(_clock.Now);
                    _eventRepository.Update(ev);
                    continue;
                }

                // Symulacja punktów
                var before = ev.Score;
                var after = SimulateScoreTick(before);

                if (!Equals(before, after))
                {
                    ev.UpdateScore(after);

                    // krytyczny moment: zawieszenie rynku
                    foreach (var market in ev.Markets.Where(m => m.State == MarketState.Open))
                        market.Suspend();

                    _oddsService.RecalculateForEvent(ev);

                    foreach (var market in ev.Markets.Where(m => m.State == MarketState.Suspended))
                        market.Open();
                }

                _eventRepository.Update(ev);
            }
        }

        private static Score SimulateScoreTick(Score score)
        {
            var r = Random.Shared.NextDouble();

            // ~12% szansy na punkt w ticku
            if (r < 0.06) return new Score(score.Home + 1, score.Away);
            if (r < 0.12) return new Score(score.Home, score.Away + 1);

            return score;
        }
    }

}
