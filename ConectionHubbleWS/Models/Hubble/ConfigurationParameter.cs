using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class ConfigurationParameter
    {
        /// <summary>
        /// Identificador del parámetro 
        /// (se refiere al maestro de parámeros)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tipo reconocido de parámetro
        /// </summary>
        public object Type { get; set; }

        /// <summary>
        /// Nombre del parámetro
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del parámetro
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Nombre del grupo funcional al que pertenece
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Valor por defecto, en formato cadena.
        /// Propiedad informativa (ver propiedad MeaningfulStringValue)
        /// Null si no hay valor definido (no hay valor o el valor asignado es nulo)
        /// </summary>
        public string DefaultLevelStringValue { get; set; }

        /// <summary>
        /// Valor a nivel de empresa, en formato cadena.
        /// Propiedad informativa (ver propiedad MeaningfulStringValue)
        /// Null si no hay valor definido (no hay valor o el valor asignado es nulo)
        /// </summary>
        public string CompanyLevelStringValue { get; set; }

        /// <summary>
        /// Valor a nivel de tienda, en formato cadena.
        /// Propiedad informativa (ver propiedad MeaningfulStringValue)
        /// Null si no hay valor definido (no hay valor o el valor asignado es nulo)
        /// </summary>
        public string ShopLevelStringValue { get; set; }

        /// <summary>
        /// Valor a nivel de TPV, en formato cadena.
        /// Propiedad informativa (ver propiedad MeaningfulStringValue)
        /// Null si no hay valor definido (no hay valor o el valor asignado es nulo)
        /// </summary>
        public string POSLevelStringValue { get; set; }

        /// <summary>
        /// Valor a significativo/resultante/válido a usar por el TPV, en formato cadena.
        /// Null si no hay valor definido (no hay valor o el valor asignado es nulo)
        /// </summary>
        public string MeaningfulStringValue { get; set; }

    }
}
