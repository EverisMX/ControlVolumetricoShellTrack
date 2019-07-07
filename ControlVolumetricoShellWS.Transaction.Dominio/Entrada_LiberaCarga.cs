using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Entrada_LiberaCarga
    {
        [DataMember]
        public string serial { get; set; }
        [DataMember]
        public string PTID { get; set; }
        [DataMember]
        public int Pos_Carga { get; set; }
        [DataMember]
        public string idpos { get; set; }
        [DataMember]
        public int nHD { get; set; }
        [DataMember]
        public string pss { get; set; }
    }
}
