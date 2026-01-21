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
        public SettleEventUseCase SettleEventUseCase { get; } 

        public DemoBootstrapper()
        {
            EventRepository = new InMemoryEventRepository();
            BetRepository = new InMemoryBetRepository();
            WalletRepository = new InMemoryWalletRepository();

            TickClock = new TickClock(System.DateTime.Now);

            var oddsCalculator = new LeadBasedOddsCalculator();
            var oddsService = new OddsService(oddsCalculator);

            var settlementService = new SettlementService();

            PlaceBetUseCase = new PlaceBetUseCase(EventRepository, BetRepository, WalletRepository, TickClock);
            TickSimulationUseCase = new TickSimulationUseCase(EventRepository, TickClock, oddsService);

            SettleEventUseCase = new SettleEventUseCase(
                EventRepository, BetRepository, WalletRepository, settlementService, TickClock);

            Seed();
        }

        private void Seed()
        {
            //3 mecze na start
            for (int i = 0; i < 3; i++)
            {
                var demoEvent = DemoDataSeeder.CreateRandomMatch(TickClock.Now);
                EventRepository.Add(demoEvent);
            }
        }
    }

}
