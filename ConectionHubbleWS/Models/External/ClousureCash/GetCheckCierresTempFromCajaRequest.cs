using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class GetCheckCierresTempFromCajaRequest
    {
        public string Id { get; set; }
        public string User { get; set; }
        public string Nempresa { get; set; }
        public string Caja { get; set; }
        public string CajaOperario { get; set; }
        public string Operario { get; set; }
        public string UriModular { get; set; }
    }
}
