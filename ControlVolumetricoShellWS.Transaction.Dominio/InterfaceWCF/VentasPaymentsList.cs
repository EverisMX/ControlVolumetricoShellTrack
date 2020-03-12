using System;
using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class VentasPaymentsList
    {
        [DataMember]
        public string TipoPago { get; set; }
        [DataMember]
        public string Monto { get; set; }
        [DataMember]
        public string Total { get; set; } 
    }
}
