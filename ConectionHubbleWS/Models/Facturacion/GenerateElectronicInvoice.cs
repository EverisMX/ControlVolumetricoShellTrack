using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
   public class GenerateElectronicInvoice
    {
        public string EmpresaPortal { get; set; }
        public List<ListTicketDAO> ListTicket { get; set; }
    }
}
