using System.Threading.Tasks;
using ControlVolumetricoShellWS.Dominio;
using System.ServiceModel;

namespace ControlVolumetricoShellWS.Contract
{
    [ServiceContract]
    public interface IControlVolumetricoShell
    {
        [OperationContract]
        //Salida_Obtiene_Tran Obtiene_Tran(Entrada_Obtiene_Tran request);
        Task<Salida_Obtiene_Tran> Obtiene_Tran(Entrada_Obtiene_Tran request);
        [OperationContract]
        Task<Salida_Info_Forma_Pago> Info_Forma_Pago(Entrada_Info_Forma_Pagos request);
        //Salida_Info_Forma_Pago Info_Forma_Pago(Entrada_Info_Forma_Pagos request);
        [OperationContract]
        //Task<Salida_LiberaCarga> LiberaCarga(Entrada_LiberaCarga request);
        Salida_LiberaCarga LiberaCarga(Entrada_LiberaCarga request);
        [OperationContract]
        Task<Salida_getProductInfo> getProductInfo(Entrada_getProductInfo request);
        //Salida_getProductInfo getProductInfo(Entrada_ValidCustumer request);
        [OperationContract]
        Task<Salida_Electronic_billing> Electronic_billing(Entrada_Electronic_billing request);
    }
}
