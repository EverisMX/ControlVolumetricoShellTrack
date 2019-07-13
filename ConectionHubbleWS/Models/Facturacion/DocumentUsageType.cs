using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public enum DocumentUsageType
    {
        /// <summary>
        /// Otro, diferente al resto de los especificados
        /// </summary>
        Other = 0,

        /// <summary>
        /// Imprimir copia del documento
        /// </summary>
        PrintCopy = 1,

        /// <summary>
        /// Rectificar/anular un documento
        /// </summary>
        Rectify = 2,

        /// <summary>
        /// Pagar documentos pendientes
        /// </summary>
        PayPending = 3
    }
}
