using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class GetDocumentsForCashboxClosureExtendedResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public IList<CashboxClosureSummarySectionLine> Sections { get; set; } = new List<CashboxClosureSummarySectionLine>();
    }
}
