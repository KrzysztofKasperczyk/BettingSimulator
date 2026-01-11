using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BettingSimulator.Domain.Events;

namespace BettingSimulator.Application.Interfaces
{
    public interface IEventRepository
    {
        SportEvent? GetById(Guid id);
        IReadOnlyList<SportEvent> GetAll();

        void Add(SportEvent sportEvent);
        void Update(SportEvent sportEvent);
    }

}
