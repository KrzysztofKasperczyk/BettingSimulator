using BettingSimulator.Application.Interfaces;
using BettingSimulator.Domain.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Infrastructure.Repositories
{
    public sealed class InMemoryWalletRepository : IWalletRepository
    {
        private readonly Dictionary<Guid, Wallet> _wallets = new();

        public Wallet? GetByUserId(Guid userId)
            => _wallets.TryGetValue(userId, out var wallet) ? wallet : null;

        public Wallet GetOrCreate(Guid userId, string ownerName)
        {
            if (_wallets.TryGetValue(userId, out var existing))
                return existing;

            var wallet = new Wallet(ownerName, id: userId); // ID portfela = UserId (uprości)
            _wallets[userId] = wallet;
            return wallet;
        }

        public void Update(Wallet wallet)
        {
            _wallets[wallet.Id] = wallet;
        }
    }

}
