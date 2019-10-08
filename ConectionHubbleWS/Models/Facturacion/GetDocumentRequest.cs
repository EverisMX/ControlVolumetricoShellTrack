using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class GetDocumentRequest:BaseRequest
    {
        /// <summary>
        /// Identificador del cliente
        /// (NCompany + Code)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Uso para el que se solicita el documento.
        /// La lógica de obtención puede variar dependiendo el uso que se le vaya a dar.
        /// </summary>
        public DocumentUsageType UsageType { get; set; }

    }
}
