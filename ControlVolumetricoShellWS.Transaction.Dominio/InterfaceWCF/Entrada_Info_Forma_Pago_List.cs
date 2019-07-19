using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Entrada_Info_Forma_Pago_List
    {
        /// <summary>
        /// Número de autorización 
        /// </summary>
        [DataMember]
        public string nNum_autorizacions { get; set; }

        /// <summary>
        /// Últimos cuatro dígitos de la tarjeta 
        /// </summary>
        [DataMember]
        public string Ultimos_Digitoss { get; set; }

        /// <summary>
        /// Forma de pago
        /// </summary>
        [DataMember]
        public string formapagos { get; set; }

        /// <summary>
        /// IVA del producto.
        /// </summary>
        //[DataMember]
        //public string IvaProducto { get; set; }

        /// <summary>
        /// Identificador de producto
        /// </summary>
        [DataMember]
        public string Id_product { get; set; }

        /// <summary>
        /// Cantidad en litros.
        /// </summary>
        [DataMember]
        public string Cantidad { get; set; }

        /// <summary>
        /// importe
        /// </summary>
        [DataMember]
        public string Importe_Unitario { get; set; }

        /// <summary>
        /// Importe.
        /// </summary>
        [DataMember]
        public string Importetotal { get; set; }


        /// <summary>
        /// Importe.
        /// </summary>
        [DataMember]
        public bool Producto { get; set; }

        /// <summary>
        /// Importe.
        /// </summary>
        [DataMember]
        public string montoPagadoParcial { get; set; }
    }
}
