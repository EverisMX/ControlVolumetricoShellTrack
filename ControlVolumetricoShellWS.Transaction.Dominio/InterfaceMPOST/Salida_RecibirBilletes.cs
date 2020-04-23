using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Salida_RecibirBilletes
    {
        [DataMember]
        public int Monto { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string Stock { get; set; }
        [DataMember]
        public bool LibreBill { get; set; }
    }
}
