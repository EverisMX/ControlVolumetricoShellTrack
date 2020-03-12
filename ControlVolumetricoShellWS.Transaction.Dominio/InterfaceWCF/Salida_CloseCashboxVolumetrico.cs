using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Salida_CloseCashboxVolumetrico
    {
        [DataMember]
        public string Fecha { get; set; }

        [DataMember]
        public string Tienda { get; set; }

        [DataMember]
        public string Num_Cierre { get; set; }

        [DataMember]
        public string Operador { get; set; }

        [DataMember]
        public IList<VentasPaymentsList> VentaPayments { get; set; } = new List<VentasPaymentsList>();

        [DataMember]
        public IList<VentasGenerals> VentasCombustible { get; set; } = new List<VentasGenerals>();

        [DataMember]
        public IList<VentasGenerals> VentasPerificos { get; set; } = new List<VentasGenerals>();

        [DataMember]
        public bool Resultado { get; set; }

        [DataMember]
        public string Mensaje { get; set; }
    }
}
