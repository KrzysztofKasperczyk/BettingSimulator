using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BettingSimulator.Application.Interfaces;
using BettingSimulator.Domain.Bets;
using BettingSimulator.Domain.Common;
using BettingSimulator.Domain.Events;
using BettingSimulator.Domain.Markets;
using BettingSimulator.Domain.Wallet;

namespace BettingSimulator.Application.UseCases
{
    public sealed class PlaceBetUseCase
    {
        private readonly IEventRepository _eventRepository;
        private readonly IBetRepository _betRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IClock _clock;

        public PlaceBetUseCase(
            IEventRepository eventRepository,
            IBetRepository betRepository,
            IWalletRepository walletRepository,
            IClock clock)
        {
            _eventRepository = eventRepository;
            _betRepository = betRepository;
            _walletRepository = walletRepository;
            _clock = clock;
        }

        public PlaceBetResult Execute(PlaceBetRequest request)
        {
            if (request.StakeAmount <= 0m)
                throw new DomainException("Stake must be greater than zero.");

            var ev = _eventRepository.GetById(request.EventId)
                     ?? throw new DomainException("Event not found.");

            if (ev.State == EventState.Finished)
                throw new DomainException("Cannot place bet on finished event.");

            var market = ev.Markets.FirstOrDefault(m => m.Id == request.MarketId)
                         ?? throw new DomainException("Market not found for given event.");

            if (market.State != MarketState.Open)
                throw new DomainException("Market is not open for betting.");

            var selection = market.GetSelectionByCode(request.SelectionCode);

            // jesli kurs poza zakresem to nie mozna obstawic
            var oddsVal = selection.CurrentOdds.Value;
            if (!BettingSimulator.Domain.Common.OddsLimits.IsWithinRange(oddsVal))
                throw new DomainException("Nie można obstawić tej opcji (kurs poza zakresem).");


            // zamroz kurs w momencie obstawienia
            var oddsAtPlacement = selection.CurrentOdds;

            
            var wallet = _walletRepository.GetOrCreate(request.UserId, request.UserName);

            var stake = new Money(request.StakeAmount, request.Currency);

            // pobieranie z portfela
            wallet.Stake(stake, _clock.Now, $"Stake for event '{ev.Name}' ({market.Name} - {selection.Code})");
            _walletRepository.Update(wallet);

            // tworzenie kuponu
            var betSlip = new BetSlip(request.UserId, _clock.Now);
            betSlip.SetStake(stake);

            var leg = new BetLeg(ev.Id, market, selection, oddsAtPlacement);
            betSlip.AddLeg(leg);

            betSlip.Place(_clock.Now);

            _betRepository.Add(betSlip);

            var potentialPayout = betSlip.GetPotentialPayout();
            var newBalance = wallet.GetBalance(request.Currency);

            return new PlaceBetResult(
                BetSlipId: betSlip.Id,
                OddsAtPlacement: oddsAtPlacement.Value,
                PotentialPayout: potentialPayout.Amount,
                NewBalance: newBalance.Amount
            );
        }
    }

}
