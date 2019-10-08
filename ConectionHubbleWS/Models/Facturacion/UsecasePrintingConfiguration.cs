using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class UsecasePrintingConfiguration
    {
        /// <summary>
        /// Caso de uso para el que se va a usar la plantilla
        /// </summary>
        public string UseCase { get; set; }

        /// <summary>
        /// Tipo de la plantilla en plataforma
        /// </summary>
        public string PrintingTemplatePlatformType { get; set; }

        /// <summary>
        /// Numero de copias
        /// </summary>
        public int DefaultNumberOfCopies { get; set; }

        /// <summary>
        /// Indica si es obligatorio tener este template
        /// </summary>
        public Boolean Required { get; set; } = false;
    }
}
