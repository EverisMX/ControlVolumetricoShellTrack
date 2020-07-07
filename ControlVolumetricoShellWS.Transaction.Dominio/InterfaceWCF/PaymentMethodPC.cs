using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlVolumetricoShellWS.Transaction.Dominio.InterfaceWCF
{
    public enum PaymentMethodPC
    {
        AMERICAN_EXPRESS = 1,
        MORRALLA = 2,
        EFECTIVO = 3,
        VALE_GENERICO = 7,
        VALE_INBURSA = 9,
        SERVIBONOS = 13,
        VALES_GASOCHECK = 14,
        VALES_ACCOR = 15,
        VALES_EFECTIVALE = 16,
        HIDROVALE = 19,
        CHEQUE_TARJETA_DESECHABLE = 25,
        TICKET_CAR_ACCOR = 27,
        SODEXHOPASS = 28,
        PUNTO_CLAVE = 30,
        EFECTICAR = 31,
        TARJETA_BANCARIA = 32,
        MERCADO_PAGO = 91,
        PUNTOS = 98,
        TARJETA_INTELIGENTE = 99,
        VALE_EXTERNO = 8
    }

}
