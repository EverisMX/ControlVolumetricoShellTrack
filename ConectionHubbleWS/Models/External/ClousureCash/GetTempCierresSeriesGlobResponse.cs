using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class GetTempCierresSeriesGlobResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public int Error { get; set; }
        public string Ncierre { get; set; }
    }
}
