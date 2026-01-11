using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.Application.Interfaces
{
    public interface IClock
    {
        DateTime Now { get; }
    }

}
