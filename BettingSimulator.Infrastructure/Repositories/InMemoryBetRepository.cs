using BettingSimulator.Application.Interfaces;
using BettingSimulator.Domain.Bets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Infrastructure.Repositories
{
    public sealed class InMemoryBetRepository : IBetRepository
    {
        private readonly Dictionary<Guid, BetSlip> _bets = new();

        public BetSlip? GetById(Guid id)
            => _bets.TryGetValue(id, out var bet) ? bet : null;

        public IReadOnlyList<BetSlip> GetByUserId(Guid userId)
            => _bets.Values.Where(b => b.UserId == userId).ToList();

        public IReadOnlyList<BetSlip> GetAll()
            => _bets.Values.ToList();

        public void Add(BetSlip betSlip)
        {
            _bets[betSlip.Id] = betSlip;
        }

        public void Update(BetSlip betSlip)
        {
            _bets[betSlip.Id] = betSlip;
        }
    }

}
