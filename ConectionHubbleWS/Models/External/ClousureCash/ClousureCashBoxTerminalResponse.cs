using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class ClousureCashBoxTerminalResponse
    {
        public int Status { get; set; }

        public string Message { get; set; }

        public int Error { get; set; }

        public string Ncierre { get; set; }

        public decimal Saldo_Metalico { get; set; }

        public decimal Saldo_Total { get; set; }

        public decimal Saldo_Apertura { get; set; }

        public decimal Saldo_NoMetalico { get; set; }

        public decimal Saldo_Metalico_Cierre_Ant { get; set; }

        public int NumTickets { get; set; }

        public string DesdeTicket { get; set; }

        public string HastaTicket { get; set; }

        public decimal Propina { get; set; }
    }
}
