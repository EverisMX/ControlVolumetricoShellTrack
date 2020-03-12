using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class DateFromTicketResponse
    {
        public DateTime TicketIni { get; set; }
        public DateTime TicketFin { get; set; }
        public string Message { get; set; }
        public bool Resultado { get; set; }
    }
}
