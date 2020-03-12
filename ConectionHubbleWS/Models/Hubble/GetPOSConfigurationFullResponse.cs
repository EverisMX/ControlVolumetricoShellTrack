using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class GetPOSConfigurationFullResponse
    {
        /// <summary>
        /// Configuración del layout del TPV.
        /// Null si no se especifica ninguna
        /// </summary>
        public Object LayoutConfiguration { get; set; }

        /// <summary>
        /// Precisión decimal a utilizar por el TPV (número de decimales en cantidades, precios,...).
        /// </summary>
        public Object DecimalPrecisionConfiguration { get; set; }

        /// <summary>
        /// Lista con los parámetros de configuración con valores asignados para el TPV.
        /// Lista vacía si no hay ninguno
        /// </summary>
        public IList<ConfigurationParameter> ConfigurationParameterList { get; set; } = new List<ConfigurationParameter>();

        /// <summary>
        /// Tipo de negocio
        /// </summary>
        public object BusinessType { get; set; }

        /// <summary>
        /// Identificador del país por defecto (propósito general).
        /// Null si no hay
        /// </summary>
        public int? DefaultCountryId { get; set; }

        /// <summary>
        /// Identificador del cliente no identificado (cliente de contado).
        /// </summary>
        public string UnknownCustomerId { get; set; }

        /// <summary>
        /// Identificador del cliente por defecto de TPV.
        /// </summary>
        public string DefaultCustomer { get; set; }

        /// <summary>
        /// Identificador del operador por defecto de TPV.
        /// </summary>
        public string DefaultOperator { get; set; }
    }
}
