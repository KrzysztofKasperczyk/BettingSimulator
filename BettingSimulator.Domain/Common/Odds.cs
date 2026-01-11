using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Common
{
    public readonly record struct Odds
    {
        public decimal Value { get; }

        public Odds(decimal value)
        {
            // Minimalny sensowny kurs w bukmacherce > 1.0
            if (value <= 1.0m)
                throw new DomainException("Odds must be greater than 1.0.");

            Value = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        public override string ToString() => Value.ToString("0.00");

        public static Odds FromProbability(decimal probability)
        {
            // probability in (0,1)
            if (probability <= 0m || probability >= 1m)
                throw new DomainException("Probability must be between 0 and 1 (exclusive).");

            // odds = 1 / p
            var odds = 1m / probability;
            return new Odds(odds);
        }
    }

}
