using System.Collections.Generic;

namespace ControlVolumetricoShellWS.Implementation
{
    public class GetAllSupplyTransactionsOfFuellingPointResponse : BaseResponse
    {
        public IEnumerable<SupplyTransaction> SupplyTransactionList { get; set; } = new List<SupplyTransaction>();
    }
}
