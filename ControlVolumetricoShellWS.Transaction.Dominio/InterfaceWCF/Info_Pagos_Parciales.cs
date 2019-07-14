using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Info_Pagos_Parciales
    {
        [DataMember]
        public string formaPagosParcial { get; set; }
        [DataMember]
        public decimal montoPagadoParcial { get; set; }
    }
}
