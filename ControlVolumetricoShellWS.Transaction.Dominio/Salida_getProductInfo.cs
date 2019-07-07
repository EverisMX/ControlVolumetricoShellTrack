using System;
using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Salida_getProductInfo
    {
        [DataMember]
        public Boolean Resultado { get; set; }
        [DataMember]
        public string Msj { get; set; }
        [DataMember]
        public string producto { get; set; }
        [DataMember]
        public decimal importe { get; set; }
        [DataMember]
        public decimal precio_Uni { get; set; }
        [DataMember]
        public string mensajePromocion { get; set; }
    }
}
