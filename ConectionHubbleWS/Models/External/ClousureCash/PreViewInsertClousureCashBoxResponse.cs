using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class PreViewInsertClousureCashBoxResponse
    {
        public Int32 Error { get; set; }
        public string Ncierre { get; set; }
        public decimal SaldoMetalico { get; set; }
        public decimal SaldoTotal { get; set; }
        public decimal SaldoApertura { get; set; }
        public decimal SaldoNoMetalico { get; set; }
        public decimal SaldoMetalicoCierreAnt { get; set; }
        public Int32 NumTickets { get; set; }
        public string DesdeTicket { get; set; }
        public string HastaTicket { get; set; }
        public decimal Propina { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}
