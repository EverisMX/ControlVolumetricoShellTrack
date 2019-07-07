using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class GetProductForSaleResponse
    {
        /// <summary>
        /// Referencia del producto (id)
        /// </summary>
        public string ProductReference { get; set; }
        /// <summary>
        /// nombre del producto
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// precio unitario ANTES de descuentos
        /// </summary>
        public decimal UnitaryPricePreDiscount { get; set; }
        /// <summary>
        /// precio unitario POST descuentos
        /// </summary>
        public decimal UnitaryPricePostDiscount { get; set; }
        /// <summary>
        /// Importe Final
        /// </summary>
        public decimal FinalAmount { get; set; }
        /// <summary>
        /// Porcentaje de impuestos aplicado
        /// </summary>
        public decimal TaxPercentage { get; set; }
        /// <summary>
        /// Mensaje o información asociada al artículo.
        /// Típicamente se trata de algún aviso relacionado con el artículo que se le hace al operador que realiza la venta.
        /// Null si no hay mensaje.
        /// </summary>
        public string ProductMessage { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
    }
}
