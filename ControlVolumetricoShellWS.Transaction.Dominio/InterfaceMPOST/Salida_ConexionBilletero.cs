using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Salida_ConexionBilletero
    {
        [DataMember]
        public string Conexion { get; set; }
        [DataMember]
        public bool Resultado { get; set; }
    }
}
