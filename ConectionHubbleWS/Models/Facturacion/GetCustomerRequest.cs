using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class GetCustomerRequest: BaseRequest
    {
        public string Id { get; set; }
    }
}
