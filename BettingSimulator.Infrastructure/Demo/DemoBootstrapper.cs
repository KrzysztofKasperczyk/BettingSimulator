using BettingSimulator.Application.Interfaces;
using BettingSimulator.Application.UseCases;
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
        public IClock Clock { get; }

        public PlaceBetUseCase PlaceBetUseCase { get; }

        public DemoBootstrapper()
        {
            EventRepository = new InMemoryEventRepository();
            BetRepository = new InMemoryBetRepository();
            WalletRepository = new InMemoryWalletRepository();
            Clock = new SystemClockAdapter();

            PlaceBetUseCase = new PlaceBetUseCase(EventRepository, BetRepository, WalletRepository, Clock);

            Seed();
        }

        private void Seed()
        {
            var demoEvent = DemoDataSeeder.CreateSingleDemoEvent(Clock.Now);
            EventRepository.Add(demoEvent);
        }
    }
}
