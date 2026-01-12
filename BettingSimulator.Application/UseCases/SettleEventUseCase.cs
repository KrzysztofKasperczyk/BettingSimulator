using BettingSimulator.Application.Interfaces;
using BettingSimulator.Domain.Bets;
using BettingSimulator.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Application.UseCases
{
    public sealed class SettleEventUseCase
    {
        private readonly IEventRepository _eventRepository;
        private readonly IBetRepository _betRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ISettlementService _settlementService;
        private readonly IClock _clock;

        public SettleEventUseCase(
            IEventRepository eventRepository,
            IBetRepository betRepository,
            IWalletRepository walletRepository,
            ISettlementService settlementService,
            IClock clock)
        {
            _eventRepository = eventRepository;
            _betRepository = betRepository;
            _walletRepository = walletRepository;
            _settlementService = settlementService;
            _clock = clock;
        }

        public void Execute(Guid eventId)
        {
            var ev = _eventRepository.GetById(eventId)
                     ?? throw new DomainException("Event not found.");

            if (ev.State != Domain.Events.EventState.Finished)
                throw new DomainException("Event must be finished to settle bets.");

            // Rozliczamy tylko bety Placed, które mają nogę na ten event
            var bets = _betRepository.GetAll()
                .Where(b => b.Status == BetStatus.Placed)
                .Where(b => b.Legs.Any(l => l.EventId == eventId))
                .ToList();

            foreach (var bet in bets)
            {
                var isWin = _settlementService.IsWinningBet(bet, ev);

                var wallet = _walletRepository.GetOrCreate(bet.UserId, "User");

                if (isWin)
                {
                    bet.SettleWon(_clock.Now);

                    var payout = bet.GetPotentialPayout(); // Stake * odds
                    wallet.Payout(payout, _clock.Now, $"Payout for bet {bet.Id}");
                }
                else
                {
                    bet.SettleLost(_clock.Now);
                }

                _walletRepository.Update(wallet);
                _betRepository.Update(bet);
            }
        }
    }

}
