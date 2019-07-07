using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Salida_Electronic_billing
    {
        [DataMember]
        public Boolean Resultado { get; set; }
        [DataMember]
        public string Msj { get; set; }
        [DataMember]
        public string Text { get; set; }
    }
    [DataContract]
    public class Electronic_billingJSON
    {
        [DataMember]
        public string Tienda { get; set; }
        [DataMember]
        public string RegFiscal { get; set; }
        [DataMember]
        public string Ticket { get; set; }
        [DataMember]
        public string Folio { get; set; }
        [DataMember]
        public string Fecha { get; set; }
        [DataMember]
        public string Terminal { get; set; }
        [DataMember]
        public string Operador { get; set; }
        [DataMember]
        public string WebID { get; set; }
        [DataMember]
        public string FormaPago { get; set; }
        [DataMember]
        public Productos Producto { get; set; }
        [DataMember]
        public decimal Subtotal { get; set; }
        [DataMember]
        public decimal Iva { get; set; }
        [DataMember]
        public decimal Total { get; set; }
        [DataMember]
        public string FolioFiscal { get; set; }
        [DataMember]
        public string NumSat { get; set; }
        [DataMember]
        public string DatetimeCert { get; set; }
        [DataMember]
        public string RfcPro { get; set; }
        [DataMember]
        public string SelloSat { get; set; }
        [DataMember]
        public string SelloCdfi { get; set; }
        [DataMember]
        public string Timbre { get; set; }
    }

    [DataContract]
    public class Productos
    {
        [DataMember]
        public string Nombre { get; set; }
        [DataMember]
        public decimal Cantidad { get; set; }
        [DataMember]
        public decimal Precio { get; set; }
        [DataMember]
        public decimal Importe { get; set; }
    }
}
