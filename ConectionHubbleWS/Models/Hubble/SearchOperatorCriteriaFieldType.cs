using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public enum SearchOperatorCriteriaFieldType
    {
        /// <summary>
        /// Número de identificación fiscal (DNI, CIF, RUC...)
        /// </summary>
        TIN = 0,

        /// <summary>
        /// Código
        /// </summary>
        Code = 1,

        /// <summary>
        /// Login
        /// </summary>
        Login = 2,

        /// <summary>
        /// Nombre
        /// </summary>
        Name = 3,

        /// <summary>
        /// Id del operador (NCompany + TIN)
        /// </summary>
        Id = 4
    }
}
