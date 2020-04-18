using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Entrada_PaymentPinpadHP
    {
        [DataMember]
        public string Monto { get; set; }

        [DataMember]
        public string Enlacepinpad { get; set; }

        [DataMember]
        public int Autorizar { get; set; }

        [DataMember]
        public string Var_pinpad { get; set; }

        [DataMember]
        public string Var_timeout { get; set; }
    }
}
