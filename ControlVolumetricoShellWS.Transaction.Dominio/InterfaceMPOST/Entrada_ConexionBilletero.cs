using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Entrada_ConexionBilletero
    {
        [DataMember]
        public string Iienda { get; set; }

        [DataMember]
        public string TPV { get; set; }
    }
}
