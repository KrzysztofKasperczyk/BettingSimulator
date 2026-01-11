using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BettingSimulator.Domain.Common;

namespace BettingSimulator.Domain.Wallet
{
    public class Wallet : Entity
    {
        public string OwnerName { get; }

        public IReadOnlyList<WalletTransaction> Transactions => _transactions;
        private readonly List<WalletTransaction> _transactions = new();

        public Wallet(string ownerName, Guid? id = null) : base(id)
        {
            if (string.IsNullOrWhiteSpace(ownerName))
                throw new DomainException("Owner name cannot be empty.");

            OwnerName = ownerName.Trim();
        }

        public Money GetBalance(string currency = "PLN")
        {
            // Saldo liczymy z transakcji (audit-friendly).
            decimal sum = 0m;

            foreach (var t in _transactions)
            {
                if (!string.Equals(t.Amount.Currency, currency, StringComparison.OrdinalIgnoreCase))
                    continue;

                sum += t.Type switch
                {
                    TransactionType.Deposit => t.Amount.Amount,
                    TransactionType.Payout => t.Amount.Amount,
                    TransactionType.Refund => t.Amount.Amount,
                    TransactionType.Stake => -t.Amount.Amount,
                    _ => 0m
                };
            }

            // Money nie pozwala na ujemne, ale saldo nie powinno być ujemne przez nasze walidacje.
            return new Money(sum < 0m ? 0m : sum, currency);
        }

        public void Deposit(Money amount, DateTime timestamp, string description = "Deposit")
        {
            AddTransaction(TransactionType.Deposit, amount, description, timestamp);
        }

        public void Stake(Money amount, DateTime timestamp, string description)
        {
            var balance = GetBalance(amount.Currency);
            if (balance.Amount < amount.Amount)
                throw new DomainException("Insufficient wallet balance.");

            AddTransaction(TransactionType.Stake, amount, description, timestamp);
        }

        public void Payout(Money amount, DateTime timestamp, string description)
        {
            AddTransaction(TransactionType.Payout, amount, description, timestamp);
        }

        public void Refund(Money amount, DateTime timestamp, string description)
        {
            AddTransaction(TransactionType.Refund, amount, description, timestamp);
        }

        private void AddTransaction(TransactionType type, Money amount, string description, DateTime timestamp)
        {
            var tx = new WalletTransaction(type, amount, description, timestamp);
            _transactions.Add(tx);
        }
    }

}
