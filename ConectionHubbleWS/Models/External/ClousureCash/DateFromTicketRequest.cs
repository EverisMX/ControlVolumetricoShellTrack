using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class DateFromTicketRequest
    {
        public string Id { get; set; }
        public string User { get; set; }
        public string TicketIni { get; set; }
        public string TicketFin { get; set; }
        public string UriModular { get; set; }

    }
}
