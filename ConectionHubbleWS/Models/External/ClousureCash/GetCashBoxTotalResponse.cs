using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class GetCashBoxTotalResponse
    {
        public string Tpago { get; set; }
        public string NombreTPago { get; set; }
        public decimal Entradas { get; set; }
        public decimal Salidas { get; set; }
        public decimal Saldo { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}
