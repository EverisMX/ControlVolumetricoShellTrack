using System;
using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class VentasGenerals
    {
        [DataMember]
        public string Categoria { get; set; }
        [DataMember]
        public string Unidad { get; set; }
        [DataMember]
        public string Monto { get; set; }
        [DataMember]
        public string Total { get; set; }
    }
}
