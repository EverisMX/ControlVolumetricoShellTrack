using System.Collections.Generic;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class GetPrintingConfigurationResponse : BaseResponse
    {
        /// <summary>
        /// Respuesta: Plantillas disponibles para la impresión
        /// </summary>
        //public List<PrintingTemplate> Templates { get; set; } = new List<PrintingTemplate>();

        /// <summary>
        /// Respuesta: Configuracion para el modulo de impresión
        /// </summary>
        //public IList<GlobalSettingss> GlobalSettings { get; set; } = new List<GlobalSettingss>();
       public IDictionary<string, string> GlobalSettings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Respuesta: Comandos para la impresora
        /// </summary>
        //public PosCommands PosCommands { get; set; }
    }
}
