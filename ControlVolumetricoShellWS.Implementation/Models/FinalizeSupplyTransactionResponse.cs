using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlVolumetricoShellWS.Implementation
{
    public class FinalizeSupplyTransactionResponse : BaseResponse
    {
        /// <summary>
        /// Representa el maping entre los identificadores provisionales proporcionados y el identificador definitivo del suministro
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ProvisionalSupplyIdToDefinitiveSupplyIdMapping { get; set; } = new Dictionary<string, string>();
    }
}
