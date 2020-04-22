using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlVolumetricoShellWS.Implementation
{
    public class Request_IC0
    {
        public string Monto { get; set; }
        public string enlacepinpad { get; set; }

        public int autorizar { get; set; }
        public string var_pinpad { get; set; }
        public string var_timeout { get; set; }

    }

    public class tRequestToken
    {
        public string LabelEN { get; set; }
        public string LabelES { get; set; }
        public string Value { get; set; }
    }

    public class respuestapinpad
    {
        public string No_AutorizaReversal { get; set; }
        public string Desc_Reversal { get; set; }
        public bool Bandera_Reversal { get; set; }
        public string FOLIO { get; set; }
        public string descripcion { get; set; }
        public bool respuesta { get; set; }
        //se agrego para la impresion
        public string ARQC { get; set; }
        public string NUMSEQ { get; set; }
        public string EMV_APP_LABEL { get; set; }
        public string AUTHORIZACION { get; set; }
        public string AFILIATION { get; set; }
        public string CARDHOLDER_NAME { get; set; }
        public string PAN { get; set; }
        public string retorno { get; set; }
        public string EMV_AID { get; set; }
        public bool firma { get; set; }
    }

    public class metodospinpad
    {
        public string retorno { get; set; }
        public string codeerro { get; set; }
    }


    public class dataJson
    {
        public string RESPONSE { get; set; }
        public string RETURN { get; set; }
        public string PAN { get; set; }
        public string PES { get; set; }
        public string PEZ { get; set; }
        public string PEM { get; set; }
        public string EMV_TLV { get; set; }
        public string TRANSACTION_INFO { get; set; }
        public string PINPAD_SN { get; set; }
    }

    public class dataJson3
    {
        public string RESPONSE { get; set; }
        public string RETURN { get; set; }
        public string PAN { get; set; }
        public string CARDHOLDER_NAME { get; set; }
        public string EMV_ARQC { get; set; }
        public string EMV_TC { get; set; }
        public string EMV_AID { get; set; }
        public string EMV_APP_LABEL { get; set; }
        public string EMV_TVR { get; set; }
        public string EMV_TLV { get; set; }
        public string TRANSACTION_INFO { get; set; }

    }

    public class dataJson3response
    {
        public string RESPONSE { get; set; }
        public string RETURN { get; set; }
        public string PAN { get; set; }
        public string CARDHOLDER_NAME { get; set; }
        public string EMV_ARQC { get; set; }
        public string EMV_TC { get; set; }
        public string EMV_AID { get; set; }
        public string EMV_APP_LABEL { get; set; }
        public string EMV_TVR { get; set; }
        public string EMV_TLV { get; set; }
        public string TRANSACTION_INFO { get; set; }

    }

    public class dataJson2
    {
        public string FOLIO { get; set; }
        public string REQUEST { get; set; }
        public string AUTHORIZATION { get; set; }
        public string RESPONSE_CODE { get; set; }
        public string EMV_TLV { get; set; }
        public string EMV_ISSUER_DATA { get; set; }
        public string AFILIATION { get; set; }
        public string IDPOS { get; set; }
        public string NUMSEQ { get; set; }

    }

    public class dataJson2request
    {
        public string FOLIO { get; set; }
        public string REQUEST { get; set; }
        public string AUTHORIZATION { get; set; }
        public string RESPONSE_CODE { get; set; }
        public string EMV_TLV { get; set; }
        public string EMV_ISSUER_DATA { get; set; }
        public string AFILIATION { get; set; }
        public string IDPOS { get; set; }
        public string NUMSEQ { get; set; }

    }

    public class respIC0
    {
        //agregado temporalmente
        public string montos { get; set; }
        //quitar
        public string Reversal { get; set; }
        public bool Bandera_IC0_autorizacion { get; set; }
        public string retorno { get; set; }
        public string IC0PAN { get; set; }
        public bool FIRMA { get; set; }
        public bool Bandera_IC0 { get; set; }
    }
    public class respIC1
    {
        public string resic1 { get; set; }
        public string resic0 { get; set; }
        //quitar temporal los monto total
        public string montos { get; set; }
    }

    public class envioimpresion
    {

        public string Reversal { get; set; }
        public string FOLIO { get; set; }
        public string ARQC { get; set; }
        public string NUMSEQ { get; set; }
        public string EMV_APP_LABEL { get; set; }
        public string AUTHORIZACION { get; set; }
        public string AFILIATION { get; set; }
        public string CARDHOLDER_NAME { get; set; }
        public string PAN { get; set; }
        public string retorno { get; set; }
        public string EMV_AID { get; set; }

    }

    public class Consumo_apis
    {
        public string sResult { get; set; }
        public bool Bandera_IC0_SR { get; set; }
        public bool Bandera_Time { get; set; }
    }

    public class Consumo_apis_reversal
    {
        public string sResult_rever { get; set; }
        public bool Bandera_rever { get; set; }
    }

    public class Reversal
    {
        public string No_AutorizaReversal { get; set; }
        public string No_TarjetaReversal { get; set; }
        public string Desc_Reversal { get; set; }
        public bool Bandera_Reversal { get; set; }

    }

    public class PARAMS_PINPAD
    {
        public string TIMEOUT { get; set; }
        public string IP { get; set; }
    }
}
