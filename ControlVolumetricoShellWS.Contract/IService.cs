using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlVolumetricoShellWS.Dominio;
using System.ServiceModel;

namespace ControlVolumetricoShellWS.Contract
{
    [ServiceContract]
    public interface IService
    {

        //Salida_Obtiene_Tran Obtiene_Tran(string serial, string PTID, int Pos_Carga, string idpos, int nHD, string pss, string Id_teller);
        [OperationContract]
        Salida_Obtiene_Tran Obtiene_Tran(Entrada_Obtiene_Tran request);
        [OperationContract]
        Salida_Info_Forma_Pago Info_Forma_Pago(Entrada_Info_Forma_Pagos request);
        [OperationContract]
        Salida_LiberaCarga LiberaCarga(Entrada_LiberaCarga request);
        [OperationContract]
        Task<Salida_getProductInfo> getProductInfo(Entrada_getProductInfo request);

        [OperationContract]
        Salida_Electronic_billing Electronic_billing(Entrada_Electronic_billing request);




    }
}
