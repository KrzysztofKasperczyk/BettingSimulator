using BettingSimulator.Application.UseCases;
using BettingSimulator.Domain.Events;
using BettingSimulator.Domain.Markets;
using BettingSimulator.Infrastructure.Demo;
using BettingSimulator.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Threading;

namespace BettingSimulator.UI.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private readonly DemoBootstrapper _bootstrapper;
        private readonly DispatcherTimer _timer;

        // Lista po lewej (VM-y tylko do listy, żeby kolory działały bez migania)
        public ObservableCollection<EventListItemViewModel> EventItems { get; } = new();

        private EventListItemViewModel? _selectedEventItem;
        public EventListItemViewModel? SelectedEventItem
        {
            get => _selectedEventItem;
            set
            {
                _selectedEventItem = value;
                OnPropertyChanged();

                // Napędza prawą stronę
                SelectedEvent = _selectedEventItem?.Event;
            }
        }

        // Historia zakładów
        public ObservableCollection<BetListItemViewModel> UserBets { get; } = new();

        private SportEvent? _selectedEvent;
        public SportEvent? SelectedEvent
        {
            get => _selectedEvent;
            set
            {
                _selectedEvent = value;
                OnPropertyChanged();

                // Uwaga: tu nie zmieniamy SelectedMarket, jeśli już był ustawiony i nadal pasuje.
                // Dzięki temu nie resetujemy użytkownikowi wyborów przy drobnych odświeżeniach.
                if (_selectedEvent is null)
                {
                    SelectedMarket = null;
                    return;
                }

                if (SelectedMarket is null || !_selectedEvent.Markets.Any(m => m.Id == SelectedMarket.Id))
                    SelectedMarket = _selectedEvent.Markets.FirstOrDefault();

                PlaceBetCommand.RaiseCanExecuteChanged();
            }
        }

        private Market? _selectedMarket;
        public Market? SelectedMarket
        {
            get => _selectedMarket;
            set
            {
                _selectedMarket = value;
                OnPropertyChanged();

                RefreshSelectionsPreserveSelection();

                PlaceBetCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<Selection> Selections { get; } = new();

        private Selection? _selectedSelection;
        public Selection? SelectedSelection
        {
            get => _selectedSelection;
            set
            {
                _selectedSelection = value;
                OnPropertyChanged();

                PlaceBetCommand.RaiseCanExecuteChanged();
            }
        }

        private string _stakeText = "10.00";
        public string StakeText
        {
            get => _stakeText;
            set
            {
                _stakeText = value;
                OnPropertyChanged();

                PlaceBetCommand.RaiseCanExecuteChanged();
            }
        }

        private string _message = "";
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set { _balance = value; OnPropertyChanged(); }
        }

        public string SimTimeText => _bootstrapper.Clock.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Demo user (na razie 1 użytkownik)
        public Guid UserId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string UserName { get; } = "Demo User";

        public RelayCommand Deposit100Command { get; }
        public RelayCommand PlaceBetCommand { get; }

        public MainViewModel()
        {
            _bootstrapper = new DemoBootstrapper();

            Deposit100Command = new RelayCommand(Deposit100);
            PlaceBetCommand = new RelayCommand(PlaceBet, CanPlaceBet);

            // Załaduj eventy do listy po lewej
            foreach (var ev in _bootstrapper.EventRepository.GetAll())
                EventItems.Add(new EventListItemViewModel(ev));

            SelectedEventItem = EventItems.FirstOrDefault();

            RefreshBalance();
            RefreshUserBets();
            OnPropertyChanged(nameof(SimTimeText));

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _timer.Tick += (_, _) => OnTick();
            _timer.Start();
        }

        private void OnTick()
        {
            try
            {
                // 1 tick = 5 sekund czasu symulacji
                _bootstrapper.TickClock.Advance(TimeSpan.FromSeconds(5));

                // logika symulacji
                _bootstrapper.TickSimulationUseCase.Execute();

                // ✅ odśwież statusy eventów na liście po lewej (bez migania)
                foreach (var item in EventItems)
                    item.SyncFromDomain();

                // settlement (MVP: może się wywołać wiele razy, ale jest bezpieczne)
                if (SelectedEvent is not null &&
                    SelectedEvent.State == EventState.Finished)
                {
                    _bootstrapper.SettleEventUseCase.Execute(SelectedEvent.Id);
                    RefreshBalance();
                    RefreshUserBets();
                }

                // czas w UI
                OnPropertyChanged(nameof(SimTimeText));

                // odśwież szczegóły po prawej (State/Score/itd.)
                OnPropertyChanged(nameof(SelectedEvent));

                // odśwież kursy w tabeli, ale zachowaj klikniętą selekcję
                RefreshSelectionsPreserveSelection();
            }
            catch (Exception ex)
            {
                _timer.Stop();
                Message = $"Błąd symulacji: {ex.Message}";
            }
        }

        private void RefreshSelectionsPreserveSelection()
        {
            var previouslySelectedCode = SelectedSelection?.Code;

            Selections.Clear();
            if (SelectedMarket is null)
                return;

            foreach (var s in SelectedMarket.Selections)
                Selections.Add(s);

            // Przywróć wybór jeśli nadal istnieje
            if (!string.IsNullOrWhiteSpace(previouslySelectedCode))
                SelectedSelection = Selections.FirstOrDefault(s => s.Code == previouslySelectedCode);

            // Jeśli nie było poprzedniego wyboru albo zniknął – wybierz pierwszy
            SelectedSelection ??= Selections.FirstOrDefault();
        }

        private void Deposit100()
        {
            try
            {
                var wallet = _bootstrapper.WalletRepository.GetOrCreate(UserId, UserName);
                wallet.Deposit(
                    new BettingSimulator.Domain.Common.Money(100m, "PLN"),
                    _bootstrapper.Clock.Now,
                    "Initial top-up"
                );
                _bootstrapper.WalletRepository.Update(wallet);

                RefreshBalance();
                Message = "Doładowano 100.00 PLN.";
            }
            catch (Exception ex)
            {
                Message = $"Błąd: {ex.Message}";
            }
        }

        private bool CanPlaceBet()
        {
            if (SelectedEvent is null || SelectedMarket is null || SelectedSelection is null)
                return false;

            // blokada jeśli kurs poza zakresem (min 1.05, max 50) -> w UI powinno być "-"
            var oddsVal = SelectedSelection.CurrentOdds.Value;
            if (!BettingSimulator.Domain.Common.OddsLimits.IsWithinRange(oddsVal))
                return false;

            if (string.IsNullOrWhiteSpace(StakeText))
                return false;

            return decimal.TryParse(
                StakeText,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var stake
            ) && stake > 0m;
        }

        private void PlaceBet()
        {
            try
            {
                if (SelectedEvent is null || SelectedMarket is null || SelectedSelection is null)
                    return;

                if (!decimal.TryParse(StakeText, NumberStyles.Number, CultureInfo.InvariantCulture, out var stakeAmount))
                {
                    Message = "Niepoprawna stawka (użyj np. 10.00).";
                    return;
                }

                var req = new PlaceBetRequest(
                    UserId: UserId,
                    UserName: UserName,
                    EventId: SelectedEvent.Id,
                    MarketId: SelectedMarket.Id,
                    SelectionCode: SelectedSelection.Code,
                    StakeAmount: stakeAmount,
                    Currency: "PLN"
                );

                var result = _bootstrapper.PlaceBetUseCase.Execute(req);

                RefreshBalance();

                Message =
                    $"POSTAWIONO ✅  Kurs: {result.OddsAtPlacement:0.00} | " +
                    $"Potencjalna wygrana: {result.PotentialPayout:0.00} PLN | " +
                    $"Saldo: {result.NewBalance:0.00} PLN";

                RefreshUserBets();
            }
            catch (Exception ex)
            {
                Message = $"Błąd: {ex.Message}";
            }
        }

        private void RefreshUserBets()
        {
            UserBets.Clear();

            var bets = _bootstrapper.BetRepository.GetByUserId(UserId);

            foreach (var bet in bets.OrderByDescending(b => b.PlacedAt ?? b.CreatedAt))
            {
                var leg = bet.Legs.FirstOrDefault();
                if (leg is null) continue;

                var ev = _bootstrapper.EventRepository.GetById(leg.EventId);
                var eventName = ev?.Name ?? leg.EventId.ToString();

                var combinedOdds = bet.GetCombinedOdds().Value;
                var potential = bet.GetPotentialPayout().Amount;

                UserBets.Add(new BetListItemViewModel
                {
                    BetSlipId = bet.Id,
                    EventName = eventName,
                    SelectionCode = leg.SelectionCode,
                    Odds = combinedOdds,
                    Stake = bet.Stake.Amount,
                    PotentialPayout = potential,
                    Status = bet.Status.ToString(),
                    PlacedAt = bet.PlacedAt ?? bet.CreatedAt
                });
            }
        }

        private void RefreshBalance()
        {
            var wallet = _bootstrapper.WalletRepository.GetOrCreate(UserId, UserName);
            Balance = wallet.GetBalance("PLN").Amount;
        }
    }
}
