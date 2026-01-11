using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Common
{
    public readonly record struct Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = "PLN")
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new DomainException("Currency cannot be empty.");

            // Dla prostoty: nie pozwalamy na ujemne kwoty (saldo/kwoty zakładów).
            if (amount < 0m)
                throw new DomainException("Money amount cannot be negative.");

            Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
            Currency = currency.Trim().ToUpperInvariant();
        }

        public static Money Zero(string currency = "PLN") => new(0m, currency);

        public static Money operator +(Money a, Money b)
        {
            EnsureSameCurrency(a, b);
            return new Money(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            EnsureSameCurrency(a, b);
            if (a.Amount < b.Amount)
                throw new DomainException("Insufficient funds for subtraction.");

            return new Money(a.Amount - b.Amount, a.Currency);
        }

        public static Money operator *(Money a, decimal multiplier)
        {
            if (multiplier < 0m) throw new DomainException("Multiplier cannot be negative.");
            return new Money(a.Amount * multiplier, a.Currency);
        }

        public static Money operator *(decimal multiplier, Money a) => a * multiplier;

        private static void EnsureSameCurrency(Money a, Money b)
        {
            if (!string.Equals(a.Currency, b.Currency, StringComparison.OrdinalIgnoreCase))
                throw new DomainException($"Currency mismatch: {a.Currency} vs {b.Currency}");
        }

        public override string ToString() => $"{Amount:0.00} {Currency}";
    }

}
