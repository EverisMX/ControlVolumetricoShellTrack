using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class GetDocumentsForCashboxClosureExtendedRequest
    {
        public string Id { get; set; }
        public string User { get; set; }
        public string Idcompany { get; set; }
        public string Idcashbox { get; set; }
        public DateTime Dateto { get; set; }
        public string Idrunaway { get; set; }
        public string Idcashboxopeningannotationtype { get; set; }
        public Int32 CashboxClosureMode { get; set; }
        public string Idoperator { get; set; }
        public string UriModular { get; set; }

    }
}
