using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Salida_AlmacenarStatusBill
    {
        [DataMember]
        public string StatusBill { get; set; }
        [DataMember]
        public string Almacenar { get; set; }
        [DataMember]
        public bool Resultado { get; set; }
    }
}
