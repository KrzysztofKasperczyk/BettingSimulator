using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BettingSimulator.Domain.Common;

namespace BettingSimulator.Domain.Wallet
{
    public class WalletTransaction : Entity
    {
        public DateTime Timestamp { get; }
        public TransactionType Type { get; }
        public Money Amount { get; }          // kwota transakcji (zawsze dodatnia)
        public string Description { get; }    // krótki opis (np. "BetSlip XYZ")

        public WalletTransaction(
            TransactionType type,
            Money amount,
            string description,
            DateTime timestamp,
            Guid? id = null) : base(id)
        {
            if (amount.Amount <= 0m)
                throw new DomainException("Transaction amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException("Transaction description cannot be empty.");

            Type = type;
            Amount = amount;
            Description = description.Trim();
            Timestamp = timestamp;
        }
    }

}
