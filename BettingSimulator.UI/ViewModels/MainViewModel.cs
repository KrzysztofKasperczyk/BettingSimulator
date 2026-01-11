using BettingSimulator.Application.UseCases;
using BettingSimulator.Domain.Events;
using BettingSimulator.Domain.Markets;
using BettingSimulator.Infrastructure.Demo;
using BettingSimulator.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.UI.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private readonly DemoBootstrapper _bootstrapper;

        public ObservableCollection<SportEvent> Events { get; } = new();

        private SportEvent? _selectedEvent;
        public SportEvent? SelectedEvent
        {
            get => _selectedEvent;
            set
            {
                _selectedEvent = value;
                OnPropertyChanged();
                SelectedMarket = _selectedEvent?.Markets.FirstOrDefault();
                RefreshSelections();
                ((RelayCommand)PlaceBetCommand).RaiseCanExecuteChanged();
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
                RefreshSelections();
                ((RelayCommand)PlaceBetCommand).RaiseCanExecuteChanged();
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
                ((RelayCommand)PlaceBetCommand).RaiseCanExecuteChanged();
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
                ((RelayCommand)PlaceBetCommand).RaiseCanExecuteChanged();
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

        // Demo user (na razie 1 użytkownik)
        public Guid UserId { get; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string UserName { get; } = "Demo User";

        public RelayCommand Deposit100Command { get; }
        public RelayCommand PlaceBetCommand { get; }

        public MainViewModel()
        {
            _bootstrapper = new DemoBootstrapper();

            // 1) Najpierw komendy (żeby setter SelectedEvent mógł je bezpiecznie wywoływać)
            Deposit100Command = new RelayCommand(Deposit100);
            PlaceBetCommand = new RelayCommand(PlaceBet, CanPlaceBet);

            // 2) Wczytaj eventy
            foreach (var ev in _bootstrapper.EventRepository.GetAll())
                Events.Add(ev);

            // 3) Ustaw domyślny wybór (to wywoła setter i RaiseCanExecuteChanged)
            SelectedEvent = Events.FirstOrDefault();

            // 4) startowe saldo
            RefreshBalance();
        }

        private void RefreshSelections()
        {
            Selections.Clear();
            SelectedSelection = null;

            if (SelectedMarket is null) return;

            foreach (var s in SelectedMarket.Selections)
                Selections.Add(s);

            SelectedSelection = Selections.FirstOrDefault();
        }

        private void Deposit100()
        {
            try
            {
                var wallet = _bootstrapper.WalletRepository.GetOrCreate(UserId, UserName);
                wallet.Deposit(new BettingSimulator.Domain.Common.Money(100m, "PLN"), _bootstrapper.Clock.Now, "Initial top-up");
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

            if (string.IsNullOrWhiteSpace(StakeText))
                return false;

            return decimal.TryParse(StakeText, NumberStyles.Number, CultureInfo.InvariantCulture, out var stake) && stake > 0m;
        }

        private void PlaceBet()
        {
            try
            {
                if (SelectedEvent is null || SelectedMarket is null || SelectedSelection is null)
                    return;

                // parsing stake (w UI używamy kropki)
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
            }
            catch (Exception ex)
            {
                Message = $"Błąd: {ex.Message}";
            }
        }

        private void RefreshBalance()
        {
            var wallet = _bootstrapper.WalletRepository.GetOrCreate(UserId, UserName);
            Balance = wallet.GetBalance("PLN").Amount;
        }
    }

}
