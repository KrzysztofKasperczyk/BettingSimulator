using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Wallet
{
    public enum TransactionType
    {
        Deposit,     // doładowanie
        Stake,       // postawienie zakładu (obciążenie)
        Payout,      // wypłata wygranej
        Refund       // zwrot stawki (np. void/cancel)
    }

}
