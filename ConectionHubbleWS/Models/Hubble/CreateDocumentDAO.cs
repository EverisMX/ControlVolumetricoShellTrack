using System;
using System.Collections.Generic;

namespace Conection.HubbleWS.Models
{
    /// <summary>
    /// Representa un documento de venta que se va a dar de alta
    /// </summary>
    public class CreateDocumentDAO
    {
        /// <summary>
        /// Identificador provisional para la creación del documento.
        /// 
        /// De carácter temporal, su ámbito será la operación de alta donde se cree este DAO, 
        /// y servirá para identificarlo de manera unívoca frente a otros posibles DAOs de la misma 
        /// operación de creación, hasta que adquieran un identificador de documento definitivo.
        /// Puede repetirse o reutilizarse en sucesivas operaciones de creación de documentos
        ///
        /// Ejemplo:
        /// Si creamos un único documento, su id provisional puede ser un 1. Si a continación creamos
        /// otro único documento, su id provisional también puede ser un 1 (o cualquier otro), y así sucesivamente...
        /// 
        /// Otro ejemplo:
        /// Si creamos dos documentos en una única operación, sus ids provisionales pueden ser 1 y 2. Si a continación
        /// creamos otros dos documentos, sus ids provisionales pueden ser de nuevo 1 y 2 (o cualquieras otros), y así sucesivamente...
        /// </summary>
        public int ProvisionalId { get; set; }

        /// <summary>
        /// Lista de identificadores provisionales, de documentos que se crearán en la misma operación de creación
        /// que este DAO, y que deban ser referenciados (p.ej: porque sean anulados por este documento).
        /// 
        /// Una vez creados, los documentos mantendrán su relaciones de referencia mediante los identificadores definitivos
        /// de documento y no mediante los provisionales, que tienen carácter temporal y son válidos únicamente en la creación.
        /// 
        /// Lista vacía si no hay referencias.
        /// </summary>
        public IList<int> ReferencedProvisionalIdList { get; set; } = new List<int>();

        /// <summary>
        /// Lista de identificadores (NCompany + DocumentNumber) de documentos referenciados.
        /// 
        /// Contendrá indentificadores cuando el presente documento sea una anulación 
        /// de otros documentos o tenga que referenciar a algunos otros.
        /// Lista vacía en otro caso.
        /// </summary>
        public IList<string> ReferencedDocumentIdList { get; set; } = new List<string>();

        /// <summary>
        /// Identificador del TPV que emite el documento
        /// (NCompany + POSCode)
        /// </summary>
        public string PosId { get; set; }

        /// <summary>
        /// Identificador del operador
        /// (NCompany + DNI)
        /// </summary>
        public string OperatorId { get; set; }

        /// <summary>
        /// Identificador de la serie
        /// (NCompany + Code)
        /// </summary>
        public string SerieId { get; set; }

        /// <summary>
        /// Fecha y hora locales de la emisión del documento
        /// </summary>
        public DateTime EmissionLocalDateTime { get; set; }

        /// <summary>
        /// Fecha y hora UTC de la emisión del documento
        /// </summary>
        public DateTime EmissionUTCDateTime { get; set; }

        /// <summary>
        /// Identificador del cliente
        /// (NCompany + Code)
        /// </summary>
        public string CustomerId { get; set; }

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
        /// Null si n existe esta información.
        /// </summary>
        public decimal? Kilometers { get; set; }

        /// <summary>
        /// Matricula habitual. 
        /// (ej: la asociada a Tarjeta Flota).
        /// Null en caso contrario
        /// </summary>
        public string Plate { get; set; }

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
        public IList<CreateDocumentLineDAO> LineList { get; set; } = new List<CreateDocumentLineDAO>();

        /// <summary>
        /// Lista de las entregas/pagos efectuados por el cliente.
        /// Lista vacía en caso de no haber ninguna
        /// </summary>
        public IList<CreateDocumentPaymentDetailDAO> PaymentDetailList { get; set; } = new List<CreateDocumentPaymentDetailDAO>();

        /// <summary>
        /// Datos extra.
        /// Diccionario vacío en caso de no haber ninguno
        /// </summary>
        public IDictionary<string, string> ExtraData { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Modo Atendido 
        /// Si la venta es en modo Atendido
        /// </summary>
        public string isatend { get; set; }
    }
}
