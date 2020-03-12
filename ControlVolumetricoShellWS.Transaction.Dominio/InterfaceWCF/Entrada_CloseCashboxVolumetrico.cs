using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Entrada_CloseCashboxVolumetrico
    {
        [DataMember]
        public string Serial { get; set; }

        [DataMember]
        public string Ptid { get; set; }

        [DataMember]
        public int Pos_Carga { get; set; }

        [DataMember]
        public string Idpos { get; set; }

        [DataMember]
        public int Nhd { get; set; }

        [DataMember]
        public string Pss { get; set; }

        [DataMember]
        public string Id_teller { get; set; } 
    }
}
