using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Salida_PaymentPinpadHP
    {
        [DataMember]
        public string No_AutorizaReversal { get; set; }

        [DataMember]
        public string Desc_Reversal { get; set; }

        [DataMember]
        public bool Bandera_Reversal { get; set; }

        [DataMember]
        public string FOLIO { get; set; }

        [DataMember]
        public string Descripcion { get; set; }

        [DataMember]
        public bool Respuesta { get; set; }

        [DataMember]
        //se agrego para la impresion
        public string ARQC { get; set; }

        [DataMember]
        public string NUMSEQ { get; set; }

        [DataMember]
        public string EMV_APP_LABEL { get; set; }

        [DataMember]
        public string AUTHORIZACION { get; set; }

        [DataMember]
        public string AFILIATION { get; set; }

        [DataMember]
        public string CARDHOLDER_NAME { get; set; }

        [DataMember]
        public string PAN { get; set; }

        [DataMember]
        public string Retorno { get; set; }

        [DataMember]
        public string EMV_AID { get; set; }

        [DataMember]
        public bool Firma { get; set; }
    }
}
