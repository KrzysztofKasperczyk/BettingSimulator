using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Domain.Common
{
    public abstract class Entity
    {
        public Guid Id { get; }

        protected Entity(Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Entity other) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && GetType() == other.GetType();
        }

        public override int GetHashCode()
            => HashCode.Combine(GetType(), Id);
    }

}
