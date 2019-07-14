﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ControlVolumetricoShellWS.Dominio
{
    [DataContract]
    public class Entrada_Info_Forma_Pagos
    {
        /// <summary>
        /// Número de operación de la bomba enviado en el método Obtiene_Tran
        /// </summary>
        [DataMember]
        public string Id_Transaccion { get; set; }

        /// <summary>
        /// Lista de formas de pagos 
        /// </summary>
        [DataMember]
        public IList<Entrada_Info_Forma_Pago_List> Info_Forma_Pago { get; set; } = new List<Entrada_Info_Forma_Pago_List>();

        /// <summary>
        /// Número de estación por parte de PC
        /// </summary>
        [DataMember]
        public decimal PorpagarEntrada { get; set; }

        /// <summary>
        /// Número de estación por parte de PC
        /// </summary>
        [DataMember]
        public int nHD { get; set; }

        /// <summary>
        /// Número de carga (Bomba).
        /// </summary>
        [DataMember]
        public int Pos_Carga { get; set; }

        /// <summary>
        /// Identificador de usuario.
        /// </summary>
        [DataMember]
        public string Id_teller { get; set; }

        /// <summary>
        /// Id POS
        /// </summary>
        [DataMember]
        public string idpos { get; set; }

        /// <summary>
        /// Parcial de la venta
        /// </summary>
        [DataMember]
        public bool parciales { get; set; }
    }
}
