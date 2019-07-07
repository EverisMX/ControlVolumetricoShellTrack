using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Entrada_Info_Forma_Pagos
    {
        [DataMember]
        public string Id_Transaccion { get; set; }
        [DataMember]
        public IList<Entrada_Info_Forma_Pago_List> Info_Forma_Pago { get; set; } = new List<Entrada_Info_Forma_Pago_List>();
        [DataMember]
        public Boolean parciales { get; set; }
        [DataMember]
        public int nHD { get; set; }

    }
}
