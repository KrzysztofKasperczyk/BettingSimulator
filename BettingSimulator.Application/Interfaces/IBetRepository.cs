using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BettingSimulator.Domain.Bets;

namespace BettingSimulator.Application.Interfaces
{
    public interface IBetRepository
    {
        BetSlip? GetById(Guid id);
        IReadOnlyList<BetSlip> GetByUserId(Guid userId);
        IReadOnlyList<BetSlip> GetAll();

        void Add(BetSlip betSlip);
        void Update(BetSlip betSlip);
    }

}
