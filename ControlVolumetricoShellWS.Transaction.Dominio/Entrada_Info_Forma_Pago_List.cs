using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Entrada_Info_Forma_Pago_List
    {
        [DataMember]
        string nNum_autorizacions { get; set; }
        [DataMember]
        string Ultimos_Digitoss { get; set; }
        [DataMember]
        string formapagos { get; set; }
        [DataMember]
        double importes { get; set; }
    }
}
