using System;
using System.Collections.Generic;

namespace Conection.HubbleWS
{
    /// <summary>
    /// Representa una entrega/pago efectuado por el cliente en un documento de venta que se va a dar de alta
    /// </summary>
    public class CreateDocumentPaymentDetailDAO
    {
        /// <summary>
        /// Indica la fecha en la que se realiza el pago en fecha/hora local.
        /// </summary>
        public DateTime LocalDateTime { get; set; }

        /// <summary>
        /// Indica la fecha en la que se realiza el pago en fecha/hora local UTC.
        /// </summary>
        public DateTime UtcDateTime { get; set; }

        /// <summary>
        /// identificador del medio de pago.
        /// (NCompany + Code)
        /// </summary>
        public string PaymentMethodId { get; set; }

        /// <summary>
        /// Identificador de la divisa en la que el cliente ha efectuado el pago.
        /// (NCompany + Code)
        /// </summary>
        public string CurrencyId { get; set; }

        /// <summary>
        /// Factor de cambio respecto a la moneda base, 
        /// es decir, cuántas por cada unidad de la moneda base.
        /// La moneda base tiene un factor de 1
        /// 
        /// (p.ej: si la base fuera el euro, la peseta tendría un factor de 166.386)
        /// </summary>
        public decimal ChangeFactorFromBase { get; set; }

        /// <summary>
        /// Cantidad entregada por el cliente, en moneda primaria (indicada en el documento).
        /// 
        /// Si la divisa indicada en esta entrega es base, representa el hecho sucedido.
        /// Si no, es una conversión a moneda base del valor correspondiente.
        /// </summary>
        public decimal PrimaryCurrencyGivenAmount { get; set; }

        /// <summary>
        /// De lo entregado por el cliente, valor monetario cogido por el TPV, en moneda primaria (indicada en el documento).
        /// 
        /// Si la divisa indicada en esta entrega es base, representa directamente el hecho sucedido.
        /// Si no, es una conversión a moneda base del valor correspondiente.
        /// </summary>
        public decimal PrimaryCurrencyTakenAmount { get; set; }

        /// <summary>
        /// Especifica el tipo de pago que se va a generar
        /// </summary>
        public CreatePaymentUsageType UsageType { get; set; }
    }
}
