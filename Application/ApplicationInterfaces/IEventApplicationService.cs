using Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ApplicationInterfaces
{
    public interface IEventApplicationService
    {
        public Task<EventObject> CreateEventAsync(EventObject eventObject);
    }
}
