using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public enum SeriesType
    {
        /// <summary>
        /// Ticket/boleta
        /// </summary>
        Ticket = 1,

        /// <summary>
        /// Factura
        /// </summary>
        Invoice = 2,

        /// <summary>
        /// Factura rectificativa
        /// </summary>
        RectifyingInvoice = 3,

        /// <summary>
        /// Otro
        /// </summary>
        Other = 4,

        /// <summary>
        /// Nota de despacho
        /// </summary>
        DispatchNote = 5,

        /// <summary>
        /// Ticket rectificativo
        /// </summary>
        RectifyingTicket = 6,

        /// <summary>
        /// Consigna
        /// </summary>
        Consignment = 7
    }
}
