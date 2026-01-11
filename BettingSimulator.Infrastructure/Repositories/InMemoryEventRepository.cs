using BettingSimulator.Application.Interfaces;
using BettingSimulator.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Infrastructure.Repositories
{
    public sealed class InMemoryEventRepository : IEventRepository
    {
        private readonly Dictionary<Guid, SportEvent> _events = new();

        public SportEvent? GetById(Guid id)
            => _events.TryGetValue(id, out var ev) ? ev : null;

        public IReadOnlyList<SportEvent> GetAll()
            => _events.Values.ToList();

        public void Add(SportEvent sportEvent)
        {
            _events[sportEvent.Id] = sportEvent;
        }

        public void Update(SportEvent sportEvent)
        {
            // In-memory: obiekt i tak jest referencją, ale trzymamy spójny kontrakt.
            _events[sportEvent.Id] = sportEvent;
        }
    }

}
