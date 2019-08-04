using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS.Models.Facturacion
{
    public class facresponse
    {

        public string tfd { get; set; }
        public string schemaLocation { get; set; }
        public string Version { get; set; }
        public string FolioFiscal { get; set; }
        public string RFCProveedorCert { get; set; }
        public string DateCertificacion { get; set; }
        public string SelloDigitaCFDI { get; set; }
        public string NumCertificado { get; set; }
        public string SelloDigitaSAT { get; set; }
        public string CadenaOrigTimbre { get; set; }
        public string mensaje { get; set; }
    }
}
