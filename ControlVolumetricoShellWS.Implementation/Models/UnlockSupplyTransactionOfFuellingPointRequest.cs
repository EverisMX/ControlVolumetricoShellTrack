using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlVolumetricoShellWS.Implementation
{
    public class UnlockSupplyTransactionOfFuellingPointRequest
    {
        public string OperatorId { get; set; }
        public int SupplyTransactionId { get; set; }
        public int FuellingPointId { get; set; }
    }
}
