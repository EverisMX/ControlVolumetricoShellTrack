using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class PrintingTemplate
    {
        /// <summary>
        /// Identificador 
        /// (nombre del template)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Plantilla en formato texto
        /// </summary>
        public string StringifiedTemplate { get; set; }

        /// <summary>
        /// Configuracion de la plantilla. Diccionario string, string
        /// </summary>
        public IDictionary<string, string> TemplateSettings { get; set; } = new Dictionary<string, string>();
    }
}
