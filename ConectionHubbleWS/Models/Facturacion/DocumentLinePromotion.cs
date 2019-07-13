using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class DocumentLinePromotion
    {
        /// <summary>
        /// Identificador de la promoción
        /// </summary>
        public string PromotionId { get; set; }

        /// <summary>
        /// Descripción de la promoción.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Valor monetario del descuento aplicado, con impuestos incluidos
        /// </summary>
        public decimal DiscountAmountWithTax { get; set; }

        /// <summary>
        /// Número de veces aplicado
        /// </summary>
        public int NumberOfTimesApplied { get; set; }

        /// <summary>
        /// Identificador de línea de documento referida por esta promoción
        /// </summary>
        public int ReferredLineNumber { get; set; }

    }
}
