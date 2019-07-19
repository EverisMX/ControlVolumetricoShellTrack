using System;
using System.Runtime.Serialization;
namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Salida_Obtiene_Tran
    {
        [DataMember]
        public Boolean Resultado { get; set; }
        [DataMember]
        public string Msj { get; set; }
        [DataMember]
        public int Estacion { get; set; }
        [DataMember]
        public string Id_product { get; set; }
        [DataMember]
        public string Producto { get; set; }
        [DataMember]
        public decimal Importe { get; set; }
        [DataMember]
        public int PosID { get; set; }
        [DataMember]
        public long Num_Operacion { get; set; }
        [DataMember]
        public decimal Precio_Uni { get; set; }
        [DataMember]
        public decimal IvaPorcentaje { get; set; }
        [DataMember]
        public decimal Litros { get; set; }
        [DataMember]
        public long ID_Interno { get; set; }
        [DataMember]
        public Boolean Parcial { get; set; }
    }
}
