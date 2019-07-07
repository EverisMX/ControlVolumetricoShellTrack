using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Salida_LiberaCarga
    {

        [DataMember]
        public Boolean Resultado { get; set; }
        [DataMember]
        public string Msj { get; set; }

    }
}
