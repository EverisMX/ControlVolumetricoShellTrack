using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class Document
    {
        /// <summary>
        /// Identificador
        /// (NCompany + DocumentNumber)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Número de documento
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Lista de identificadores (NCompany + DocumentNumber) de documentos referenciados. Vacía en otro caso.
        /// 
        /// Contendrá indentificadores cuando el presente documento sea una anulación 
        /// de otros documentos o tenga que referenciar a algunos otros.
        /// </summary>
        public IList<string> ReferencedDocumentIdList { get; set; } = new List<string>();

        /// <summary>
        /// Identificador del TPV que emitió el documento
        /// (NCompany + POSCode)
        /// </summary>
        public string PosId { get; set; }

        /// <summary>
        /// Identificador del operador
        /// (NCompany + DNI)
        /// </summary>
        public string OperatorId { get; set; }

        /// <summary>
        /// Nombre del operador
        /// </summary>
        public string OperatorName { get; set; }

        /// <summary>
        /// Identificador de la serie
        /// (NCompany + Code)
        /// </summary>
        public string SeriesId { get; set; }

        /// <summary>
        /// Tipo de la serie
        /// </summary>
        public SeriesType SeriesType { get; set; }

        /// <summary>
        /// Fecha y hora locales de la emisión del documento
        /// </summary>
        public DateTime EmissionLocalDateTime { get; set; }

        /// <summary>
        /// Fecha y hora UTC de la emisión del documento
        /// Null en caso de no estar especificada (típicamente con los documentos anteriores a este producto o de otros productos)
        /// </summary>
        public DateTime? EmissionUTCDateTime { get; set; }

        /// <summary>
        /// Cliente
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// Identificador del contacto.
        /// (Incluye el NComapny y es el antiguamente denominado NContact_Cli).
        /// 
        /// El contacto relaciona un cliente con una tarjeta o algún otro
        /// medio usado para su identificación al realizar la venta.
        /// 
        /// Null si no se ha utilizado ninguno de estos medios opcionales.
        /// </summary>
        public string ContactId { get; set; }

        /// <summary>
        /// Kilómetros. 
        /// 
        /// Null si no existe esta información.
        /// </summary>
        public decimal? Kilometers { get; set; }

        /// <summary>
        /// Identificador de la divisa. 
        /// (NComapny + Code)
        /// 
        /// Indica la divisa en la que está la información monetaria a
        /// nivel global de documento, como son el total, cambio devuelto, impuestos, base imponible...
        /// </summary>
        public string CurrencyId { get; set; }

        /// <summary>
        /// Porcentaje de descuento a nivel de documento.
        /// 
        /// Es decir, no es el resultante de los distintos descuentos que 
        /// se hayan podido aplicar en las líneas, sino un posible descuento 
        /// que se aplica a nivel global de documento, posterior e independientemente
        /// de los descuentos aplicados en los productos comprados.
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Cantidad monetaria de descuento aplicado a nivel de documento y con impuestos incluidos.
        /// (En la divisa indicada por el campo CurrencyId).
        /// 
        /// Es decir, no es el resultante de los distintos descuentos que
        /// se hayan podido aplicar en las líneas, sino un posible descuento
        /// que se aplica a nivel global de docuemnto, posterior e independientemente
        /// de los descuentos aplicados en los productos comprados.
        /// </summary>
        public decimal DiscountAmountWithTax { get; set; }

        /// <summary>
        /// Base imponible.
        /// (En la divisa indicada por el campo CurrencyId)
        /// </summary>
        public decimal TaxableAmount { get; set; }

        /// <summary>
        /// Totales monetarios de impuestos desglosados por porcentaje de impuestos.
        /// (En la divisa indicada por el campo CurrencyId)
        /// (Key: %; Value: valor monetario)
        ///
        /// Los valores son obtenidos incluyendo cualquier descuento a nivel global de documento.   
        /// 
        /// Puede incluir el porcentaje 0%, para indicar los artículos que están exentos
        /// del pago de impuestos. En este caso, la cantidad de impuestos total será la
        /// de todos los valores del diccionario, excepto el valor del par con impuesto 0%
        /// </summary>
        public IDictionary<decimal, decimal> TotalTaxList { get; set; } = new Dictionary<decimal, decimal>();

        /// <summary>
        /// Total del documento con impuestos incluidos.
        /// (En la divisa indicada por el campo CurrencyId)
        /// </summary>
        public decimal TotalAmountWithTax { get; set; }

        /// <summary>
        /// Cambio entregado al cliente
        /// (En la divisa indicada por el campo CurrencyId)
        /// </summary>
        public decimal ChangeDelivered { get; set; }

        /// <summary>
        /// Líneas del documento con los productos vendidos.
        /// Lista vacía en caso de no haber ninguna.
        /// </summary>
        public IList<DocumentLine> LineList { get; set; } = new List<DocumentLine>();

        /// <summary>
        /// Lista de las entregas/pagos efectuados por el cliente.
        /// Lista vacía en caso de no haber ninguna
        /// </summary>
        public IList<DocumentPaymentDetail> PaymentDetailList { get; set; } = new List<DocumentPaymentDetail>();

        /// <summary>
        /// Datos extra.
        /// Diccionario vacío en caso de no haber ninguno
        /// </summary>
        public IDictionary<string, string> ExtraData { get; set; } = new Dictionary<string, string>();
    }
}
