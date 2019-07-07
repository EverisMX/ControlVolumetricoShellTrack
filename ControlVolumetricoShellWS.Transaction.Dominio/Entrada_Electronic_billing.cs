using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Entrada_Electronic_billing
    {
        [DataMember]
        public string Serial { get; set; }
        [DataMember]
        public string PTID { get; set; }
        [DataMember]
        public string idpos { get; set; }
        [DataMember]
        public int nHD { get; set; }
        [DataMember]
        public string pss { get; set; }
        [DataMember]
        public string Nticket { get; set; }
        [DataMember]
        public string NoCliente { get; set; }
        [DataMember]
        public int TipoOperacion { get; set; }
        [DataMember]
        public string WebID { get; set; }
        [DataMember]
        public string EESS { get; set; }
    }
}
