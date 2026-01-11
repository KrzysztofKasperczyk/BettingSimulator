using BettingSimulator.Domain.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BettingSimulator.Domain.Wallet;

namespace BettingSimulator.Application.Interfaces
{
    public interface IWalletRepository
    {
        Wallet? GetByUserId(Guid userId);

        /// <summary>
        /// Tworzy portfel, jeśli nie istnieje (w in-memory łatwo).
        /// </summary>
        Wallet GetOrCreate(Guid userId, string ownerName);

        void Update(Wallet wallet);
    }

}
