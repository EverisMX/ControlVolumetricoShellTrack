using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Entrada_RecibirBilletes
    {
        [DataMember]
        public int MontoMock { get; set; }
        [DataMember]
        public string TPV { get; set; }
        [DataMember]
        public string Producto { get; set; }
    }
}
