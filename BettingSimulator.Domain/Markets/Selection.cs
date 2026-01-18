using BettingSimulator.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Markets
{
    public class Selection
    {
        public string Code { get; }
        public string Name { get; }

        public Odds OpeningOdds { get; }      // NEW
        public Odds CurrentOdds { get; private set; }

        public Selection(string code, string name, Odds openingOdds)
        {
            Code = code;
            Name = name;

            OpeningOdds = openingOdds;        // NEW
            CurrentOdds = openingOdds;
        }

        public void UpdateOdds(Odds newOdds)
        {
            CurrentOdds = newOdds;
        }
    }

}
