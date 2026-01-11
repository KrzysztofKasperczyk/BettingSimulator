using BettingSimulator.Application.Interfaces;
using BettingSimulator.Application.Services;
using BettingSimulator.Application.UseCases;
using BettingSimulator.Infrastructure.Odds;
using BettingSimulator.Infrastructure.Repositories;
using BettingSimulator.Infrastructure.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Infrastructure.Demo
{
    public sealed class DemoBootstrapper
    {
        public IEventRepository EventRepository { get; }
        public IBetRepository BetRepository { get; }
        public IWalletRepository WalletRepository { get; }

        public TickClock TickClock { get; }
        public IClock Clock => TickClock;

        public PlaceBetUseCase PlaceBetUseCase { get; }
        public TickSimulationUseCase TickSimulationUseCase { get; }

        public DemoBootstrapper()
        {
            EventRepository = new InMemoryEventRepository();
            BetRepository = new InMemoryBetRepository();
            WalletRepository = new InMemoryWalletRepository();

            TickClock = new TickClock(System.DateTime.Now);

            var oddsCalculator = new LeadBasedOddsCalculator();
            var oddsService = new OddsService(oddsCalculator);

            PlaceBetUseCase = new PlaceBetUseCase(EventRepository, BetRepository, WalletRepository, TickClock);
            TickSimulationUseCase = new TickSimulationUseCase(EventRepository, TickClock, oddsService);

            Seed();
        }

        private void Seed()
        {
            var demoEvent = DemoDataSeeder.CreateSingleDemoEvent(TickClock.Now);
            EventRepository.Add(demoEvent);
        }
    }

}
