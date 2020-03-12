using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class CashboxClosureSummarySectionLine
    {
        /// <summary>
        /// Tipo de línea de sumario.
        /// Vease CashboxClosureSummarySectionLineType para obtener más información.
        /// </summary>
        public string SectionLineType { get; set; }

        /// <summary>
        /// Texto que identifica el concepto referenciado por la línea dentro de su tipo de sumario 
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Cantidad de ítems afectos
        /// </summary>
        public decimal? Quantity { get; set; }

        /// <summary>
        /// texto que representa la abreviatura de la unidad de medida utilizada en la cuantificación de ítems de la línea
        /// </summary>
        public string MeasureUnit { get; set; }

        /// <summary>
        /// Cantidad monetaria de la línea de sumario
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// texto que representa el símbolo de la moneda utilizada en la cantidad monetaria de la línea
        /// </summary>
        public string CurrencySymbol { get; set; }
    }
}
