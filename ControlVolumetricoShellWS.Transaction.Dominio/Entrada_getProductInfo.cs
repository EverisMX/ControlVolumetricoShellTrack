﻿using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Entrada_getProductInfo
    {
        [DataMember]
        public string serial { get; set; }
        [DataMember]
        public string PTID { get; set; }
        [DataMember]
        public int Pos_Carga { get; set; }
        [DataMember]
        public string idpos { get; set; }
        [DataMember]
        public int nHD { get; set; }
        [DataMember]
        public string pss { get; set; }
        [DataMember]
        public string IdProduct { get; set; }
        [DataMember]
        public string Id_teller { get; set; }
    }
}
