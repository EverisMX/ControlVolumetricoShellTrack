using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Salida_Electronic_billing
    {
        [DataMember]
        public Boolean Resultado { get; set; }
        [DataMember]
        public string RFC { get; set; }
        [DataMember]
        public string RazonSocial { get; set; }
        [DataMember]
        public string Msj { get; set; }

        #region PATRONES SAT
        [DataMember]
        public string FolioFiscal { get; set; }
        [DataMember]
        public string NumCertificado { get; set; }
        [DataMember]
        public string DateCertificacion { get; set; }
        [DataMember]
        public string RFCProveedorCert { get; set; }
        [DataMember]
        public string SelloDigitaSAT { get; set; }
        [DataMember]
        public string SelloDigitaCFDI { get; set; }
        [DataMember]
        public string CadenaOrigTimbre { get; set; }
        #endregion

        #region PINTADO DE TICKET IMPRESION
        [DataMember]
        public string Estacion { get; set; }
        [DataMember]
        public string Tienda { get; set; }
        [DataMember]
        public string NombreCompania { get; set; }
        [DataMember]
        public string DireccionCompania { get; set; }
        [DataMember]
        public string ColoniaCompania { get; set; }
        [DataMember]
        public string MunicipioCompania { get; set; }
        [DataMember]
        public string EstadoCompania { get; set; }
        [DataMember]
        public string PaisCompania { get; set; }
        [DataMember]
        public string CodigoPostalCompania { get; set; }
        [DataMember]
        public string RfcCompania { get; set; }
        [DataMember]
        public string PermisoCRE { get; set; }
        [DataMember]
        public string RegFiscal { get; set; }
        [DataMember]
        public string ExpedicionTienda { get; set; }
        [DataMember]
        public string DireccionTienda { get; set; }
        [DataMember]
        public string ColoniaTienda { get; set; }
        [DataMember]
        public string MunicipioTienda { get; set; }
        [DataMember]
        public string EstadoTienda { get; set; }
        [DataMember]
        public string PaisTienda { get; set; }
        [DataMember]
        public string CodigoPostalTienda { get; set; }
        [DataMember]
        public string HeaderTick1 { get; set; }
        [DataMember]
        public string HeaderTick2 { get; set; }
        [DataMember]
        public string HeaderTick3 { get; set; }
        [DataMember]
        public string HedaerTick4 { get; set; }
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


        //public List<Productos>productos
       public IList<Productos> productos { get; set; } = new List<Productos>();

        // public Productos Producto { get; set; }
        [DataMember]
        public string Subtotal { get; set; }
        [DataMember]
       // public decimal Iva { get; set; }
        public string iva { get; set; }

        [DataMember]
        public string Total { get; set; }
        [DataMember]
        public string ImporteEnLetra { get; set; }
        [DataMember]
        public string FooterTick1 { get; set; }
        [DataMember]
        public string FooterTick2 { get; set; }
        [DataMember]
        public string FooterTick3 { get; set; }
        [DataMember]
        public string FooterTick4 { get; set; }
        [DataMember]
        public string FooterTick5 { get; set; }
        #endregion
    }

    [DataContract]
    public class Productos
    {
        [DataMember]
        public string ProductName { get; set; }
        [DataMember]
        public int Quantity { get; set; }
        [DataMember]
        public string UnitaryPriceWithTax { get; set; }
        [DataMember]
        public string TotalAmountWithTax { get; set; }
    }
    [DataContract]
    public class Iva
    {
        [DataMember]
        public int TaxPercentage { get; set; }
        [DataMember]
        public string TaxAmount { get; set; }
    }
    [DataContract]
    public class IvaUnico
    {
        [DataMember]
        public int Iva { get; set; }
    }

    [DataContract]
    public class PaymentDetail
    {
        [DataMember]
        public string PaymentMethodId { get; set; }

    }
}
