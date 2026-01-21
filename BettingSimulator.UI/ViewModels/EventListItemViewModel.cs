using BettingSimulator.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BettingSimulator.UI.ViewModels
{
    public sealed class EventListItemViewModel : ViewModelBase
    {
        public Guid Id => Event.Id;
        public SportEvent Event { get; }

        public string Name => Event.Name;

        private EventState _state;
        public EventState State
        {
            get => _state;
            private set
            {
                if (_state == value) return;
                _state = value;
                OnPropertyChanged();
            }
        }

        public EventListItemViewModel(SportEvent ev)
        {
            Event = ev ?? throw new ArgumentNullException(nameof(ev));
            _state = ev.State;
        }

        public void SyncFromDomain()
        {
            
            State = Event.State;

        }
    }
}
