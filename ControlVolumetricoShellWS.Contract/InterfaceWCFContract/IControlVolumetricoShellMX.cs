using System.Threading.Tasks;
using ControlVolumetricoShellWS.Dominio;
using System.ServiceModel;

namespace ControlVolumetricoShellWS.Contract
{
    [ServiceContract]
    public interface IControlVolumetricoShellMX
    {
        [OperationContract]
        Task<Salida_Obtiene_Tran> Obtiene_Tran(Entrada_Obtiene_Tran request);

        [OperationContract]
        Task<Salida_Info_Forma_Pago> Info_Forma_Pago(Entrada_Info_Forma_Pagos request);

        [OperationContract]
        Task<Salida_DesbloquearCarga> DesbloquearCarga(Entrada_DesbloquearCarga request);

        [OperationContract]
        Task<Salida_getProductInfo> getProductInfo(Entrada_getProductInfo request);

        [OperationContract]
        Task<Salida_Electronic_billing> Electronic_billing(Entrada_Electronic_billing request);

        [OperationContract]
        Task<Salida_Electronic_billing_FP> Electronic_billing_FP(Entrada_Electronic_billing_FP request);
    }
}
