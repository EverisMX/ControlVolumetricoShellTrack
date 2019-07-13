using Conection.HubbleWS.Models.Hubble;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
   public class GetCustomerResponse:BaseResponse
    {
        public Customer Customer { get; set; }

    }
}
