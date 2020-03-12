using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class AreThereAnyPendingToCloseItemByCashBoxIdRequest
    {
        public string Id { get; set; }
        public string User { get; set; }
        public string IdCompany { get; set; }
        public string IdCashbox { get; set; }
        public string CashboxOpeningAnnotationTypeId { get; set; }
        public Int32 CashboxClosureMode { get; set; }
        public string Idoperator { get; set; }
        public string UriModular { get; set; }
    }
}
