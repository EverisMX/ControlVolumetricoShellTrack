﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models
{
    /// <summary>
    /// Representa una línea de un documento de venta que se va a dar de alta
    /// </summary>
    public class CreateDocumentLineDAO
    {
        /// <summary>
        /// Número de línea dentro del documento
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Identificador del producto
        /// (NComapny + Reference)
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Cantidad o número de unidades del producto
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Precio unitario, con impuestos incluidos
        /// </summary>
        public decimal UnitaryPriceWithTax { get; set; }

        /// <summary>
        /// Porcentaje de descuento aplicado
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Valor monetario del descuento aplicado, con impuestos incluidos.
        /// </summary>
        public decimal DiscountAmountWithTax { get; set; }

        /// <summary>
        /// Porcentaje de impuestos aplicado
        /// </summary>
        public decimal TaxPercentage { get; set; }

        /// <summary>
        /// Valor monetario de los impuestos aplicados
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Monto total de la línea, con impuestos incluidos
        /// </summary>
        public decimal TotalAmountWithTax { get; set; }

        /// <summary>
        /// Promociones aplicadas a la línea, si hay alguna.
        /// Lista vacía en caso contrario
        /// </summary>
        public IList<CreateDocumentLinePromotionDAO> AppliedPromotionList { get; set; } = new List<CreateDocumentLinePromotionDAO>();
    }
}