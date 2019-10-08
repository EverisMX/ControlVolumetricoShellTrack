using System;
using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Salida_Info_Forma_Pago
    {
        /// <summary>
        /// Indica si fue exitoso el proceso 
        /// </summary>
        [DataMember]
        public Boolean Resultado { get; set; }

        /// <summary>
        /// Mensaje de causa de error.
        /// </summary>
        [DataMember]
        public string Msj { get; set; }

        /// <summary>
        /// Numero de ticket.
        /// </summary>
        [DataMember]
        public string Nticket { get; set; }

        /// <summary>
        /// WEBID pata la facturacion.
        /// </summary>
        [DataMember]
        public string WebId { get; set; }

        /// <summary>
        /// Numero de la estacion.
        /// </summary>
        [DataMember]
        public string EESS { get; set; }
    }
}
