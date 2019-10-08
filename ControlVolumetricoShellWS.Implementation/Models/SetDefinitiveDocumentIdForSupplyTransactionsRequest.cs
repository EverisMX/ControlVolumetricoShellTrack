using System;
using System.Collections.Generic;

namespace ControlVolumetricoShellWS.Implementation
{
    public class SetDefinitiveDocumentIdForSupplyTransactionsRequest
    {
        public string OperatorId { get; set; }
        public string DefinitiveDocumentId { get; set; }

        public IList<string> SupplyTransactionIdList { get; set; } = new List<string>();
    }
}
