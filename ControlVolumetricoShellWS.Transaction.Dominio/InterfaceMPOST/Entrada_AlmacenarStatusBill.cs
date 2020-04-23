using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Entrada_AlmacenarStatusBill
    {
        [DataMember]
        public string TPV { get; set; }
        [DataMember]
        public bool Almacenar { get; set; }
        [DataMember]
        public bool Status { get; set; }
    }
}
