using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Hubble
{
    public class FinalizeSupplyTransactionForFuelTestRequest
    {
        public string OperatorId { get; set; }
        public int SupplyTransactionId { get; set; }
        public int FuellingPointId { get; set; }
        public decimal Deviation { get; set; }
        public string ReturnTankId { get; set; }
        public string Observations { get; set; }
    }
}
