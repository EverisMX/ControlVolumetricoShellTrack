using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using ControlVolumetricoShellWS.Contract;
using ControlVolumetricoShellWS.Dominio;
using Conection.HubbleWS;
using Conection.HubbleWS.Models.Facturacion;
using NodaTime;
using MX_LogsHPTPV;
using Conection.HubbleWS.Models.Hubble;
using ControlVolumetricoShellWS.Transaction.Dominio.InterfaceWCF;

namespace ControlVolumetricoShellWS.Implementation
{
    public class ControlVolumetricoShellMX : IControlVolumetricoShellMX
    {
        // propiedad el 1 indica cuantos pueden pasar por el semáforo. 
        /*private static System.Threading.SemaphoreSlim SemaphoreSlimObtener_tran { get; } = new System.Threading.SemaphoreSlim(1);
        private static System.Threading.SemaphoreSlim SemaphoreSlimInfo_Forma_Pago { get; } = new System.Threading.SemaphoreSlim(1);
        private static System.Threading.SemaphoreSlim SemaphoreSlimDesbloquearCarga { get; } = new System.Threading.SemaphoreSlim(1);
        private static System.Threading.SemaphoreSlim SemaphoreSlimGetProductInfo { get; } = new System.Threading.SemaphoreSlim(1);
        private static System.Threading.SemaphoreSlim SemaphoreSlimElectronic_billing_FP { get; } = new System.Threading.SemaphoreSlim(1);*/


        void Log(string codLog, string log)
        {
            LogsTPVHP _exec = new LogsTPVHP();
            try
            {
                _exec.GeneraLogInfo(codLog, log);
            }
            catch (Exception e)
            {
                try
                {
                    _exec.GeneraLogInfo("CODEVOL_TR LOGERR", "@SHELLMX ERROR DE ESCRITURA LOG: " + e.ToString());
                }
                catch (Exception)
                {

                    //throw;
                }
                //throw;
            }
        }

        public async Task<Salida_Obtiene_Tran> Obtiene_Tran(Entrada_Obtiene_Tran request)
        {
            Salida_Obtiene_Tran salida_Obtiene_Tran = new Salida_Obtiene_Tran();
            LogsTPVHP exec = new LogsTPVHP();
            var criptoObt = DateTime.Now.ToString("yyyyMM") + "_OBT_TRAN_" + DateTime.Now.ToString("h-mm-ss-ffff");
            try
            {
                // si está ocupado se espera.
                //await SemaphoreSlimObtener_tran.WaitAsync();

                #region VALIDACIONES DE LAS ENTRADAS DE OBTENER_TRAN
                Log("CODEVOL_INI INFO", "@SHELLMX- SE INICIA EL METODO DE OBTIENE_TRAN PARA OBTENER LA INFORMACION DE LA BOMBA: " + request.Pos_Carga.ToString() + "  IDSEGUIMIENTO: " + criptoObt);
                Log("CODEVOL_TR INFO", "@SHELLMX- REQUEST QUE SE OBTIENE. IDSEGUIMIENTO: " + criptoObt + "\n" + "Obtiene_Tran:" + "\n" + "{" + "\n" +
                    "    idpos: " + request.idpos + "," + "\n" +
                    "    teller: " + request.Id_teller + "," + "\n" +
                    "    nHD: " + request.nHD + "," + "\n" +
                    "    pos_carga: " + request.Pos_Carga + "," + "\n" +
                    "    pss: " + request.pss + "," + "\n" +
                    "    serial: " + request.serial + "\n" + "}");
                try
                {
                    if (request.Pos_Carga < 0)
                    {
                        //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                        Log("CODEVOL_FIN ERROR 67", "@SHELLMX- DEBE DE INSERTAR UN NUMERO DE SURTIDOR QUE ESTA LIGADO. IDSEGUIMIENTO: " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = "DEBE DE INSERTAR UN SURTIDOR VALIDO."
                        };
                    }
                }
                catch (Exception e)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    Log("CODEVOL_FIN ERROR 66", "@SHELLMX- DEBE DE INTRODUCIR EL FORMATO CORRECTO DE SURTIDOR <NUMERO> IDSEGUIMIENTO: + " + criptoObt + "  LOG: " + e.ToString());
                    return new Salida_Obtiene_Tran
                    {
                        Resultado = false,
                        Msj = "DEBE DE INTRODUCIR EL FORMATO CORRECTO PARA EL NUMERO DE SURTIDOR."
                    };
                    throw e;
                }
                if (request.nHD < 0 || request.idpos == null)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    Log("CODEVOL_FIN ERROR 65", "@SHELLMX- DEBE DE INSERTAR VALORES VALIDOS CON EL FORMATO nHD | idpos VERIFICAR. IDSEGUIMIENTO: " + criptoObt);
                    return new Salida_Obtiene_Tran
                    {
                        Resultado = false,
                        Msj = "VERIFICAR DATOS DE ENTRADA DE LA BOMBA " + request.Pos_Carga.ToString() + " VERIFICAR LOGS EN TPV."
                    };
                }
                // SHELLMX- Al momento de traer la informacion sobre la transaccion que hay en parte sobre un surtidor, bloquea en el TVP que Action lo este usando, Se contruye el objeto
                //          a llenar de lock para traer la demas informacion sobre la transaccion del Surtidor seleccinado.

                ConectionSignalRDoms conectionSignalRDoms = new ConectionSignalRDoms();

                if (conectionSignalRDoms.StatusConectionHubbleR() < 0)
                {
                    Log("CODEVOL_FIN ERROR 64", "@SHELLMX- FALLO LA CONEXION CON EL DOMS VERIFICAR QUE ESTE CONECTADO. RESPONSE: " + conectionSignalRDoms.StatusConectionHubbleR().ToString() + " IDSEGUIMIENTO: " + criptoObt);
                    return new Salida_Obtiene_Tran
                    {
                        Resultado = false,
                        Msj = "FALLO LA CONEXIÓN CON EL DOMS VERIFICAR LOGS Y TPV.",
                    };
                }
                #endregion

                //SHELLMX- Indentificamos que el Operador este registrado en el Sistema de Everilion.Shell
                // SHELLMX- Se consigue el Token del TPV para hacer las pruebas. 
                var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
                TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
                InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();

                #region VALIDACION DEL OPERADOR ID | CODE
                List<SearchOperatorCriteria> SearchOperatorCriteriaOperator = new List<SearchOperatorCriteria>
                {
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Id , MatchingType = SearchOperatorCriteriaMatchingType.Exact },
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Code , MatchingType = SearchOperatorCriteriaMatchingType.Exact }
                };
                SearchOperatorRequest searchOperatorRequest = new SearchOperatorRequest
                {
                    Identity = bsObj.Identity,
                    CriteriaList = SearchOperatorCriteriaOperator,
                    CriteriaRelationshipType = SearchCriteriaRelationshipType.Or,
                    MustIncludeDischarged = false
                };

                SearchOperatorResponse searchOperatorResponse = await invokeHubbleWebAPIServices.SearchOperator(searchOperatorRequest);
                string idOperatorObtTran = null;
                string codeOperatorOntTran = null;
                string nameOperator = null;

                if (searchOperatorResponse.OperatorList.Count == 0)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    Log("CODEVOL_FIN WARNING 63", "@SHELLMX- OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA. Id_Teller : " + request.Id_teller.ToString() + " IDSEGUIMIENTO: " + criptoObt);
                    return new Salida_Obtiene_Tran
                    {
                        Resultado = false,
                        Msj = "OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                    };
                }
                foreach (var searchOperator in searchOperatorResponse.OperatorList)
                {
                    idOperatorObtTran = searchOperator.Id;
                    codeOperatorOntTran = searchOperator.Code;
                    nameOperator = searchOperator.Name.ToString();
                }

                #endregion

                GetAllSupplyTransactionsOfFuellingPointRequest getAllSupplyTransactionsOfFuellingPointRequest = new GetAllSupplyTransactionsOfFuellingPointRequest { OperatorId = idOperatorObtTran, FuellingPointId = request.Pos_Carga };
                LockSupplyTransactionOfFuellingPointResponse lockTransactionInformation = await conectionSignalRDoms.LockSupplyTransactionOfFuellingPoint(bsObj.Identity, getAllSupplyTransactionsOfFuellingPointRequest, criptoObt);

                /*try
                {
                    if(lockTransactionInformation.Status <= -1)
                    {
                        Log("CODEVOL_FIN ERROR" , "@SHELLMX- SE TERMINO EL METODO DE OBTENER LA BOMBA CON STATUS FALLIDO Y ENTREGANDO UN MENSAJE AL OPERADOR. IDSEGUIMIENTO : " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = lockTransactionInformation.Message,
                        };
                    }
                    if (lockTransactionInformation.Status == 0)
                    {
                        Log("CODEVOL_FIN WARNING", "@SHELLMX- SE TERMINO EL METODO DE OBTENER LA BOMBA CON STATUS FALLIDO Y ENTREGANDO UN MENSAJE AL OPERADOR. IDSEGUIMIENTO : " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = lockTransactionInformation.Message,
                        };
                    }
                    if (lockTransactionInformation.CorrespondingVolume == 0 && lockTransactionInformation.DiscountedAmount == 0 && lockTransactionInformation.DiscountPercentage == 0 && lockTransactionInformation.FinalAmount == 0 && lockTransactionInformation.ProductName == null && lockTransactionInformation.ProductReference == null)
                    {
                        Log("CODEVOL_FIN WARNING", "@SHELLMX- Transaccion Bloqueada por otra Terminal: LockSupplyTransactionOfFuellingPoint IDSEGUIMIENTO: " + criptoObt + " \n IDTRANSACTION: \n {" + "\n" +
                            "    CorrespondingVolume: " + lockTransactionInformation.CorrespondingVolume.ToString() + "," + "\n" +
                            "    DiscountedAmount: " + lockTransactionInformation.DiscountedAmount.ToString() + "," + "\n" +
                            "    Status: " + lockTransactionInformation.Status.ToString() + "," + "\n" +
                            "    Message: " + lockTransactionInformation.Message.ToString() + "," + "\n" +
                            "    PosID: " + lockTransactionInformation.posID.ToString() + "," + "\n" +
                            "    ProductReference: " + lockTransactionInformation.ProductReference.ToString() + "," + "\n" + "}");
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = "TRANSACCION BLOQUEADA POR OTRA TERMINAL, VERIFICAR.",
                        };
                    }
                    else
                    {
                        Log("CODEVOL_TR INFO", "@SHELLMX- OPERADOR QUE PIDE EL BLOQUEO DE LA BOMBA " + request.Pos_Carga.ToString() + " -->  " + nameOperator + "  IDSEGUIMIENTO: " + criptoObt);

                        GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest { Identity = bsObj.Identity };
                        GetPOSInformationResponse getPOSInformationResponse = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);

                        salida_Obtiene_Tran.Resultado = true;
                        salida_Obtiene_Tran.ID_Interno = lockTransactionInformation.Id;
                        salida_Obtiene_Tran.Msj = lockTransactionInformation.Message;
                        salida_Obtiene_Tran.Estacion = Convert.ToInt32(getPOSInformationResponse.PosInformation.ShopCode);
                        salida_Obtiene_Tran.Importe = lockTransactionInformation.FinalAmount;
                        salida_Obtiene_Tran.Litros = lockTransactionInformation.CorrespondingVolume;
                        salida_Obtiene_Tran.Num_Operacion = request.Pos_Carga;
                        salida_Obtiene_Tran.Parcial = false;
                        salida_Obtiene_Tran.PosID = Convert.ToInt32(getPOSInformationResponse.PosInformation.Code);
                        salida_Obtiene_Tran.Precio_Uni = lockTransactionInformation.GradeUnitPrice;
                        salida_Obtiene_Tran.Producto = Convert.ToString(lockTransactionInformation.GradeId);     //lockTransactionInformation.ProductReference,
                        salida_Obtiene_Tran.idInternoPOS = lockTransactionInformation.posID;

                        //Id_product = lockTransactionInformation.ProductReference
                        Log("CODEVOL_FIN INFO", "@SHELLMX- SE TERMINO EL METODO DE OBTENER_TRAN EXITOSAMENTE CON EL SIGUIENTE RESPONSE. IDSEGUIMIENTO: " + criptoObt + "\n" + "Salida_Obtiene_Tran: \n {" + "\n" +
                            "    resultado: " + salida_Obtiene_Tran.Resultado.ToString() + "," + "\n" +
                            "    id_Interno: " + salida_Obtiene_Tran.ID_Interno.ToString() + "," + "\n" +
                            "    msj: " + salida_Obtiene_Tran.Msj.ToString() + "," + "\n" +
                            "    estacion: " + salida_Obtiene_Tran.Estacion.ToString() + "," + "\n" +
                            "    importe: " + salida_Obtiene_Tran.Importe.ToString() + "," + "\n" +
                            "    litros: " + salida_Obtiene_Tran.Litros.ToString() + "," + "\n" +
                            "    num_operacion: " + salida_Obtiene_Tran.Num_Operacion.ToString() + "," + "\n" +
                            "    parcial: " + salida_Obtiene_Tran.Parcial.ToString() + "," + "\n" +
                            "    pos_id: " + salida_Obtiene_Tran.PosID.ToString() + "," + "\n" +
                            "    precio_uni: " + salida_Obtiene_Tran.Precio_Uni.ToString() + "," + "\n" +
                            "    producto: " + salida_Obtiene_Tran.Producto.ToString() + "," + "\n" +
                            "    idInternoPOS: " + salida_Obtiene_Tran.idInternoPOS.ToString() + "\n" + "}");
                    }
                }
                catch(Exception ext0)
                {
                    //Se Pinta el error.
                    Log("CODEVOL_ WARNING", "ENTRO AL CATCH DEL OBTENER_TRAN: Message: " + ext0.Message + " Stacttrace: " + ext0.StackTrace);
                    //Se verifica el status de la bomba si se bloquee o no. 
                    GetAllSupplyTransactionsOfFuellingPointResponse getAllSupplyTransactionsOfFuellingPointResponse = conectionSignalRDoms.GetAllSupplyTransactionsOfFuellingPoint(getAllSupplyTransactionsOfFuellingPointRequest, criptoObt);
                    if(getAllSupplyTransactionsOfFuellingPointResponse.Status <= -1)
                    {
                        Log("CODEVOL_FIN ERROR", "@SHELLMX- SE PRODUJO UN ERROR AL OBTENER EL SUMINISTRO DE LA BOMBA. IDSEGUIMIENTO: " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = getAllSupplyTransactionsOfFuellingPointResponse.Message
                        };
                    }
                    if(getAllSupplyTransactionsOfFuellingPointResponse.Status == 0)
                    {
                        Log("CODEVOL_FIN WARNING", "@SHELLMX- SE PRODUJO UNA ADVERTENCIA AL OBTENER EL SUMINISTRO DE LA BOMBA. IDSEGUIMIENTO: " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = getAllSupplyTransactionsOfFuellingPointResponse.Message
                        };
                    }

                    int? lockingPOSIdAux = null;
                    foreach (SupplyTransaction supplyTransaction in getAllSupplyTransactionsOfFuellingPointResponse.SupplyTransactionList)
                    {
                        lockingPOSIdAux = supplyTransaction.LockingPOSId;
                    }
                    if(lockingPOSIdAux == null)
                    {
                        Log("CODEVOL_FIN WARNING ", "@SHELLMX- SE PRODUJO UN ERROR CON EL MAPEO DEL RESQUEST AL DATA DE LA BOMBA DEL DOMS, NO SE HA BLOQUEADO LA BOMBA, SE PUEDE PEDIR NUEVAMENTE. IDSEGUIMIENTO: " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = "ERROR AL PEDIR INFORMACION DE LA BOMBA, NO SE BLOQUEO SURTIDOR: " + request.Pos_Carga.ToString()
                        };
                    }
                }*/
                if (lockTransactionInformation.Status < 0)
                {
                    Log("CODEVOL_FIN 62", "@SHELLMX- FALLO EN BLOQUEAR LA BOMBA IDSEGUIMIENTO:" + criptoObt + "  Log:: \n" + "LockSupplyTransactionOfFuellingPointResponse: \n {" + "\n" +
                        "    status: " + lockTransactionInformation.Status.ToString() + "," + "\n" +
                        "    messaje: " + lockTransactionInformation.Message.ToString() + "\n" + "}");
                    return new Salida_Obtiene_Tran
                    {
                        Resultado = false,
                        Msj = lockTransactionInformation.Message,
                    };
                }
                else if (lockTransactionInformation.CorrespondingVolume == 0 && lockTransactionInformation.DiscountedAmount == 0 && lockTransactionInformation.DiscountPercentage == 0 && lockTransactionInformation.FinalAmount == 0 && lockTransactionInformation.ProductName == null && lockTransactionInformation.ProductReference == null)
                {
                    Log("CODEVOL_FIN 61", "@SHELLMX- Transaccion Bloqueada por otra Terminal: LockSupplyTransactionOfFuellingPoint IDSEGUIMIENTO: " + criptoObt + " \n IDTRANSACTION: \n {" + "\n" +
                        "    CorrespondingVolume: " + lockTransactionInformation.CorrespondingVolume.ToString() + "," + "\n" +
                        "    DiscountedAmount: " + lockTransactionInformation.DiscountedAmount.ToString() + "," + "\n" +
                        "    Status: " + lockTransactionInformation.Status.ToString() + "," + "\n" +
                        "    Message: " + lockTransactionInformation.Message.ToString() + "," + "\n" +
                        "    PosID: " + lockTransactionInformation.posID.ToString() + "," + "\n" +
                        "    ProductReference: " + lockTransactionInformation.ProductReference.ToString() + "," + "\n" + "}");
                    return new Salida_Obtiene_Tran
                    {
                        Resultado = false,
                        Msj = "TRANSACCION BLOQUEADA POR OTRA TERMINAL,VERIFICAR",
                    };
                }
                else
                {
                    Log("CODEVOL_TR INFO", "OPERADOR QUE PIDE EL BLOQUEO DE LA BOMBA Y INFO : " + nameOperator + "  IDSEGUIMIENTO: " + criptoObt);

                    GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest { Identity = bsObj.Identity };
                    GetPOSInformationResponse getPOSInformationResponse = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);

                    salida_Obtiene_Tran.Resultado = true;
                    salida_Obtiene_Tran.ID_Interno = lockTransactionInformation.Id;
                    salida_Obtiene_Tran.Msj = lockTransactionInformation.Message;
                    salida_Obtiene_Tran.Estacion = Convert.ToInt32(getPOSInformationResponse.PosInformation.ShopCode);
                    salida_Obtiene_Tran.Importe = lockTransactionInformation.FinalAmount;
                    salida_Obtiene_Tran.Litros = lockTransactionInformation.CorrespondingVolume;
                    salida_Obtiene_Tran.Num_Operacion = request.Pos_Carga;
                    salida_Obtiene_Tran.Parcial = false;
                    salida_Obtiene_Tran.PosID = Convert.ToInt32(getPOSInformationResponse.PosInformation.Code);
                    salida_Obtiene_Tran.Precio_Uni = lockTransactionInformation.GradeUnitPrice;
                    salida_Obtiene_Tran.Producto = Convert.ToString(lockTransactionInformation.GradeId);     //lockTransactionInformation.ProductReference,
                    salida_Obtiene_Tran.idInternoPOS = lockTransactionInformation.posID;

                    //Id_product = lockTransactionInformation.ProductReference
                    Log("CODEVOL_FIN INFO", "SE TERMINO EL METODO DE OBTENER_TRAN EXITOSAMENTE CON EL SIGUIENTE RESPONSE: IDSEGUIMIENTO: " + criptoObt + "\n" + "Salida_Obtiene_Tran: \n {" + "\n" +
                        "    resultado: " + salida_Obtiene_Tran.Resultado.ToString() + "," + "\n" +
                        "    id_Interno: " + salida_Obtiene_Tran.ID_Interno.ToString() + "," + "\n" +
                        "    msj: " + salida_Obtiene_Tran.Msj.ToString() + "," + "\n" +
                        "    estacion: " + salida_Obtiene_Tran.Estacion.ToString() + "," + "\n" +
                        "    importe: " + salida_Obtiene_Tran.Importe.ToString() + "," + "\n" +
                        "    litros: " + salida_Obtiene_Tran.Litros.ToString() + "," + "\n" +
                        "    num_operacion: " + salida_Obtiene_Tran.Num_Operacion.ToString() + "," + "\n" +
                        "    parcial: " + salida_Obtiene_Tran.Parcial.ToString() + "," + "\n" +
                        "    pos_id: " + salida_Obtiene_Tran.PosID.ToString() + "," + "\n" +
                        "    precio_uni: " + salida_Obtiene_Tran.Precio_Uni.ToString() + "," + "\n" +
                        "    producto: " + salida_Obtiene_Tran.Producto.ToString() + "," + "\n" +
                        "    idInternoPOS: " + salida_Obtiene_Tran.idInternoPOS.ToString() + "\n" + "}");
                }
            }
            catch (Exception ext)
            {
                /*Log("CODEVOL_ WARNING", "ENTRO AL CATCH MAS INTERNO (FASE DE RECUPERACION) AL OBTENER_TRAN. IDSEGUIMIENTO:" + criptoObt + "  Message: " + ext.Message + " Stacttrace: " + ext.StackTrace);
                //Se manda a verificar el estado del surtidor si tiene data en entrada.
                if (request.Id_teller != null && request.Pos_Carga != 0)
                {
                    if (request.Pos_Carga < 0)
                    {
                        //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                        Log("CODEVOL_FIN WARNING 67.catch()", "@SHELLMX- DEBE DE INSERTAR UN SURTIDOR QUE ESTA LIGADO Pos_Carga: " + request.Pos_Carga.ToString() + " IDSEGUIMIENTO: " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = "DEBE DE INSERTAR UN SURTIDOR QUE ESTE LIGADO. BOMBA: " + request.Pos_Carga.ToString()
                        };
                    }

                    ConectionSignalRDoms conectionSignalRDoms = new ConectionSignalRDoms();

                    if (conectionSignalRDoms.StatusConectionHubbleR() < 0)
                    {
                        Log("CODEVOL_FIN ERROR 64.catch()", "@SHELLMX- FALLO LA CONEXION CON EL DOMS VERIFICAR QUE ESTE CONECTADO! RESPONSE: " + conectionSignalRDoms.StatusConectionHubbleR().ToString() + " IDSEGUIMIENTO: " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = "FALLO LA CONEXCIÓN CON EL DOMS VERIFICAR SU ESTADO EN TPV."
                        };
                    }

                    //SHELLMX- Indentificamos que el Operador este registrado en el Sistema de Everilion.Shell
                    // SHELLMX- Se consigue el Token del TPV para hacer las pruebas. 
                    var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
                    TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
                    InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();

                    #region VALIDACION DEL OPERADOR ID | CODE
                    List<SearchOperatorCriteria> SearchOperatorCriteriaOperator = new List<SearchOperatorCriteria>
                        {
                            new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Id , MatchingType = SearchOperatorCriteriaMatchingType.Exact },
                            new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Code , MatchingType = SearchOperatorCriteriaMatchingType.Exact }
                        };
                    SearchOperatorRequest searchOperatorRequest = new SearchOperatorRequest
                    {
                        Identity = bsObj.Identity,
                        CriteriaList = SearchOperatorCriteriaOperator,
                        CriteriaRelationshipType = SearchCriteriaRelationshipType.Or,
                        MustIncludeDischarged = false
                    };

                    SearchOperatorResponse searchOperatorResponse = await invokeHubbleWebAPIServices.SearchOperator(searchOperatorRequest);
                    string idOperatorObtTran = null;
                    string codeOperatorOntTran = null;
                    string nameOperator = null;

                    if (searchOperatorResponse.OperatorList.Count == 0)
                    {
                        //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                        Log("CODEVOL_FIN WARNING 63.catch()", "@SHELLMX- OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA. Id_Teller : " + request.Id_teller.ToString() + " IDSEGUIMIENTO: " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = "OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA."
                        };
                    }
                    foreach (var searchOperator in searchOperatorResponse.OperatorList)
                    {
                        idOperatorObtTran = searchOperator.Id;
                        codeOperatorOntTran = searchOperator.Code;
                        nameOperator = searchOperator.Name.ToString();
                    }

                    #endregion

                    GetAllSupplyTransactionsOfFuellingPointRequest getAllSupplyTransactionsOfFuellingPointRequest = new GetAllSupplyTransactionsOfFuellingPointRequest { OperatorId = idOperatorObtTran, FuellingPointId = request.Pos_Carga };
                    GetAllSupplyTransactionsOfFuellingPointResponse getAllSupplyTransactionsOfFuellingPointResponse = conectionSignalRDoms.GetAllSupplyTransactionsOfFuellingPoint(getAllSupplyTransactionsOfFuellingPointRequest, criptoObt);

                    //Se verifica el estado del surtidor.
                    if (getAllSupplyTransactionsOfFuellingPointResponse.Status <= -1)
                    {
                        Log("CODEVOL_FIN ERROR 24.catch()", "@SHELLMX- SE PRODUJO UN ERROR EN EL PROCESO DE OBTENER EL SURTIDOR. IDSEGUIMIENTO: " + criptoObt + " : " + getAllSupplyTransactionsOfFuellingPointResponse.Message);
                        return new Salida_Obtiene_Tran { Resultado = false, Msj = getAllSupplyTransactionsOfFuellingPointResponse.Message };
                    }
                    if (getAllSupplyTransactionsOfFuellingPointResponse.Status == 0)
                    {
                        Log("CODEVOL_FIN WARNING 23.catch()", "@SHELLMX- SE PRODUJO UN ERROR EN EL PROCESO DE OBTENER EL SURTIDOR. IDSEGUIMIENTO: " + criptoObt + " : " + getAllSupplyTransactionsOfFuellingPointResponse.Message);
                        return new Salida_Obtiene_Tran { Resultado = false, Msj = getAllSupplyTransactionsOfFuellingPointResponse.Message };
                    }


                    SupplyTransaction supplyTransaction = null;
                    foreach (SupplyTransaction supply in getAllSupplyTransactionsOfFuellingPointResponse.SupplyTransactionList)
                    {
                        if (supplyTransaction == null)
                        {
                            supplyTransaction = supply;
                        }
                    }

                    if (supplyTransaction.LockingPOSId != null && supplyTransaction.FuellingPointId == request.Pos_Carga)
                    {
                        #region PROCESO DE VALIDACION DE DESBLOQUEO

                        UnlockSupplyTransactionOfFuellingPointRequest unlockSupplyTransactionOfFuellingPointRequest = new UnlockSupplyTransactionOfFuellingPointRequest
                        {
                            FuellingPointId = request.Pos_Carga,
                            OperatorId = idOperatorObtTran,
                            SupplyTransactionId = supplyTransaction.Id
                        };

                        UnlockSupplyTransactionOfFuellingPointResponse unlockSupplyTransactionOfFuellingPointResponse = conectionSignalRDoms.UnlockSupplyTransactionOfFuellingPointWS(unlockSupplyTransactionOfFuellingPointRequest);
                        if (unlockSupplyTransactionOfFuellingPointResponse.Status < 0)
                        {
                            Log("CODEVOL_FIN ERROR 55.catch()", "@SHELLMX- NO SE PUDO DESBLOQUEAR LA BOMBA NO SE APLICO EL REVERSO.  IDSEGUIMIENTO: " + criptoObt + "  LOG:: RESPONSE: " + "\n" + "UnlockSupplyTransactionOfFuellingPointResponse: {" + "\n" +
                                "    status: " + unlockSupplyTransactionOfFuellingPointResponse.Status.ToString() + "," + "\n" +
                                "    message: " + unlockSupplyTransactionOfFuellingPointResponse.Message.ToString() + "\n" + "}");
                            return new Salida_Obtiene_Tran
                            {
                                Msj = "FALLO EN OBTENER INFO DE SURTIDOR, NO SE PUDO DESBLOQUEAR LA BOMBA, VERIFICAR TPV.",
                                Resultado = false
                            };
                        }
                        Log("CODEVOL_FIN WARNING 22.catch()", "@SHELLMX- FALLO EN OBTENER INFO EN LA BOMBA, SE DESBLOQUEO LA BOMBA SATISFACTORIAMENTE CON EL SIGUIENTE RESPONSE. IDSEGUIMIENTO: " + criptoObt + "\n" + " DesbloquearCarga: {" + "\n" +
                            "    msj: " + "@ SHELLMX- SE HA INICIADO EL DESBLOQUEO DE LA BOMBA : " + request.Pos_Carga.ToString() + " CON STATUS : " + unlockSupplyTransactionOfFuellingPointResponse.Status.ToString() + "," + "\n" +
                            "    resultado: " + "true" + "\n" + "}");

                        return new Salida_Obtiene_Tran { Msj = "FALLO OBTENER INFO LA BOMBA, DESBLOQUE0 BOMBA EXITOSO BOMBA: " + request.Pos_Carga + " OBTENER NUEVAMENTE LA BOMBA. ", Resultado = false };
                        #endregion
                    }

                    if(supplyTransaction.FuellingPointId == request.Pos_Carga)
                    {
                        Log("CODEVOL_FIN WARNING 19" , "@SHELLMX- LA TERMINAR CON LA SERIE :" + request.serial.ToString() + " Y OPERADOR : " + nameOperator + " INTENTA TOMAR UNA CARGA QUE ESTA BLOQUEADA POR OTRO OPERADOR IDSEGUIMIENTO: " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Msj = "LA BOMBA : " + request.Pos_Carga.ToString() + " ESTA BLOQUEADA POR OTRO OPERADOR.",
                            Resultado = false
                        };
                    }
                    else
                    {
                        Log("CODEVOL_FIN ERROR 21.catch()", "@SHELLMX- NO SE PUDO DESBLOQUEO LA BOMBA FALLO. IDSEGUIMIENTO: " + criptoObt);

                        return new Salida_Obtiene_Tran { Msj = "FALLO OBTENER INFO DE LA BOMBA, Y NO SE PUDO DESBLOQUEAR LA BOMBA: " + request.Pos_Carga.ToString() + " REVISAR LOGS.", Resultado = false };
                    }
                }
                else
                {
                    Log("CODEVOL_FIN ERROR 20.catch()", "@SHELLMX - ERROR Al ENTRAR AL METODO DE ONTENER_TRAN" + "  IDSEGUIMIENTO: " + criptoObt + " LOG: " + ext.ToString());
                    return new Salida_Obtiene_Tran
                    {
                        Msj = "ERROR INTERNO AL OBTENER LA BOMBA Y NO SE REALIZO EL DESBLOQUEO.",
                        Resultado = false
                    };
                }*/
                //Se manda a verificar el estado del surtidor si tiene data en entrada.
                if (request.Id_teller != null && request.Pos_Carga != 0)
                {
                    if (request.Pos_Carga < 0)
                    {
                        //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                        Log("CODEVOL_FIN 67.catch()", "@SHELLMX- DEBE DE INSERTAR UN SURTIDOR QUE ESTA LIGADO Pos_Carga: " + request.Pos_Carga.ToString() + " IDSEGUIMIENTO: " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = "DEBE DE INSERTAR UN SURTIDOR VALIDO BOMBA: " + request.Pos_Carga.ToString()
                        };
                    }

                    ConectionSignalRDoms conectionSignalRDoms = new ConectionSignalRDoms();

                    if (conectionSignalRDoms.StatusConectionHubbleR() < 0)
                    {
                        Log("CODEVOL_FIN 64.catch()", "SHELLHUBLE- FALLO LA CONEXION CON EL DOMS VERIFICAR QUE ESTE CONECTADO! RESPONSE: " + conectionSignalRDoms.StatusConectionHubbleR().ToString() + " IDSEGUIMIENTO: " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = "FALLO LA CONEXCIÓN CON EL DOMS VERIFICAR SU ESTADO EN TPV",
                        };
                    }

                    //SHELLMX- Indentificamos que el Operador este registrado en el Sistema de Everilion.Shell
                    // SHELLMX- Se consigue el Token del TPV para hacer las pruebas. 
                    var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
                    TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
                    InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();

                    #region VALIDACION DEL OPERADOR ID | CODE
                    List<SearchOperatorCriteria> SearchOperatorCriteriaOperator = new List<SearchOperatorCriteria>
                        {
                            new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Id , MatchingType = SearchOperatorCriteriaMatchingType.Exact },
                            new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Code , MatchingType = SearchOperatorCriteriaMatchingType.Exact }
                        };
                    SearchOperatorRequest searchOperatorRequest = new SearchOperatorRequest
                    {
                        Identity = bsObj.Identity,
                        CriteriaList = SearchOperatorCriteriaOperator,
                        CriteriaRelationshipType = SearchCriteriaRelationshipType.Or,
                        MustIncludeDischarged = false
                    };

                    SearchOperatorResponse searchOperatorResponse = await invokeHubbleWebAPIServices.SearchOperator(searchOperatorRequest);
                    string idOperatorObtTran = null;
                    string codeOperatorOntTran = null;
                    string nameOperator = null;

                    if (searchOperatorResponse.OperatorList.Count == 0)
                    {
                        //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                        Log("CODEVOL_FIN 63.catch()", "@SHELLMX- OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA. Id_Teller : " + request.Id_teller.ToString() + " IDSEGUIMIENTO: " + criptoObt);
                        return new Salida_Obtiene_Tran
                        {
                            Resultado = false,
                            Msj = "OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                        };
                    }
                    foreach (var searchOperator in searchOperatorResponse.OperatorList)
                    {
                        idOperatorObtTran = searchOperator.Id;
                        codeOperatorOntTran = searchOperator.Code;
                        nameOperator = searchOperator.Name.ToString();
                    }

                    #endregion

                    GetAllSupplyTransactionsOfFuellingPointRequest getAllSupplyTransactionsOfFuellingPointRequest = new GetAllSupplyTransactionsOfFuellingPointRequest { OperatorId = idOperatorObtTran, FuellingPointId = request.Pos_Carga };
                    GetAllSupplyTransactionsOfFuellingPointResponse getAllSupplyTransactionsOfFuellingPointResponse = conectionSignalRDoms.GetAllSupplyTransactionsOfFuellingPoint(getAllSupplyTransactionsOfFuellingPointRequest, criptoObt);

                    //Se verifica el estado del surtidor.
                    if (getAllSupplyTransactionsOfFuellingPointResponse.Status == -99)
                    {
                        Log("CODEVOL_FIN 24.catch()", "@SHELLMX - SE PRODUJO UN ERROR EN LO SIG:  IDSEGUIMIENTO: " + criptoObt + " : " + getAllSupplyTransactionsOfFuellingPointResponse.Message);
                        return new Salida_Obtiene_Tran { Resultado = false, Msj = "NO HAY TRANSACCIONES EN EL SURTIDOR VERIFICAR QUE TENGA UNA RECARGA CH" };
                    }
                    if (getAllSupplyTransactionsOfFuellingPointResponse.Status < 0)
                    {
                        Log("CODEVOL_FIN 23.catch()", "@SHELLMX -  IDSEGUIMIENTO: " + criptoObt + " : " + getAllSupplyTransactionsOfFuellingPointResponse.Message);
                        return new Salida_Obtiene_Tran { Resultado = false, Msj = "NO HAY TRANSACCIONES EN EL SURTIDOR VERIFICAR QUE TENGA UNA RECARGA CH" };
                    }
                    if (getAllSupplyTransactionsOfFuellingPointResponse.SupplyTransactionList.Count() == 0)
                    {
                        Log("CODEVOL_FIN 22.catch()", "@SHELLMX - NULL NO HAY INFORMACION EN SURTIDOR. IDSEGUIMIENTO: " + criptoObt + " : " + getAllSupplyTransactionsOfFuellingPointResponse.Message);
                        return new Salida_Obtiene_Tran { Resultado = false, Msj = "NO HAY TRANSACCIONES EN EL SURTIDOR VERIFICAR QUE TENGA UNA RECARGA CH" };
                    }

                    SupplyTransaction supplyTransaction = null;
                    foreach (SupplyTransaction supply in getAllSupplyTransactionsOfFuellingPointResponse.SupplyTransactionList)
                    {
                        if (supplyTransaction == null)
                        {
                            supplyTransaction = supply;
                        }
                    }

                    if (supplyTransaction.LockingPOSId != null && supplyTransaction.FuellingPointId == request.Pos_Carga)
                    {
                        #region PROCESO DE VALIDACION DE DESBLOQUEO

                        UnlockSupplyTransactionOfFuellingPointRequest unlockSupplyTransactionOfFuellingPointRequest = new UnlockSupplyTransactionOfFuellingPointRequest
                        {
                            FuellingPointId = request.Pos_Carga,
                            OperatorId = idOperatorObtTran,
                            SupplyTransactionId = supplyTransaction.Id
                        };

                        UnlockSupplyTransactionOfFuellingPointResponse unlockSupplyTransactionOfFuellingPointResponse = conectionSignalRDoms.UnlockSupplyTransactionOfFuellingPointWS(unlockSupplyTransactionOfFuellingPointRequest);
                        if (unlockSupplyTransactionOfFuellingPointResponse.Status < 0)
                        {
                            Log("CODEVOL_FIN 55.catch()", "@SHELLMX- NO SE PUDO DESBLOQUEAR LA BOMBA NO SE APLICO EL REVERSO.  IDSEGUIMIENTO: " + criptoObt + "  LOG:: RESPONSE: " + "\n" + "UnlockSupplyTransactionOfFuellingPointResponse: {" + "\n" +
                                "    status: " + unlockSupplyTransactionOfFuellingPointResponse.Status.ToString() + "," + "\n" +
                                "    message: " + unlockSupplyTransactionOfFuellingPointResponse.Message.ToString() + "\n" + "}");
                            return new Salida_Obtiene_Tran
                            {
                                Msj = "FALLO OBTENER INFO LA BOMBA, Y NO SE PUDO DESBLOQUEAR LA BOMBA",
                                Resultado = false
                            };
                        }
                        Log("CODEVOL_FIN 22.catch()", "@SHELLMX- SE DESBLOQUEO LA BOMBA SATISFACTORIAMENTE CON EL SIGUIENTE RESPONSE. IDSEGUIMIENTO: " + criptoObt + "\n" + " DesbloquearCarga: {" + "\n" +
                            "    msj: " + "@ SHELLMX- SE HA INICIADO EL DESBLOQUEO DE LA BOMBA : " + request.Pos_Carga.ToString() + " CON STATUS : " + unlockSupplyTransactionOfFuellingPointResponse.Status.ToString() + "," + "\n" +
                            "    resultado: " + "true" + "\n" + "}");

                        return new Salida_Obtiene_Tran { Msj = "FALLO OBTENER INFO LA BOMBA, DESBLOQUEO BOMBA EXITOSO BOMBA: " + request.Pos_Carga + " OBTENER NUEVAMENTE LA BOMBA ", Resultado = false };
                        #endregion
                    }
                    else
                    {
                        Log("CODEVOL_FIN 21.catch()", "@SHELLMX- NO SE PUDO DESBLOQUEO LA BOMBA FALLO. IDSEGUIMIENTO: " + criptoObt);

                        return new Salida_Obtiene_Tran { Msj = "FALLO OBTENER INFO LA BOMBA: " + request.Pos_Carga.ToString() + "  Y NO SE PUDO DESBLOQUEAR", Resultado = false };
                    }
                }
                else
                {
                    Log("CODEVOL_FIN 22.catch()", "@SHELLMX - ERROR Al ENTRAR AL METODO DE ONTENER_TRAN" + "  IDSEGUIMIENTO: " + criptoObt + " LOG:: " + ext.ToString());
                    return new Salida_Obtiene_Tran
                    {
                        Msj = "ERROR INTERNO AL OBTENER LA BOMBA Y NO SE REALIZO EL DESBLOQUEO",
                        Resultado = false
                    };
                }
            }
            /*finally  // <--  termine bien o mal liberamos el semaforo
            {
                try
                {
                    SemaphoreSlimObtener_tran?.Release();
                }
                catch (Exception)
                {
                }
            }*/
            return salida_Obtiene_Tran;
        }

        public async Task<Salida_Info_Forma_Pago> Info_Forma_Pago(Entrada_Info_Forma_Pagos request)
        {
            Salida_Info_Forma_Pago salida_Info_Forma_Pago = new Salida_Info_Forma_Pago();
           
            var criptoInfoFor = DateTime.Now.ToString("yyyyMM") + "_INFOPAY_" + DateTime.Now.ToString("h-mm-ss-ffff");
            //var idSupplyValidatorPreview;
            try
            {
                // si está ocupado se espera.
                //await SemaphoreSlimInfo_Forma_Pago.WaitAsync();

                Log("CODEVOL_INI INFO", "@SHELLMX- SE INICIA LA VENTA DE CARBURANTE/PERIFERICOS EN EL METODO INFO_FORMA_PAGO  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor);
                string varRequest = String.Empty;
                foreach (Entrada_Info_Forma_Pago_List entrada_Info_Forma_Pago_List in request.Info_Forma_Pago)
                {
                    varRequest += varRequest != "" ? "/" : varRequest;
                    varRequest += entrada_Info_Forma_Pago_List.Cantidad + "," +
                                  entrada_Info_Forma_Pago_List.Id_product + "," +
                                  entrada_Info_Forma_Pago_List.Importe_Unitario + "," +
                                  entrada_Info_Forma_Pago_List.Importetotal + "," +
                                  //entrada_Info_Forma_Pago_List.Producto + "," + 
                                  entrada_Info_Forma_Pago_List.nNum_autorizacions + "," +
                                  entrada_Info_Forma_Pago_List.formapagos + "," +
                                  entrada_Info_Forma_Pago_List.montoPagadoParcial + "," +
                                  entrada_Info_Forma_Pago_List.Ultimos_Digitoss;
                }
                string[] tupleProducts = varRequest.Split('/');
                string[] tupleRequestCom = tupleProducts[0].Split(',');
                string[] tupleRequestPer = tupleProducts.Length == 2 ? tupleProducts[1].Split(',') : null;

                if (tupleProducts.Length == 2)
                {
                    Log("CODEVOL_TR INFO", "SOLICITUD PARA CONSUMIR EL METODO ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "  IDSEGUIMIENTO: " + criptoInfoFor + "\n Info_Forma_Pago: " + "\n" + "{" + "\n" +
                        "    Id_Transaccion : " + request.Id_Transaccion.ToString() + "," + "\n" +
                        "    Id_Teller : " + request.Id_teller.ToString() + "," + "\n" +
                        "    Id_Interno_POS : " + request.idInternoPOS.ToString() + "," + "\n" +
                        "    IdPOS: " + request.idpos.ToString() + "," + "\n" +
                        "    Entrada_Info_Forma_Pago_List_Combustible: " + "\n" +
                        "      [" + "\n" +
                        "        {" + "\n" +
                        "          id_Producto: " + tupleRequestCom[1] + "," + "\n" +
                        "          cantidad: " + tupleRequestCom[0] + "," + "\n" +
                        "          importeUnitario: " + tupleRequestCom[2] + "," + "\n" +
                        "          importeTotal: " + tupleRequestCom[3] + "," + "\n" +
                        "          formasPago: " + tupleRequestCom[4] + "," + "\n" +
                        "          montoaPagoParcial: " + tupleRequestCom[5] + "\n" +
                        "          numAuthCards: " + tupleRequestCom[6] + "\n" +
                        "          ultimosDigitosCards: " + tupleRequestCom[4] + "\n" +
                        "        }" + "\n" +
                        "      ]," + "\n" +
                        "    Entrada_Info_Forma_Pago_List_Perifericos: " + "\n" +
                        "      [" + "\n" +
                        "        {" + "\n" +
                        "          id_Producto: " + tupleRequestPer[1] + "," + "\n" +
                        "          cantidad: " + tupleRequestPer[0] + "," + "\n" +
                        "          importeUnitario: " + tupleRequestPer[2] + "," + "\n" +
                        "          importeTotal: " + tupleRequestPer[3] + "," + "\n" +
                        "          formasPago: " + tupleRequestPer[4] + "," + "\n" +
                        "          montoaPagoParcial: " + tupleRequestPer[5] + "\n" +
                        "          numAuthCards: " + tupleRequestPer[6] + "\n" +
                        "          ultimosDigitosCards: " + tupleRequestPer[4] + "\n" +
                        "        }" + "\n" +
                        "      ]," + "\n" +
                        "    IvaProducto: " + request.IvaProducto + "," + "\n" +
                        "    nHD: " + request.nHD + "," + "\n" +
                        "    parciales: " + request.parciales.ToString() + "," + "\n" +
                        "    Porpagarentrada: " + request.PorpagarEntrada.ToString() + "," + "\n" +
                        "    Pos_Carga: " + request.Pos_Carga.ToString() + "\n" + "}");
                }
                else
                {
                    Log("CODEVOL_TR INFO", "SOLICITUD PARA CONSUMIR EL METODO ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "  IDSEGUIMIENTO: " + criptoInfoFor + "\n Info_Forma_Pago: " + "\n" + "{" + "\n" +
                        "    Id_Transaccion : " + request.Id_Transaccion.ToString() + "," + "\n" +
                        "    Id_Teller : " + request.Id_teller.ToString() + "," + "\n" +
                        "    Id_Interno_POS : " + request.idInternoPOS.ToString() + "," + "\n" +
                        "    IdPOS: " + request.idpos.ToString() + "," + "\n" +
                        "    Entrada_Info_Forma_Pago_List_Combustible: " + "\n" +
                        "      [" + "\n" +
                        "        {" + "\n" +
                        "          id_Producto: " + tupleRequestCom[1] + "," + "\n" +
                        "          cantidad: " + tupleRequestCom[0] + "," + "\n" +
                        "          importeUnitario: " + tupleRequestCom[2] + "," + "\n" +
                        "          importeTotal: " + tupleRequestCom[3] + "," + "\n" +
                        "          formasPago: " + tupleRequestCom[4] + "," + "\n" +
                        "          montoaPagoParcial: " + tupleRequestCom[5] + "\n" +
                        "          numAuthCards: " + tupleRequestCom[6] + "\n" +
                        "          ultimosDigitosCards: " + tupleRequestCom[4] + "\n" +
                        "        }" + "\n" +
                        "      ]," + "\n" +
                        "    IvaProducto: " + request.IvaProducto + "," + "\n" +
                        "    nHD: " + request.nHD + "," + "\n" +
                        "    parciales: " + request.parciales.ToString() + "," + "\n" +
                        "    Porpagarentrada: " + request.PorpagarEntrada.ToString() + "," + "\n" +
                        "    Pos_Carga: " + request.Pos_Carga.ToString() + "\n" + "}");
                }
                if (request.Id_Transaccion == null)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    Log("CODEVOL_FIN WARNING 99", "@SHELLMX- NUMERO DE TRANSACCION VACIO INTRODUCIR EL SURTIDOR ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "IDSEGUIMIENTO: " + criptoInfoFor);
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "NUMERO DE TRANSACCION VACIO VERIFICAR LOGS.",
                    };
                }
                try
                {
                    if (Convert.ToInt32(request.Id_Transaccion) <= -1)  // Se coloca 0 por la parte del Efectivo.
                    {
                        //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                        Log("CODEVOL_FIN  WARNING 98", "@SHELLMX- INTRODUCIR UN NUMERO DE SURTIDOR VALIDO ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor);
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "INTRODUCIR UN NUMERO DE SURTIDOR VALIDO.",
                        };
                    }
                }
                catch (Exception e)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    Log("CODEVOL_FIN ERROR 97", "@SHELLMX- NO ES UN VALOR VALIDO ID_TRANSACTION VERIFICAR ---> ID_TRANSACTION: " +  request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + " LOG: " + e.ToString());
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "NO ES UN VALOR VALIDO IDTRAN DEL SURTIDOR VERIFICAR LOGS."
                    };
                    throw e;
                }

                try
                {
                    if (request.idpos == null || request.nHD <= 0 || request.PorpagarEntrada <= -1)
                    {
                        //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                        Log("CODEVOL_FIN WARNING 96", "@SHELLMX- DATOS VALIDOS EN IDPOS | nHD | PorpagarEntrada VALIDAR. ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "IDSEGUIMIENTO: " + criptoInfoFor);
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "DATOS VALIDOS EN ENTRADA VALIDAR QUE SEAN CORRECTOS.",
                        };
                    }
                }
                catch (Exception e)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    Log("CODEVOL_FIN ERROR 95", "@SHELLMX- DATOS CON FORMATO INCORRECTO EN IDPOS | nHD | PorpagarEntrada VALIDAR  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "IDSEGUIMIENTO: " + criptoInfoFor +" Log: " + e.ToString());
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "ERROR EN VALIDACION DE DATOS ENTRANTES REVISAR LOGS.",
                    };
                    throw e;
                }

                if (request.Id_teller == null)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    Log("CODEVOL_FIN WARNING 94", "@SHELLMX-  OPERADOR ESTA VACIO EN LA ENTRADA VALIDAR. ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor);
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "OPERADOR ESTA VACIO EN LA ENTRADA VALIDAR.",
                    };
                }


                //SHELLMX- Indentificamos que el Operador este registrado en el Sistema de Everilion.Shell
                // SHELLMX- Se consigue el Token del TPV para hacer las pruebas. 
                var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
                TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
                InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();

                #region VALIDACION DEL OPERADOR ID | CODE
                List<SearchOperatorCriteria> SearchOperatorCriteriaOperator = new List<SearchOperatorCriteria>
                {
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Id , MatchingType = SearchOperatorCriteriaMatchingType.Exact },
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Code , MatchingType = SearchOperatorCriteriaMatchingType.Exact }
                };
                SearchOperatorRequest searchOperatorRequest = new SearchOperatorRequest
                {
                    Identity = bsObj.Identity,
                    CriteriaList = SearchOperatorCriteriaOperator,
                    CriteriaRelationshipType = SearchCriteriaRelationshipType.Or,
                    MustIncludeDischarged = false
                };

                SearchOperatorResponse searchOperatorResponse = await invokeHubbleWebAPIServices.SearchOperator(searchOperatorRequest);
                string idOperator = null;
                string codeOperator = null;
                string nameOperator = null;

                if (searchOperatorResponse.OperatorList.Count == 0)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    Log("CODEVOL_FIN WARNING 93", "@SHELLMX- ERROR OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor);
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                    };
                }
                foreach (var searchOperator in searchOperatorResponse.OperatorList)
                {
                    idOperator = searchOperator.Id;
                    codeOperator = searchOperator.Code;
                    nameOperator = searchOperator.Name;
                }

                #endregion

                #region VALIDACION SOBRE EL REGISTRO DE LA TARJETA.
                // Se colocara la validacion si la tarjeta ha sido rechazada o paso de manera correcta para la venta.
                // si la condicion es verdadera se debe desbloquear la bomba. :)

                ConectionSignalRDoms conectionSignalRDomsInform = new ConectionSignalRDoms();

                // Se coloca esta parte para amaagar de que venga los objetos en la parte de Carburante y lubricantes.
                if (request.aprobado == false && Convert.ToInt32(request.Id_Transaccion) > 0)
                {
                    //conectionSignalRDomsInform.UnlockSupplyTransactionOfFuellingPointWS(Convert.ToInt32(request.Id_Transaccion), request.Pos_Carga, idOperator); --NO SE USA


                    UnlockSupplyTransactionOfFuellingPointRequest unlockSupplyTransactionOfFuellingPointRequest = new UnlockSupplyTransactionOfFuellingPointRequest
                    {
                        FuellingPointId = request.Pos_Carga,
                        OperatorId = idOperator,
                        SupplyTransactionId = Convert.ToUInt16(request.Id_Transaccion)
                    };

                    UnlockSupplyTransactionOfFuellingPointResponse unlockSupplyTransactionOfFuellingPointResponse = conectionSignalRDomsInform.UnlockSupplyTransactionOfFuellingPointWS(unlockSupplyTransactionOfFuellingPointRequest);
                    if (unlockSupplyTransactionOfFuellingPointResponse.Status < 0)
                    {
                        Log("CODEVOL_FIN ERROR 92", "@SHELLMX- NO SE PUDO DESBLOQUEAR LA BOMBA ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor + "\n LOG: " + "\n" +
                            "UnlockSupplyTransactionOfFuellingPointResponse: \n {" + "\n" +
                            "     " + unlockSupplyTransactionOfFuellingPointResponse.Status.ToString() + "," + "\n" +
                            "     " + unlockSupplyTransactionOfFuellingPointResponse.Message.ToString() + "\n" + "}");
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = true,
                            Msj = "NO SE PUDO DESBLOQUEAR LA BOMBA VERIFICAR LOGS Y STATUS EN TPV."
                        };
                    }
                    Log("CODEVOL_FIN INFO 91 PC", "@SHELLMX- TARJETA NO APROBADA, SE INICIA EL DESBLOQUEO DE LA BOMBA : " + request.Pos_Carga.ToString() + " CON STATUS : " + unlockSupplyTransactionOfFuellingPointResponse.Status + " --->ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor);
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = true,
                        Msj = "TARJETA NO APROBADA, SE INICIA EL DESBLOQUEO DE LA BOMBA : " + request.Pos_Carga.ToString() + " CON STATUS : " + unlockSupplyTransactionOfFuellingPointResponse.Status
                    };
                }

                #endregion

                if (request.Info_Forma_Pago == null || request.Info_Forma_Pago.Count == 0)
                {
                    //SHELLMX- Se manda una excepccion de que no esta lleno el valor del Inform.
                    Log("CODEVOL_FIN WARNING 90", "@SHELLMX- INFORM DE LA VENTA VACIO CARGAR LOS DATOS DE VENTA ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "IDSEGUIMIENTO: " + criptoInfoFor);
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "INFORMACION DE LA VENTA VACIO CARGAR LOS DATOS DE VENTA, VERIFICAR LOGS.",
                    };
                }

                #region CONFIGURACION PARA EL DOMS
                //ConectionSignalRDoms conectionSignalRDomsInform = new ConectionSignalRDoms();   ---- NO SE USA
                GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest { Identity = bsObj.Identity };
                GetPOSInformationResponse getPOSInformationResponse = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);

                if (request.Info_Forma_Pago.Count == 2 || Convert.ToInt32(request.Id_Transaccion) != 0)
                {
                    if (conectionSignalRDomsInform.StatusConectionHubbleR() < 0)
                    {
                        //SHELLMX- Se manda una excepccion de que no esta lleno el valor del Inform.
                        Log("CODEVOL_FIN ERROR 89", "@SHELLMX- Fallo la conexion con el DOMS Verificar que este conectado. " + "StatusConectionHubbleR : " + conectionSignalRDomsInform.StatusConectionHubbleR().ToString() + " --->ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor);
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "FALLO LA CONEXION CON EL DOMS VERIFICAR STATUS EN TPV.",
                        };
                    }

                    #region SE VALIDA EL ID_TRANSACTION QUE CORRESPONDA AL SURTIDOR

                    GetAllSupplyTransactionsOfFuellingPointRequest getAllSupplyTransactionsOfFuellingPoint = new GetAllSupplyTransactionsOfFuellingPointRequest
                    {
                        OperatorId = idOperator,
                        FuellingPointId = request.Pos_Carga
                    };

                    #region JARREOS 
                    //----------------------------jarreos----------------------------------               

                    try
                    {
                        string jarreo = tupleRequestCom[5];
                        if (jarreo == "96")
                        {
                            GetAllSupplyTransactionsOfFuellingPointResponse info_Bomba = conectionSignalRDomsInform.GetAllSupplyTransactionsOfFuellingPointJarreo(getAllSupplyTransactionsOfFuellingPoint, criptoInfoFor, true);

                            string gradeReference = "";
                            foreach (var supplyValidate in info_Bomba.SupplyTransactionList)
                            {
                                gradeReference = supplyValidate.GradeReference;
                            }

                            //ConectionSignalRDoms conectionSignalRDoms = new ConectionSignalRDoms();

                            FinalizeSupplyTransactionForFuelTestRequest FinalizeSupplyTransactionForFuelTestrequest = new FinalizeSupplyTransactionForFuelTestRequest
                            {
                                SupplyTransactionId = Convert.ToInt32(request.Id_Transaccion),
                                FuellingPointId = request.Pos_Carga,
                                OperatorId = idOperator,
                                ReturnTankId = gradeReference,
                                Deviation = 3,
                                Observations = "",


                            };
                            Log("CODE JARREO: ", " Request= SupplyTransactionId: " + FinalizeSupplyTransactionForFuelTestrequest.SupplyTransactionId
                                + " , FuellingPointId: " + FinalizeSupplyTransactionForFuelTestrequest.FuellingPointId
                                + " , OperatorId: " + FinalizeSupplyTransactionForFuelTestrequest.OperatorId
                                + " , ReturnTankId: " + FinalizeSupplyTransactionForFuelTestrequest.ReturnTankId
                                + " , Deviation: " + FinalizeSupplyTransactionForFuelTestrequest.Deviation
                                + " , Observations: " + FinalizeSupplyTransactionForFuelTestrequest.Observations
                                );
                            FinalizeSupplyTransactionForFuelTestResponse FinalizeSupplyTransactionForFuelTestresponse = conectionSignalRDomsInform.FinalizeSupplyTransactionForFuelTestWS(FinalizeSupplyTransactionForFuelTestrequest);


                            if (FinalizeSupplyTransactionForFuelTestresponse.Status == 1)
                            {
                                Log("CODE JARREO: ", FinalizeSupplyTransactionForFuelTestresponse.Message.ToString() + ", Status: " + FinalizeSupplyTransactionForFuelTestresponse.Status.ToString());

                                return new Salida_Info_Forma_Pago
                                {
                                    Msj = "JARREO REALIZADO CON EXITO",
                                    Resultado = true
                                };
                            }

                            else
                            {
                                Log("CODE JARREO: ", FinalizeSupplyTransactionForFuelTestresponse.Message.ToString() + ", Status: " + FinalizeSupplyTransactionForFuelTestresponse.Status.ToString());
                                return new Salida_Info_Forma_Pago
                                {
                                    Msj = "JARREO NO REALIZADO DE MANERA EXITOSA",
                                    Resultado = false
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("CODE JARREO: ", "Ocurrió una excepción, " + ex.Message.ToString());
                        return new Salida_Info_Forma_Pago
                        {
                            Msj = "OCURRIO UNA EXEPCION AL REALIZAR EL JARREO",
                            Resultado = false

                        };
                    }


                    //------------------------termina jarreos-----------------------------
                    #endregion

                    int[] validateFuellingPointO = await conectionSignalRDomsInform.ValidateSupplyTransactionOfFuellingPoint(bsObj.Identity, getAllSupplyTransactionsOfFuellingPoint, criptoInfoFor, request.Id_Transaccion);
                    if (validateFuellingPointO[0] == -99 && validateFuellingPointO[0] == -99) // TEORICAMENTE JAMAS DEBE DE ENTRAR EN ESTA CONDICION..
                    {
                        Log("CODEVOL_FIN ERROR", "@SHELLMX- SE PRODUJO UN ERROR DE CASTEO POR PARTE DEL IDINTERNOPOS, NO SE PUDO FINALIZAR LA VENTA. ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "IDSEGUIMIENTO: " + criptoInfoFor);
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "EL ID NO EXISTE EN EL SURTIDOR, VERIFICAR LOGS EN TPV.",
                        };
                    }
                    if (validateFuellingPointO[0] == -98 && validateFuellingPointO[0] == -98) //SURGUE EL INTENTO DE BLOQUEO FALLIDO.
                    {
                        Log("CODEVOL_FIN ERROR", "@SHELLMX- SE TERMINO FALLIDO LA TRANSACCION DE LA VENTA ERROR DEL IDINTERNOPOS POR PARTE DEL PSSCONTROLLER, NO SE GENERO LA VENTA.  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "IDSEGUIMIENTO: " + criptoInfoFor);
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "NO EXISTE UN BLOQUEO EN LA BOMBA " + request.Pos_Carga.ToString() + " , NO SE TERMINO DE HACER LA VENTA, VERIFICAR TPV.",
                        };
                    }
                    if (validateFuellingPointO[0] <= -1 && validateFuellingPointO[0] <= -1)
                    {
                        Log("CODEVOL_FIN ERROR", "@SHELLMX- SE PRODUJO UN ERROR EN EL PSSCONTROLLER DONDE LA BOMBA " + request.Pos_Carga.ToString() + " NO ENTREGO INFORMACION Y FINALIZA LA VENTA.  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "IDSEGUIMIENTO: " + criptoInfoFor);
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "NO EXISTE UN BLOQUEO EN LA BOMBA " + request.Pos_Carga.ToString() + " , NO SE TERMINO DE HACER LA VENTA, VERIFICAR TPV.",
                        };
                    }
                    if (validateFuellingPointO[0] == 0 && validateFuellingPointO[1] == 0)
                    {
                        Log("CODEVOL_FIN WARNING", "@SHELLMX- EL ID_TRANSACTION NO EXISTE EN EL SURTIDOR INTENTAR NUEVAMENTE ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor);
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "LA BOMBA " + request.Pos_Carga.ToString() + " SE ENCUENTRA BLOQUEADO POR OTRO OPERADOR, NO SE FINALIZO VENTA.",
                        };
                    }

                    #endregion
                }

                #endregion

                #region VALIDACION SOBRE EL PARCIAL DE LA ENTRADA.
                if (request.parciales)
                {
                    //SHELLMX- Se manda una excepccion de que no esta lleno el valor del Inform.
                    Log("CODEVOL_FIN INFO 84 PC", "@SHELLMX- INFORM VAliDACION DE PRIMERA ENTRADA VARIABLE PARCIALES = " + request.parciales.ToString() + " ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor);
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = true,
                        Msj = "INFORMACIÓN VALIDACION DE PRIMERA ENTRADA PARCIAL : " + request.parciales.ToString()
                    };
                }
                #endregion

                //SHELLMX- Se crea el vaciado de la informacion de la venta.
                List<Products> InformListProducts = new List<Products>();

                List<string[]> Combustible = new List<string[]>();
                List<string[]> Products = new List<string[]>();
                List<string[]> ProcessPayments = new List<string[]>();
                List<string[]> ProcessAmountOfSale = new List<string[]>();

                List<List<string[]>> CombustibleGlobal = new List<List<string[]>>();
                List<List<string[]>> ProductsGlobal = new List<List<string[]>>();

                #region PRODUCTO QUE SE ALMACENA EN PLATAFORMA
                //Es el nombre porque no recibo la parte de del ID.
                string[] Id_product = null;
                string[] cantidad = null;

                string[] importe_Unitario = null;
                string[] importe_Total = null;
                #endregion

                #region FORMA DE PAGO Y MONTO TOTAL
                string[] forma_Pago = null;
                string[] monto_Pagado = null;
                #endregion

                #region PEfectivo
                string[] forma_PagoCombu = null;
                string[] monto_PagadoCombu = null;
                #endregion

                #region PTarjeta
                string[] forma_PagoPeri = null;
                string[] monto_PagadoPeri = null;
                #endregion

                #region listForSale
                List<string[]> ProcessPaymentsCombu = new List<string[]>();
                List<string[]> ProcessAmountOfSaleCombu = new List<string[]>();

                List<string[]> ProcessPaymentsPeri = new List<string[]>();
                List<string[]> ProcessAmountOfSalePeri = new List<string[]>();
                #endregion

                List<int> countCombustible = new List<int>();
                bool flagCountCombustible = false;

                List<int> countProducts = new List<int>();
                bool flagCountProduct = false;

                List<int> countFormaPago = new List<int>();
                bool flagCountFormaPago = false;

                List<int> countMontoPagar = new List<int>();
                bool flagCountMontoPagar = false;

                // Variable para controlar el estus de la venta de perifericos.
                bool IsCombustibleEnabler = false;

                //Variables para el logs sobre el arreglo de productos de PC.
                /*string id_productoPC = null;
                string cantidadPC = null;
                string importeUnPC = null;
                string importeTPC = null;
                string formasPagPC = null;
                string montopagPC = null;
                string numAutCard = null;
                string cardNumCode = null;*/

                #region PROCESO DE SEPARACION DE LOS PRODUCTOS Y ALMACENAR LOS EN UNA LISTA
                try
                {
                    foreach (Entrada_Info_Forma_Pago_List varPrincipal in request.Info_Forma_Pago)
                    {
                        //Se verifica si es producto o combustible.
                        if (varPrincipal.Producto)
                        {
                            IsCombustibleEnabler = true;

                            //nNum_autorizacions = varPrincipal.nNum_autorizacions.Split('|');
                            //Ultimos_Digitoss = varPrincipal.Ultimos_Digitoss.Split('|');
                            Id_product = varPrincipal.Id_product.Split('|');
                            //productosName = varPrincipal.Id_product.Split('|');
                            cantidad = varPrincipal.Cantidad.Split('|');
                            importe_Unitario = varPrincipal.Importe_Unitario.Split('|');
                            importe_Total = varPrincipal.Importetotal.Split('|');
                            //forma_Pago = varPrincipal.formapagos.Split('|');
                            //monto_Pagado = varPrincipal.montoPagadoParcial.Split('|');
                            forma_PagoCombu = varPrincipal.formapagos.Split('|');
                            monto_PagadoCombu = varPrincipal.montoPagadoParcial.Split('|');
                            //iva = varPrincipal.IvaProducto.Split('|');

                            countCombustible.Add(Id_product.Length);
                            //countCombustible.Add(productosName.Length);
                            countCombustible.Add(cantidad.Length);
                            countCombustible.Add(importe_Unitario.Length);
                            countCombustible.Add(importe_Total.Length);

                            //countFormaPago.Add(forma_PagoCombu.Length);
                            //countMontoPagar.Add(monto_PagadoCombu.Length);
                            //Combustible.Add(nNum_autorizacions);
                            //Combustible.Add(Ultimos_Digitoss);
                            Combustible.Add(Id_product);
                            //Combustible.Add(productosName);
                            Combustible.Add(cantidad);
                            Combustible.Add(importe_Unitario);
                            Combustible.Add(importe_Total);
                            //ProcessPayments.Add(forma_Pago);
                            //ProcessAmountOfSale.Add(monto_Pagado);

                            ProcessPaymentsCombu.Add(forma_PagoCombu);
                            ProcessAmountOfSaleCombu.Add(monto_PagadoCombu);
                            //Combustible.Add(iva);
                            CombustibleGlobal.Add(Combustible);

                            //Proceso para almacenaje para los logs
                            /*id_productoPC = varPrincipal.Id_product;
                            cantidadPC = varPrincipal.Cantidad;
                            importeUnPC = varPrincipal.Importe_Unitario;
                            importeTPC = varPrincipal.Importetotal;
                            formasPagPC = varPrincipal.formapagos;
                            montopagPC = varPrincipal.montoPagadoParcial;
                            numAutCard = varPrincipal.nNum_autorizacions;
                            cardNumCode = varPrincipal.Ultimos_Digitoss;*/
                        }
                        else
                        {
                            //nNum_autorizacions = varPrincipal.nNum_autorizacions.Split('|');
                            //Ultimos_Digitoss = varPrincipal.Ultimos_Digitoss.Split('|');
                            Id_product = varPrincipal.Id_product.Split('|');
                            //productosName = varPrincipal.Id_product.Split('|');
                            cantidad = varPrincipal.Cantidad.Split('|');
                            importe_Unitario = varPrincipal.Importe_Unitario.Split('|');
                            importe_Total = varPrincipal.Importetotal.Split('|');
                            forma_Pago = varPrincipal.formapagos.Split('|');
                            monto_Pagado = varPrincipal.montoPagadoParcial.Split('|');

                            forma_PagoPeri = varPrincipal.formapagos.Split('|');
                            monto_PagadoPeri = varPrincipal.montoPagadoParcial.Split('|');
                            //iva = varPrincipal.IvaProducto.Split('|');

                            countProducts.Add(Id_product.Length);
                            //countProducts.Add(productosName.Length);
                            countProducts.Add(cantidad.Length);
                            countProducts.Add(importe_Unitario.Length);
                            countProducts.Add(importe_Total.Length);

                            //countFormaPago.Add(forma_Pago.Length);
                            //countMontoPagar.Add(monto_Pagado.Length);
                            //Products.Add(nNum_autorizacions);
                            //Products.Add(Ultimos_Digitoss);
                            Products.Add(Id_product);
                            //Products.Add(productosName);
                            Products.Add(cantidad);
                            Products.Add(importe_Unitario);
                            Products.Add(importe_Total);
                            //ProcessPayments.Add(forma_Pago);
                            //ProcessAmountOfSale.Add(monto_Pagado);

                            ProcessPaymentsPeri.Add(forma_PagoPeri);
                            ProcessAmountOfSalePeri.Add(monto_PagadoPeri);
                            //Products.Add(iva);
                            ProductsGlobal.Add(Products);

                            //Proceso para almacenaje para los logs
                            /*id_productoPC = varPrincipal.Id_product;
                            cantidadPC = varPrincipal.Cantidad;
                            importeUnPC = varPrincipal.Importe_Unitario;
                            importeTPC = varPrincipal.Importetotal;
                            formasPagPC = varPrincipal.formapagos;
                            montopagPC = varPrincipal.montoPagadoParcial;
                            numAutCard = varPrincipal.nNum_autorizacions;
                            cardNumCode = varPrincipal.Ultimos_Digitoss;*/
                        }
                    }
                }
                catch (Exception e)
                {
                    Log("CODEVOL_FIN ERROR 83", "@SHELLMX- ERRORES EN LA ENTREGA DE Info_Forma_PagoList ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor + "\n Log:: " + e.ToString());
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "ERROR EN LA INFORMACION DE LA VENTA VERIFICAR LOGS."
                    };
                    throw e;
                }

                /*Log("CODEVOL_TR INFO", "SOLICITUD PARA CONSUMIR EL METODO ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "  IDSEGUIMIENTO: " + criptoInfoFor + "\n Info_Forma_Pago: " + "\n" + "{" + "\n" +
                    "    Id_Transaccion : " + request.Id_Transaccion.ToString() + "," + "\n" +
                    "    Id_Teller : " + request.Id_teller.ToString() + "," + "\n" +
                    "    Id_Interno_POS : " + request.idInternoPOS.ToString() + "," + "\n" +
                    "    IdPOS: " + request.idpos.ToString() + "," + "\n" +
                    "    Entrada_Info_Forma_Pago_List: " + "\n" + 
                    "      [" + "\n" + 
                    "        {" + "\n" +
                    "          id_Producto: " + id_productoPC + "," + "\n" +
                    "          cantidad: " + cantidadPC + "," + "\n" +
                    "          importeUnitario: " + importeUnPC + "," + "\n" +
                    "          importeTotal: " + importeTPC + "," + "\n" +
                    "          formasPago: " + formasPagPC + "," + "\n" +
                    "          montoaPagoParcial: " + montopagPC + "\n" + 
                    "          numAuthCards: " + numAutCard + "\n" + 
                    "          ultimosDigitosCards: " + cardNumCode + "\n" +
                    "        }" + "\n" + 
                    "      ]," + "\n" +
                    "    IvaProducto: " + request.IvaProducto + "," + "\n" +
                    "    nHD: " + request.nHD + "," + "\n" +
                    "    parciales: " + request.parciales.ToString() + "," + "\n" +
                    "    Porpagarentrada: " + request.PorpagarEntrada.ToString() + "," + "\n" +
                    "    Pos_Carga: " + request.Pos_Carga.ToString() + "\n" + "}");*/
                Log("CODEVOL_TR INFO", "NOMBRE DEL OPERADOR QUE REALIZA LA VENTA --> " + nameOperator + " ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "  IDSEGUIMIENTO: " + criptoInfoFor);

                #region VERIFICAR LA LONGITUD QUE COINCIDAN EN LOS PRODUCTOS.

                // Validador para los Combustiblees.
                int countUniversalCombu = 0;
                if (IsCombustibleEnabler)
                {
                    int valOldCombu = -1;
                    foreach (int lengthCombu in countCombustible)
                    {
                        countUniversalCombu = lengthCombu;
                        valOldCombu = valOldCombu == -1 ? lengthCombu : valOldCombu;
                        flagCountCombustible = countUniversalCombu == valOldCombu ? true : false;
                        if (!flagCountCombustible)
                        {
                            Log("CODEVOL_FIN WARNING 82", "@SHELLMX- NO COINCIDE LA CANTIDAD DE PRODUCTOS CARBURANTES EN LA SOLICITUD VERIFICAR ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor);
                            return new Salida_Info_Forma_Pago
                            {
                                Resultado = false,
                                Msj = "ERROR EN LA ENTRADA DE DATOS, VERIFICAR LOGS.",
                            };
                        }
                        valOldCombu = lengthCombu;
                    }
                }

                int countUniversalProduc = 0;
                int valOldProduct = -1;
                foreach (int lengthProduc in countProducts)
                {
                    countUniversalProduc = lengthProduc;
                    valOldProduct = valOldProduct == -1 ? lengthProduc : valOldProduct;
                    flagCountProduct = countUniversalProduc == valOldProduct ? true : false;
                    if (!flagCountProduct)
                    {
                        Log("CODEVOL_FIN WARNING 81", "@SHELLMX- NO COINCIDE LA CANTIDAD DE PRODUCTOS PERIFARICOS EN LA SOLICITUD VERIFICAR ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor);
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "ERROR EN LA ENTRADA DE DATOS, VERIFICAR LOGS.",
                        };
                    }
                    valOldProduct = lengthProduc;
                }

                #endregion

                #region VERIFICAR LA LONGITUD DE FORMAPAGO Y MONTOPAGAR PARA PROCESO VENTA.
                int countUniversalCountMontoP = 0;
                int valOldMontoP = -1;
                foreach (int lengthMontoPagar in countMontoPagar)
                {
                    countUniversalCountMontoP = lengthMontoPagar;
                    valOldMontoP = valOldMontoP == -1 ? lengthMontoPagar : valOldMontoP;
                    flagCountMontoPagar = countUniversalCountMontoP == valOldMontoP ? true : false;
                    if (!flagCountMontoPagar)
                    {
                        Log("CODEVOL_FIN WARNING 80", "@SHELLMX- NO COINCIDE LA CANTIDAD DE LAS MONTOPAGAR REALIZADAS EN LA SOLICITUD VERIFICAR  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor);
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "ERROR EN LA ENTRADA DE DATOS, VERIFICAR LOGS.",
                        };
                    }
                    valOldMontoP = lengthMontoPagar;
                }

                int countUniversalFormPago;
                int valOldFormPago = -1;
                foreach (int lengthFormPago in countFormaPago)
                {
                    countUniversalFormPago = lengthFormPago;
                    valOldFormPago = valOldFormPago == -1 ? lengthFormPago : valOldFormPago;
                    flagCountFormaPago = countUniversalFormPago == valOldFormPago ? true : false;
                    if (!flagCountFormaPago)
                    {
                        Log("CODEVOL_FIN WARNING 79", "@SHELLMX- NO COINCIDE LA CANTIDAD DE LAS FORMAPAGO REALIZADAS EN LA SOLICITUD VERIFICAR  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor);
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "ERROR EN LA ENTRADA DE DATOS, VERIFICAR LOGS.",
                        };
                    }
                    valOldFormPago = lengthFormPago;
                }

                #endregion

                //Se separa para poder extraer la informacion sobre los productos y almacenarlos.
                //Se agrega el IDCOMPANY para que se haga la venta segura y no truene.

                decimal totalOriginalON = 0M;
                if (IsCombustibleEnabler)
                {
                    #region COMBUSTIBLE.
                    decimal Importe_TotalCOMBUSTIBLE = 0M;
                    List<Products> OlisAuxForPreview = new List<Products>();
                    List<Products> OlisAuxForRespaldo = new List<Products>();
                    // Variable para verificar la cantidad de del importe total del carburante.
                    //decimal totalAmountTOTALOIL = 0M;

                    //decimal Importe_TotalCOMBUSTIBLE = 0M;
                    try
                    {
                        for (int intCombu = 0; intCombu < countUniversalCombu; intCombu++)
                        {
                            int flagContCombu = 0;
                            foreach (List<string[]> combustibleGlobal in CombustibleGlobal)
                            {
                                Products producto = new Products();
                                foreach (string[] combustible in Combustible)
                                {
                                    for (int intValueCombustible = 0; intValueCombustible <= intCombu; intValueCombustible++)
                                    {
                                        switch (flagContCombu)
                                        {
                                            case 0:
                                                producto.Id_producto = getPOSInformationResponse.PosInformation.CompanyCode + combustible[intValueCombustible];
                                                break;
                                            case 1:
                                                producto.Cantidad = Convert.ToDecimal(combustible[intValueCombustible]);
                                                break;
                                            case 2:
                                                producto.Importe_Unitario = Convert.ToDecimal(combustible[intValueCombustible]);
                                                break;
                                            case 3:
                                                producto.Importe_Total = Convert.ToDecimal(combustible[intValueCombustible]);
                                                Importe_TotalCOMBUSTIBLE = Importe_TotalCOMBUSTIBLE + Convert.ToDecimal(combustible[intValueCombustible]);
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    flagContCombu++;
                                }
                                //InformListProducts.Add(producto);
                                OlisAuxForPreview.Add(producto);
                                producto = null;
                            }
                        }

                        // VERIFICADOR DE QUE UNIFIFICAR SI UN PRODUCTO ESTA REPETIDO.

                        foreach (Products oilsPreview in OlisAuxForPreview)
                        {
                            // Se coloca la condicion para obtener el total de la unificacion sobre el carburante.
                            decimal amountOilsP = 0M;

                            foreach (Products oilsPreviewCountPoint in OlisAuxForPreview)
                            {
                                if (oilsPreviewCountPoint.Id_producto == oilsPreview.Id_producto)
                                {
                                    amountOilsP = amountOilsP + oilsPreviewCountPoint.Importe_Total;
                                }
                            }

                            // Se setea los demas valores porque pertenece al mismo objeto a invocar.
                            // En esta condicion se setea el primer objeto.
                            if (InformListProducts.Count == 0)
                            {
                                oilsPreview.Importe_Total = amountOilsP;

                                // Se obtiene los valores originales para la comparacion sobre el importe total de entrada y la original.
                                //totalAmountTOTALOIL = amountOilsP;
                                totalOriginalON = oilsPreview.Cantidad * oilsPreview.Importe_Unitario;

                                InformListProducts.Add(oilsPreview);
                                OlisAuxForRespaldo.Add(oilsPreview);
                            }
                            else
                            {
                                // Si ya se tiene un valor en la parte de la lista global se compara para que no este repetido el ID del producto.
                                //Products productRespado = new Products();
                                bool flag = false;
                                foreach (Products oilsGlobals in OlisAuxForRespaldo)
                                {
                                    if (oilsGlobals.Id_producto != oilsPreview.Id_producto)
                                    {
                                        oilsGlobals.Importe_Total = amountOilsP;
                                        InformListProducts.Add(oilsPreview);
                                        flag = true;
                                    }
                                }
                                if (flag)
                                {
                                    OlisAuxForRespaldo.Add(new Products { Cantidad = oilsPreview.Cantidad, Id_producto = oilsPreview.Id_producto, Importe_Total = oilsPreview.Importe_Total, Importe_Unitario = oilsPreview.Importe_Unitario });
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log("CODEVOL_FIN ERROR 78", "@SHELLMX- EN LA CANTIDAD DE LOS PRODUCTO DE CARBURANTE VENGAN CON CARACTER ESPECIAL Log:: " + e.ToString() + "  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor + " LOG: " + e.ToString());
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "ERROR EN LA ENTRADA DE DATOS, VERIFICAR LOGS.",
                        };
                        //throw e;
                    }

                    #endregion
                }

                #region MONTO
                decimal TotalAmountWithTaxMonto = 0M;
                decimal TotalAmountWithTaxMontoPerifericos = 0M;

                decimal TotalAmountWithTaxMontoCombu = 0M;
                try
                {
                    //OIL
                    foreach (string[] montoComb in ProcessAmountOfSaleCombu)
                    {
                        int countMontoComb = montoComb.Length;
                        for (int i = 0; i < countMontoComb; i++)
                        {
                            TotalAmountWithTaxMonto = TotalAmountWithTaxMonto + Convert.ToDecimal(montoComb[i]);
                            TotalAmountWithTaxMontoCombu = TotalAmountWithTaxMontoCombu + Convert.ToDecimal(montoComb[i]);
                        }
                    }
                    //PaymentSale
                    foreach (string[] montoPeri in ProcessAmountOfSalePeri)
                    {
                        int countMontoPeri = montoPeri.Length;
                        for (int i = 0; i < countMontoPeri; i++)
                        {
                            TotalAmountWithTaxMonto = TotalAmountWithTaxMonto + Convert.ToDecimal(montoPeri[i]);
                            TotalAmountWithTaxMontoPerifericos = TotalAmountWithTaxMontoPerifericos + Convert.ToDecimal(montoPeri[i]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log("CODEVOL_FIN ERROR 77", "@SHELLMX- ERRORES DE CONVERSION  O FORMATO DE MONTOAPAGAR VERIFICAR DATOS Log:: " + e.ToString() + "  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor + " LOG: " + e.ToString());
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "ERROR EN LA ENTRADA DE DATOS, VERIFICAR LOGS.",
                    };
                    throw e;
                }

                #endregion

                if (IsCombustibleEnabler)
                {
                    #region SE VERIFICA LA PARTE DE LA CANTIDAD DEL IMPORTE TOTAL EN LA PARTE DEL COMBUSTIBLE.
                    //if (TotalAmountWithTaxMontoCombu != Convert.ToDecimal(0))
                    //{
                    //    decimal totalOriginalONN = Math.Round(TotalAmountWithTaxMontoCombu, 2);
                    //    if (totalOriginalONN != totalOriginalON)
                    //    {
                    //        return new Salida_Info_Forma_Pago
                    //        {
                    //            Resultado = false,
                    //            Msj = "@SHELLMX- EL TOTAL DEL IMPORTE TOTAL DEL CARBURANTE ES DIFERENTE A LA QUE SE SOLICITO EN EL SURTUDOR. | CANTIDAD ENTRANTE : " + totalOriginalONN + " ! CANTIDAD ORIGINAL : " + totalOriginalON
                    //        };
                    //    }
                    //}
                    #endregion
                }

                #region PERIFERICOS.
                try
                {
                    for (int intPerife = 0; intPerife < countUniversalProduc; intPerife++)
                    {
                        int flagContPerife = 0;
                        foreach (List<string[]> productsGlobal in ProductsGlobal)
                        {
                            Products producto = new Products();
                            foreach (string[] products in Products)
                            {
                                for (int intValueProducts = 0; intValueProducts <= intPerife; intValueProducts++)
                                {
                                    switch (flagContPerife)
                                    {
                                        case 0:
                                            producto.Id_producto = products[intValueProducts];
                                            break;
                                        case 1:
                                            producto.Cantidad = Convert.ToDecimal(products[intValueProducts]);
                                            break;
                                        case 2:
                                            producto.Importe_Unitario = Convert.ToDecimal(products[intValueProducts]);
                                            break;
                                        case 3:
                                            producto.Importe_Total = Convert.ToDecimal(products[intValueProducts]);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                flagContPerife++;
                            }
                            InformListProducts.Add(producto);
                            producto = null;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log("CODEVOL_FIN ERROR 76", "@SHELLMX- ERRORES DE CONVERSION DE ID_PRODUCTO | CANTIDAD | IMPORTE_UNITARIO | IMPORTE_TOTAL VERIFICAR DATOS Log: " + e.ToString() + "  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "  IDSEGUIMIENTO: " + criptoInfoFor);
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "ERROR EN LA ENTRADA DE DATOS, VERIFICAR LOGS.",
                    };
                    throw e;
                }
                #endregion

                #endregion

                //SHELMX- Se realiza los calculos para el iva y el total de la venta y otros procesos para CreateD.
                //decimal totalAmountWithTax;
                //decimal totalTax;
                //decimal taxableAmount;

                #region SERIE & CLIENTE CONTADO & POSID DE TPV

                string serieId = null;
                string serieWebId = null;
                string customerId;
                string posId;
                string currencyId = null;

                GetSeriesRequest getSeriesRequest = new GetSeriesRequest { Identity = bsObj.Identity };
                GetSeriesResponse getSeriesResponse = await invokeHubbleWebAPIServices.GetSeries(getSeriesRequest);

                serieId = getSeriesResponse.SeriesList[0].Id.ToString();
                serieWebId = getSeriesResponse.SeriesList[0].Code.ToString();
                foreach (var series in getSeriesResponse.SeriesList)
                {
                    serieId = series.Id;
                    serieWebId = series.Code;
                }

                posId = getPOSInformationResponse.PosInformation.CompanyCode + getPOSInformationResponse.PosInformation.Code;

                GetPOSConfigurationRequest getPOSConfigurationRequest = new GetPOSConfigurationRequest { Identity = bsObj.Identity };
                GetPOSConfigurationResponse getPOSConfigurationResponse = await invokeHubbleWebAPIServices.GetPOSConfiguration(getPOSConfigurationRequest);
                customerId = getPOSConfigurationResponse.UnknownCustomerId;

                #endregion

                #region HORA UTC POR LA VENTA

                string emissionLocalDateTime;
                string emissionUTCDateTime;
                //SHLMX - Se coloca la hora de la venta. 
                DateTime horaCreacionVentalocal = DateTime.Now;
                DateTime horaCreacionVentaUniversalUTC = horaCreacionVentalocal.ToUniversalTime();

                Instant instanthoraCreacionVentalocal = Instant.FromUtc(horaCreacionVentalocal.Year, horaCreacionVentalocal.Month, horaCreacionVentalocal.Day, horaCreacionVentalocal.Hour, horaCreacionVentalocal.Minute, horaCreacionVentalocal.Second);
                Instant instanthoraCreacionVentaUniversalUTC = Instant.FromUtc(horaCreacionVentaUniversalUTC.Year, horaCreacionVentaUniversalUTC.Month, horaCreacionVentaUniversalUTC.Day, horaCreacionVentaUniversalUTC.Hour, horaCreacionVentaUniversalUTC.Minute, horaCreacionVentaUniversalUTC.Second);

                emissionLocalDateTime = instanthoraCreacionVentalocal.ToString();
                emissionUTCDateTime = instanthoraCreacionVentaUniversalUTC.ToString();

                #endregion

                #region LLENADO DE UNA LISTA DE PRODUCTOS PARA CREATEDOCUMENT

                #region SE BUSCA Y SE TRANFORMA EL PAYMENTS DE LA VENTA.

                GetPaymentMethodsRequest getPaymentMethodsRequest = new GetPaymentMethodsRequest { Identity = bsObj.Identity };
                GetPaymentMethodsResponse getPaymentMethodsResponse = await invokeHubbleWebAPIServices.GetPaymentMethods(getPaymentMethodsRequest);

                GetCurrenciesRequest getCurrenciesRequest = new GetCurrenciesRequest { Identity = bsObj.Identity };
                GetCurrenciesResponse getCurrenciesResponse = await invokeHubbleWebAPIServices.GetCurrencies(getCurrenciesRequest);

                //SHLMX- Se llena el paymentsList<> con las ventas.
                List<CreateDocumentPaymentDetailDAO> PaymentDetailListPreview = new List<CreateDocumentPaymentDetailDAO>();

                //CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                string paymentCash = null;
                string paymentCard = null;
                string paymentany = null;

                #region OIL STATION
                //List<CreateDocumentPaymentDetailDAO> DetailsCardSale = new List<CreateDocumentPaymentDetailDAO>();
                //bool isValidFormaPagoT = false;

                if (IsCombustibleEnabler)
                {
                    try
                    {
                        foreach (string[] processPaymentsCombu in ProcessPaymentsCombu)
                        {
                            foreach (string[] processAmountOfSaleCombu in ProcessAmountOfSaleCombu)
                            {
                                int processPaymentsCombuCount = processPaymentsCombu.Length;
                                int processAmountOfSaleCount = processAmountOfSaleCombu.Length;
                             // foreach (var paymentMethods in getPaymentMethodsResponse.PaymentMethodList)
                             // {
                                    //CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                             //     if (paymentMethods.Description.ToUpper() == "TARJETA")
                             //     {
                                        for (int i = 0; i < processPaymentsCombuCount; i++)
                                        {
                                            CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                            foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                            {
                                             // if (Convert.ToInt32(processPaymentsCombu[i]) != 3) //if (processPaymentsCombu[i].ToUpper() == "TARJETA")
                                             // {
                                                    if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                                    {
                                                        if (i < processAmountOfSaleCount)
                                                        {
                                                            createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSaleCombu[i]);
                                                            createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSaleCombu[i]);
                                                            createDocumentPaymentDetailDAO.PaymentMethodId = getPOSInformationResponse.PosInformation.CompanyCode + getPaymentTypeByIdPC(Convert.ToInt32(processPaymentsCombu[i])); //paymentMethods.Id;
                                                            createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                                            createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                                            createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                                            PaymentDetailListPreview.Add(createDocumentPaymentDetailDAO);
                                                            currencyId = CurrenciesBase.Id;
                                                            //paymentCard = paymentMethods.Id;
                                                        }
                                                    }
                                                    //createDocumentPaymentDetailDAO = null;
                                             // }
                                            }
                                        }
                                //  }
                                   /* if (paymentMethods.Description.ToUpper() == "EFECTIVO")
                                    {
                                        for (int i = 0; i < processPaymentsCombuCount; i++)
                                        {
                                            CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                            foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                            {
                                                if (Convert.ToInt32(processPaymentsCombu[i]) == 3) //if (processPaymentsCombu[i].ToUpper() == "EFECTIVO")
                                                {
                                                    if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                                    {
                                                        if (i < processAmountOfSaleCount)
                                                        {
                                                            createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSaleCombu[i]);
                                                            createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSaleCombu[i]);
                                                            createDocumentPaymentDetailDAO.PaymentMethodId = paymentMethods.Id;
                                                            createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                                            createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                                            createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                                            PaymentDetailListPreview.Add(createDocumentPaymentDetailDAO);
                                                            currencyId = CurrenciesBase.Id;
                                                            paymentCash = paymentMethods.Id;
                                                        }
                                                    }
                                                    //createDocumentPaymentDetailDAO = null;
                                                }
                                            }
                                        }
                                    }*/

                                    //if (paymentMethods.Description.ToUpper() == "AMERICAN EXPRESS")
                                    //{
                                    //    for (int i = 0; i < processPaymentsCombuCount; i++)
                                    //    {
                                    //        CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                    //        foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                    //        {
                                    //            if (Convert.ToInt32(processPaymentsCombu[i]) == 1)
                                    //            {
                                    //                if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                    //                {
                                    //                    if (i < processAmountOfSaleCount)
                                    //                    {
                                    //                        createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSaleCombu[i]);
                                    //                        createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSaleCombu[i]);
                                    //                        createDocumentPaymentDetailDAO.PaymentMethodId = paymentMethods.Id;
                                    //                        createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                    //                        createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                    //                        createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                    //                        PaymentDetailListPreview.Add(createDocumentPaymentDetailDAO);
                                    //                        currencyId = CurrenciesBase.Id;
                                    //                        paymentany = paymentMethods.Id;
                                    //                    }
                                    //                }
                                    //                //createDocumentPaymentDetailDAO = null;
                                    //            }
                                    //        }
                                    //    }
                                    //}//end
                              //}
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log("CODEVOL_FIN ERROR 75", "@SHELLMX- ERRORES DE CONVERSION EN MONTOPAGO O FOMAPAGO NO TIENE EL IDCATALOGO ADECUADO VERIFICAR!! Log:: " + e.ToString() + " ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor);
                        return new Salida_Info_Forma_Pago
                        {
                            Resultado = false,
                            Msj = "ERROR EN LA ENTRADA DE DATOS, VERIFICAR LOGS."
                        };
                        throw e;
                    }
                }

                #endregion

                #region PERIPHERICS STATION
                //List<CreateDocumentPaymentDetailDAO> DetailsCashSale = new List<CreateDocumentPaymentDetailDAO>();
                //bool isValidFormaPagoE = false;
                try
                {
                    foreach (string[] processPaymentsPeri in ProcessPaymentsPeri)
                    {
                        foreach (string[] processAmountOfSalePeri in ProcessAmountOfSalePeri)
                        {
                            int processPaymentsPeriCount = processPaymentsPeri.Length;
                            int processAmountOfSaleCount = processAmountOfSalePeri.Length;
                       //   foreach (var paymentMethods in getPaymentMethodsResponse.PaymentMethodList)
                       //   {
                                //CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                       //       if (paymentMethods.Description.ToUpper() == "TARJETA")
                                {
                                    for (int i = 0; i < processPaymentsPeriCount; i++)
                                    {
                                        CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                        foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                        {
                                          //if (Convert.ToInt32(processPaymentsPeri[i]) != 3) //if (processPaymentsPeri[i].ToUpper() == "TARJETA")
                                          //{
                                                if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                                {
                                                    if (i < processAmountOfSaleCount)
                                                    {
                                                        createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSalePeri[i]);
                                                        createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSalePeri[i]);
                                                        createDocumentPaymentDetailDAO.PaymentMethodId = getPOSInformationResponse.PosInformation.CompanyCode + getPaymentTypeByIdPC(Convert.ToInt32(processPaymentsPeri[i])); // paymentMethods.Id;
                                                        createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                                        createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                                        createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                                        PaymentDetailListPreview.Add(createDocumentPaymentDetailDAO);
                                                        currencyId = CurrenciesBase.Id;
                                                        //paymentCard = paymentMethods.Id;
                                                    }
                                                }
                                                //createDocumentPaymentDetailDAO = null;
                                          //}
                                        }
                                    }
                                }
                              /*if (paymentMethods.Description.ToUpper() == "EFECTIVO")
                                {
                                    for (int i = 0; i < processPaymentsPeriCount; i++)
                                    {
                                        CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                        foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                        {
                                            if (Convert.ToInt32(processPaymentsPeri[i]) == 3) //if (processPaymentsPeri[i].ToUpper() == "EFECTIVO")
                                            {
                                                if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                                {
                                                    if (i < processAmountOfSaleCount)
                                                    {
                                                        createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSalePeri[i]);
                                                        createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSalePeri[i]);
                                                        createDocumentPaymentDetailDAO.PaymentMethodId = paymentMethods.Id;
                                                        createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                                        createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                                        createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                                        PaymentDetailListPreview.Add(createDocumentPaymentDetailDAO);
                                                        currencyId = CurrenciesBase.Id;
                                                        paymentCash = paymentMethods.Id;
                                                    }
                                                }
                                                //createDocumentPaymentDetailDAO = null;
                                            }
                                        }
                                    }
                                }*/

                                //if (paymentMethods.Description.ToUpper() == "AMERICAN EXPRESS")
                                //{
                                //    for (int i = 0; i < processPaymentsPeriCount; i++)
                                //    {
                                //        CreateDocumentPaymentDetailDAO createDocumentPaymentDetailDAO = new CreateDocumentPaymentDetailDAO();
                                //        foreach (var CurrenciesBase in getCurrenciesResponse.CurrencyList)
                                //        {
                                //            if (Convert.ToInt32(processPaymentsPeri[i]) == 1)
                                //            {
                                //                if (CurrenciesBase.PriorityType == CurrencyPriorityType.Base)
                                //                {
                                //                    if (i < processAmountOfSaleCount)
                                //                    {
                                //                        createDocumentPaymentDetailDAO.PrimaryCurrencyGivenAmount = Convert.ToDecimal(processAmountOfSalePeri[i]);
                                //                        createDocumentPaymentDetailDAO.PrimaryCurrencyTakenAmount = Convert.ToDecimal(processAmountOfSalePeri[i]);
                                //                        createDocumentPaymentDetailDAO.PaymentMethodId = paymentMethods.Id;
                                //                        createDocumentPaymentDetailDAO.CurrencyId = CurrenciesBase.Id;
                                //                        createDocumentPaymentDetailDAO.ChangeFactorFromBase = Convert.ToDecimal(CurrenciesBase.ChangeFactorFromBase);
                                //                        createDocumentPaymentDetailDAO.UsageType = CreatePaymentUsageType.PendingPayment;
                                //                        PaymentDetailListPreview.Add(createDocumentPaymentDetailDAO);
                                //                        currencyId = CurrenciesBase.Id;
                                //                        paymentany = paymentMethods.Id; ;
                                //                    }
                                //                }
                                //                //createDocumentPaymentDetailDAO = null;
                                //            }
                                //        }
                                //    }
                                //}//end
                         // }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log("CODEVOL_FIN ERROR 74", "@SHELLMX- ERRORES DE CONVERSION EN MONTOPAGO O FOMAPAGO NO TIENE EL IDCATALOGO ADECUADO VERIFICAR EN PERIFERICOS  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO:" + criptoInfoFor + "\n Log: " + e.ToString());
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "ERROR EN LA ENTRADA DE DATOS, VERIFICAR LOGS."
                    };
                    throw e;
                }

                #endregion

                #region PREPARE OF UNIFIQUE PAYMENTS OF THE DOCUMENTS
                List<CreateDocumentPaymentDetailDAO> PaymentDetailList = new List<CreateDocumentPaymentDetailDAO>();
                PaymentDetailList = PaymentDetailListPreview; // SHLMX - SE ASIGNA LA LISTA DIRECTA DE LA LECTURA DE MEDIOS DE PAGO
                /*
                bool iscash = false;
                bool isCard = false;
                bool isAny = false;

                bool cashT = false;
                bool cardT = false;
                bool anyT = false;
                foreach (var paymentsU in PaymentDetailListPreview)
                {
                    decimal paymentIncrementyGivenAmount = 0M;
                    decimal paymentIncrementyTakenAmount = 0M;
                    foreach (var paymentsU1 in PaymentDetailListPreview)
                    {
                        if (paymentCash == paymentsU.PaymentMethodId && paymentsU1.PaymentMethodId == paymentCash)
                        {
                            paymentIncrementyGivenAmount = paymentIncrementyGivenAmount + paymentsU1.PrimaryCurrencyGivenAmount;
                            paymentIncrementyTakenAmount = paymentIncrementyTakenAmount + paymentsU1.PrimaryCurrencyTakenAmount;
                            isCard = false;
                            iscash = true;
                            isAny = false;
                        }
                        if (paymentCard == paymentsU.PaymentMethodId && paymentsU1.PaymentMethodId == paymentCard)
                        {
                            paymentIncrementyGivenAmount = paymentIncrementyGivenAmount + paymentsU1.PrimaryCurrencyGivenAmount;
                            paymentIncrementyTakenAmount = paymentIncrementyTakenAmount + paymentsU1.PrimaryCurrencyTakenAmount;
                            isCard = true;
                            iscash = false;
                            isAny = false;
                        }
                        if (paymentany == paymentsU.PaymentMethodId && paymentsU1.PaymentMethodId == paymentany)
                        {
                            paymentIncrementyGivenAmount = paymentIncrementyGivenAmount + paymentsU1.PrimaryCurrencyGivenAmount;
                            paymentIncrementyTakenAmount = paymentIncrementyTakenAmount + paymentsU1.PrimaryCurrencyTakenAmount;
                            isCard = false;
                            iscash = false;
                            isAny = true;
                        }
                    }
                    if (iscash && !cashT)
                    {
                        paymentsU.PrimaryCurrencyGivenAmount = paymentIncrementyGivenAmount;
                        paymentsU.PrimaryCurrencyTakenAmount = paymentIncrementyTakenAmount;
                        PaymentDetailList.Add(paymentsU);
                        cashT = true;
                    }
                    if (isCard && !cardT)
                    {
                        paymentsU.PrimaryCurrencyGivenAmount = paymentIncrementyGivenAmount;
                        paymentsU.PrimaryCurrencyTakenAmount = paymentIncrementyTakenAmount;
                        PaymentDetailList.Add(paymentsU);
                        cardT = true;
                    }
                    if (isAny && !anyT)
                    {
                        paymentsU.PrimaryCurrencyGivenAmount = paymentIncrementyGivenAmount;
                        paymentsU.PrimaryCurrencyTakenAmount = paymentIncrementyTakenAmount;
                        PaymentDetailList.Add(paymentsU);
                        anyT = true;
                    }
                }
                */
                #endregion

                #endregion

                #region SE LLENA LA LINEAS DE LA VENTA
                List<CreateDocumentLineDAO> LineList = new List<CreateDocumentLineDAO>();

                #region SE EMPIEZA A CONSTRUIR LA LISTA DE PRODUCTOS.
                Dictionary<decimal, decimal> TotalTaxListSale = new Dictionary<decimal, decimal>();
                //List<decimal[]> ListIvas = new List<decimal[]>();

                int lineNumber = 1;
                bool isValidIVAZERO = false;
                bool ZERO = true;
                decimal validateTotalAmountWithTax = 0M;
                try
                {
                    foreach (Products informListProducts in InformListProducts)
                    {
                        CreateDocumentLineDAO createDocumentLineDAO = new CreateDocumentLineDAO();
                        GetProductForSaleRequest getProductForSaleRequest = new GetProductForSaleRequest { ProductId = informListProducts.Id_producto.ToString(), Quantity = informListProducts.Cantidad, Identity = bsObj.Identity };
                        GetProductForSaleResponse getProductForSaleResponse = await invokeHubbleWebAPIServices.GetProductForSale(getProductForSaleRequest);

                        if (getProductForSaleResponse.Status < 0)
                        {
                            Log("CODEVOL_FIN WARNING 73", "@SHELLMX- LOS PRODUCTOS INTRODUCIDOS NO EXISTEN O SON INCORRECTOS VERIFICAR ID PRODUCTO INTRODUCIDO : " + informListProducts.Id_producto.ToString() + " ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + "  IDSEGUIMIENTO: " + criptoInfoFor);
                            return new Salida_Info_Forma_Pago
                            {
                                Resultado = false,
                                Msj = "PRODUCTOS INTRODUCIDOS NO EXISTEN O SON INCORRECTOS VERIFICAR."
                            };
                        }
                        decimal IvaProducto = 0M;
                        if (getProductForSaleResponse.TaxPercentage == Convert.ToDecimal(0))
                        {
                            //SHLMX- Se contruye el product, para el arreglo.
                            createDocumentLineDAO.LineNumber = lineNumber;
                            createDocumentLineDAO.ProductId = informListProducts.Id_producto;
                            createDocumentLineDAO.Quantity = informListProducts.Cantidad;
                            createDocumentLineDAO.UnitaryPriceWithTax = informListProducts.Importe_Unitario;
                            createDocumentLineDAO.ProductName = getProductForSaleResponse.ProductName;
                            createDocumentLineDAO.TotalAmountWithTax = informListProducts.Importe_Total;
                            createDocumentLineDAO.PriceWithoutTax = informListProducts.Importe_Total;
                            //IvaProductos[0] = getProductForSaleResponse.TaxPercentage;
                            //IvaProductos[1] = getProductForSaleResponse.TaxPercentage;
                            //ListIvas.Add(IvaProductos);
                            //IvaProductos = null;
                            ///summary 
                            /// PROCESO DE VALIDACION DEL QUE SE DEBE ENTREGAR EL TOTAL CORRESPONDIENTE EN LA VENTA PARA EVITAR DATOS ELEVADOS.
                            /// 
                            validateTotalAmountWithTax = validateTotalAmountWithTax + informListProducts.Importe_Total;
                            ///summary 
                            /// FIN DEL PROCESO DE LLENADO DEL TOTAL DE LA VENTA.
                            /// 
                            isValidIVAZERO = true;
                        }
                        else
                        {
                            //SHLMX- Se contruye el product, para el arreglo.
                            createDocumentLineDAO.LineNumber = lineNumber;
                            createDocumentLineDAO.ProductId = informListProducts.Id_producto;
                            createDocumentLineDAO.Quantity = informListProducts.Cantidad;
                            createDocumentLineDAO.UnitaryPriceWithTax = informListProducts.Importe_Unitario;
                            createDocumentLineDAO.TaxPercentage = getProductForSaleResponse.TaxPercentage;
                            decimal priceWithoutTaxW = informListProducts.Importe_Total / ((getProductForSaleResponse.TaxPercentage / 100) + 1);
                            decimal priceWithoutTax = Math.Round(priceWithoutTaxW, 6);
                            createDocumentLineDAO.PriceWithoutTax = priceWithoutTax;
                            createDocumentLineDAO.TaxAmount = informListProducts.Importe_Total - createDocumentLineDAO.PriceWithoutTax;

                            createDocumentLineDAO.ProductName = getProductForSaleResponse.ProductName;
                            createDocumentLineDAO.TotalAmountWithTax = informListProducts.Importe_Total;
                            IvaProducto = Convert.ToDecimal(getProductForSaleResponse.TaxPercentage);
                            //IvaProductos[1] = informListProducts.Importe_Total - createDocumentLineDAO.PriceWithoutTax; ListIvas.Add(IvaProductos);
                            //IvaProductos = null;

                            ///summary 
                            /// PROCESO DE VALIDACION DEL QUE SE DEBE ENTREGAR EL TOTAL CORRESPONDIENTE EN LA VENTA PARA EVITAR DATOS ELEVADOS.
                            /// 
                            validateTotalAmountWithTax = validateTotalAmountWithTax + getProductForSaleResponse.FinalAmount;
                            ///summary 
                            /// FIN DEL PROCESO DE LLENADO DEL TOTAL DE LA VENTA.
                            /// 

                            decimal ivaaplicado = 0M;
                            decimal priceTaxW = 0M;
                            decimal priceWTax = 0M;
                            decimal taxAmount = 0M;
                            foreach (Products informListPro in InformListProducts)
                            {
                                GetProductForSaleRequest getProduct = new GetProductForSaleRequest { ProductId = informListPro.Id_producto.ToString(), Quantity = informListPro.Cantidad, Identity = bsObj.Identity };
                                GetProductForSaleResponse getProductFor = await invokeHubbleWebAPIServices.GetProductForSale(getProductForSaleRequest);

                                if (IvaProducto == getProductFor.TaxPercentage)
                                {
                                    priceTaxW = informListPro.Importe_Total / ((getProductForSaleResponse.TaxPercentage / 100) + 1);
                                    priceWTax = Math.Round(priceTaxW, 6);//Math.Round(priceWithoutTaxW, 6);
                                    taxAmount = informListPro.Importe_Total - priceWTax;//createDocumentLineDAO.PriceWithoutTax;
                                    ivaaplicado += taxAmount;
                                }
                            }
                            if (ZERO)
                            {
                                TotalTaxListSale.Add(IvaProducto, ivaaplicado);
                                ZERO = false;
                            }
                            IvaProducto = 0M;
                        }

                        LineList.Add(createDocumentLineDAO);
                        lineNumber++;
                        createDocumentLineDAO = null;
                    }
                    if (isValidIVAZERO)
                    {
                        TotalTaxListSale.Add(Convert.ToDecimal(0), Convert.ToDecimal(0));
                    }
                }
                catch (Exception e)
                {
                    Log("CODEVOL_FIN ERROR 72", "@SHELLMX- LOS DATOS INTRODUCIDOS NO TIENEN EL FORMATO ESPECIFICO VERIFICAR E INTERTAR NUEVAMENTE ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor + "\n LOG: " + e.ToString());
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "ERROR EN LA ENTRADA DE DATOS, VERIFICAR LOGS."
                    };
                    throw e;
                }

                #endregion

                #region VALIDACION DEL TOTAL DE LA ENTRADA CON LA QUE SE CALCULO EN LOS ARTICULOS ORIGINALES.
                //if(TotalAmountWithTaxMontoPerifericos != 0)
                //{
                //    if(TotalAmountWithTaxMontoPerifericos < (validateTotalAmountWithTax - Importe_TotalCOMBUSTIBLE))
                //    {
                //        return new Salida_Info_Forma_Pago
                //        {
                //            Resultado = false,
                //            Msj = "@SHELLMX- EL TOTAL DEL CAMPO MONTOPAGODOPARCIAL NO CORRESPONDE CON EL TOTAL EL IMPORTE TOTAL DE LOS (PERIFERICOS) VERIFICAR! -- TOTAL_REQUEST: " + TotalAmountWithTaxMontoPerifericos + " | TOTAL_CALCULADO: " + (validateTotalAmountWithTax - Importe_TotalCOMBUSTIBLE) + " --"
                //        };
                //    }
                //    if (TotalAmountWithTaxMontoPerifericos > (validateTotalAmountWithTax - Importe_TotalCOMBUSTIBLE))
                //    {
                //        return new Salida_Info_Forma_Pago
                //        {
                //            Resultado = false,
                //            Msj = "@SHELLMX- EL TOTAL DEL CAMPO MONTOPAGODOPARCIAL NO CORRESPONDE CON EL TOTAL EL IMPORTE TOTAL DE LOS (PERIFERICOS) VERIFICAR! -- TOTAL_REQUEST: " + TotalAmountWithTaxMontoPerifericos + " | TOTAL_CALCULADO: " + (validateTotalAmountWithTax - Importe_TotalCOMBUSTIBLE) + " --"
                //        };
                //    }
                //}

                #endregion

                #endregion

                #endregion

                #region VALIDACION DE LAS IVAS
                decimal ivaTotal = 0M;
                foreach (KeyValuePair<decimal, decimal> result in TotalTaxListSale)
                {
                    ivaTotal = ivaTotal + result.Value;
                }
                #endregion

                #region LLAMAR EL API DE EVERILION PARA LA VENTA
                CreateDocumentDAO createDocumentDAO = new CreateDocumentDAO
                {
                    ProvisionalId = 1,
                    SerieId = serieId,
                    EmissionLocalDateTime = emissionLocalDateTime,
                    EmissionUTCDateTime = emissionUTCDateTime,
                    TaxableAmount = ivaTotal,
                    TotalTaxList = TotalTaxListSale,
                    TotalAmountWithTax = TotalAmountWithTaxMonto,
                    PaymentDetailList = PaymentDetailList,
                    LineList = LineList,
                    OperatorId = idOperator,
                    CustomerId = customerId,
                    ExtraData = null,
                    CurrencyId = currencyId,
                    PosId = posId,
                };
                List<CreateDocumentDAO> createDocumentDAOs = new List<CreateDocumentDAO>();
                createDocumentDAOs.Add(createDocumentDAO);

                //SHELL Se coloca el mapping para arrastrar el numero de ticket.
                //GetProvisionalIdToDocumentNumberMappingRequest getProvisionalIdToDocumentNumberMappingRequest = new GetProvisionalIdToDocumentNumberMappingRequest { CreateDAOList = createDocumentDAOs, Identity = bsObj.Identity };
                //GetProvisionalIdToDocumentNumberMappingResponse getProvisionalIdToDocumentNumberMappingResponse = await invokeHubbleWebAPIServices.GetProvisionalIdToDocumentNumberMapping(getProvisionalIdToDocumentNumberMappingRequest);
                /*if(getProvisionalIdToDocumentNumberMappingResponse.Status < 0)
                {
                    Log("CODEVOL_FIN ERROR 25", "@SHELLMX- FALLO EN PROCESO DE ASIGNACION DE NTICKET EN EL MAPPINGTICKET REVISAR. IDSEGUIMIENTO: " + criptoInfoFor + "\n LOG: \n " +
                        "getProvisionalIdToDocumentNumberMappingResponse : \n {" + "\n" + "    status: " + getProvisionalIdToDocumentNumberMappingResponse.Status.ToString() + ", \n" + "    message :" + getProvisionalIdToDocumentNumberMappingResponse.Message + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "ERROR EN ALMACENAR LA VENTA, VERIFICAR LOGS."
                    };
                }*/
                /*string item1 = null;
                string item2 = null;
                foreach(KeyValuePair<int, Tuple<string, string>> resultProvisionalIdToDocumentNumberMapping in getProvisionalIdToDocumentNumberMappingResponse.ProvisionalToDefinitiveDocumentIdDictionary)
                {
                    if(resultProvisionalIdToDocumentNumberMapping.Key == 1)
                    {
                        item1 = resultProvisionalIdToDocumentNumberMapping.Value.Item1;
                        item2 = resultProvisionalIdToDocumentNumberMapping.Value.Item2;
                    }
                }*/
                /*Log("CODEVOL_TR INFO", "SE HA INVOCADO EL WEBAPI DE PROVICIONALID DEL MAPPING. IDSEGUIMIENTO: " + criptoInfoFor + "\n" + "GetProvisionalIdToDocumentNumberMapping: \n" + "{" + "\n" +
                    "    item1: " + item1 + "," + "\n" +
                    "    item2: " + item2 + "," + "\n" + "}");
                foreach(CreateDocumentDAO createDocument in createDocumentDAOs)
                {
                    createDocument.ReferencedDocumentIdList.Add(item1);
                }*/

                CreateDocumentsRequest createDocumentsRequest = new CreateDocumentsRequest { CreateDAOList = createDocumentDAOs, Identity = bsObj.Identity };
                CreateDocumentsResponse createDocumentsResponse = await invokeHubbleWebAPIServices.CreateDocuments(createDocumentsRequest);
                if (createDocumentsResponse.Status < 0)
                {
                    Log("CODEVOL_FIN ERROR 71", "@SHELLMX- FALLO EN PROCESO INTERNO HUBBLE NO SE ALMACENO EN BDEVERILION REINTENTAR Y VERIFICAR LOS DATOS CORRECTOS DE ENTRADA  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO:" + criptoInfoFor + "\n " +
                        "createDocumentsResponse: \n {" + "\n" + "    status: " + createDocumentsResponse.Status.ToString() + ", \n" + "    message :" + createDocumentsResponse.Message + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "ERROR EN ALMACENAR LA VENTA, REVISAR VENTA EN TPV."
                    };
                }

                string possibleDocumentId = null;
                foreach (KeyValuePair<int, string> resultCreateDocuments in createDocumentsResponse.ProvisionalToDefinitiveDocumentIdDictionary)
                {
                    if (createDocumentsResponse.ProvisionalToDefinitiveDocumentIdDictionary.Count == 1)
                    {
                        possibleDocumentId = resultCreateDocuments.Key == 1 ? resultCreateDocuments.Value : null;
                    }
                }

                Log("CODEVOL_TR INFO", "RESPUESTA DE CREATEDOCUMENT() SUCCESFULL  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor + "\n " + "CreateDocumentsResponse: \n {" + "\n" +
                     "    status: " + createDocumentsResponse.Status.ToString() + "," + "\n" +
                     "    message: " + createDocumentsResponse.Message.ToString() + "," + "\n" +
                     "    possibleDocumentId: " + possibleDocumentId.ToString() + "\n" + "}");

                #region VALIDACION SOBRE EL WEBID Y OTROS COMPONENTES PARA LA FACTURA

                DateTimeOffset fechaTicketSale = DateTimeOffset.Parse(emissionLocalDateTime);
                string fechaTicketFact = Convert.ToString(fechaTicketSale.DateTime);
                string horaFormatFact = fechaTicketFact.Replace(" ", "");

                string hourWebID =   horaFormatFact.Substring(10, 2); 
                string companyEESS = getPOSInformationResponse.PosInformation.CompanyCode;
                string minutWebID = horaFormatFact.Substring(13, 2);  
                string serieTicket = serieWebId;
                string secontWebID = horaFormatFact.Substring(16, 2); 

                string webIDFact = string.Concat(hourWebID + companyEESS + minutWebID + serieTicket + secontWebID);

                #endregion

                // Se Revisa que se realizo correctamente la subida de la venta de producto menos carburante.
                if (IsCombustibleEnabler == false)
                {
                    Log("CODEVOL_FIN INFO", "SE TERMINO LA VENTA DE PERIFERICOS DONDE NO SE LIBERA NINGUNA BOMBA  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor + "\n" + "Salida_Info_Forma_Pago: \n {" + "\n" +
                        "    msj: " + "VENTA SATISFACTORIA DE PERIFERICOS OK," + "\n" +
                        "    Resultado: " + "true," + "\n" +
                        "    EESS: " + getPOSInformationResponse.PosInformation.ShopCode.ToString() + "," + "\n" +
                        "    Nticket: " + possibleDocumentId.ToString() + "," + "\n" +
                        "    WebId: " + webIDFact + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Msj = "SHELLHUBBLE- VENTA SATISFACTORIA DE PERIFERICOS OK.",
                        Resultado = true,
                        EESS = getPOSInformationResponse.PosInformation.ShopCode,
                        Nticket = possibleDocumentId,
                        WebId = webIDFact
                    };
                }

                #endregion

                #region COMUNICACION CON EL DOMS

                var guidDOMS = Guid.NewGuid().ToString();
                FinalizeSupplyTransactionRequest finalizeSupplyTransactionRequest = new FinalizeSupplyTransactionRequest
                {
                    ContactId = null,
                    CustomerId = customerId,
                    FuellingPointId = request.Pos_Carga,
                    LineNumberInDocument = 1,
                    OdometerMeasurement = null,
                    OperatorId = idOperator,
                    PossibleDocumentId = possibleDocumentId,
                    ProvisionalId = guidDOMS,
                    SupplyTransactionId = Convert.ToInt32(request.Id_Transaccion),
                    VehicleLicensePlate = null
                };

                Log("CODEVOL_TR INFO", "SE CONTRUYE EL OBJETO PARA FINALIZAR UNA BOMBA O SURTIDOR  ---> ID_TRANSACCION_BOMBA: " + request.Id_Transaccion + " IDSEGUIMIENTO: " + criptoInfoFor + "\n" + "FinalizeSupplyTransactionRequest: \n {" + "\n" +
                    "    ContactId: " + "NULL," + "\n" +
                    "    CustomerId: " + finalizeSupplyTransactionRequest.CustomerId.ToString() + "," + "\n" +
                    "    FuellingPointId: " + finalizeSupplyTransactionRequest.FuellingPointId.ToString() + "," + "\n" +
                    "    LineNumberInDocument: " + finalizeSupplyTransactionRequest.LineNumberInDocument.ToString() + "," + "\n" +
                    "    OdometerMeasurement: " + "NULL," + "\n" +
                    "    OperatorId : " + finalizeSupplyTransactionRequest.OperatorId.ToString() + "," + "\n" +
                    "    PossibleDocumentId: " + finalizeSupplyTransactionRequest.PossibleDocumentId.ToString() + "," + "\n" +
                    "    ProvisionalId: " + finalizeSupplyTransactionRequest.ProvisionalId.ToString() + "," + "\n" +
                    "    SupplyTransactionId: " + finalizeSupplyTransactionRequest.SupplyTransactionId.ToString() + "," + "\n" +
                    "    VehicleLicensePlate: " + "NULL," + "\n" +
                    "}");

                FinalizeSupplyTransactionResponse finalizeSupplyTransactionResponse = conectionSignalRDomsInform.FinalizeSupplyTransactionWS(finalizeSupplyTransactionRequest);
                string supplyTransactionIdDOMS = null;

                if (finalizeSupplyTransactionResponse.Status == -1)
                {
                    Log("CODEVOL_FIN ERROR 70", "@SHELLMX- FALLO LA CONEXION DE CERRAR LA BOMBA EN EL DOMS ERROR DE VALIDACION CHECAR LIBERACION DE BOMBA Y SU REGISTRO DEL TICKET CON EL ID_TRANSACTION.  ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + "\n LOG:" + "\n" +
                                       "FinalizeSupplyTransactionResponse: \n {" + "\n" +
                                       "    status: " + finalizeSupplyTransactionResponse.Status.ToString() + ", \n" + "    mesagge: " + finalizeSupplyTransactionResponse.Message.ToString() + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "<VENTA GENERADA EXITOSAMENTE> ERROR EN LIBERAR LA BOMBA DE LA VENTA, LIBERAR LA BOMBA DESDE EL TPV -1"
                    };
                }
                if (finalizeSupplyTransactionResponse.Status == -2)
                {
                    Log("CODEVOL_FIN ERROR 70", "@SHELLMX- FALLO LA CONEXION DE CERRAR LA BOMBA EN EL DOMS <<PSS NO CONECTADO AL HUBLEPOS CHECAR CONEXION Y GUARDADO DEL TICKET CON EL ID_TRANSACTION>>  ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + "\n LOG:" + "\n" +
                                       "FinalizeSupplyTransactionResponse: \n {" + "\n" +
                                       "    status: " + finalizeSupplyTransactionResponse.Status.ToString() + ", \n" + "    mesagge: " + finalizeSupplyTransactionResponse.Message.ToString() + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "<VENTA GENERADA EXITOSAMENTE> ERROR EN LIBERAR LA BOMBA DE LA VENTA, LIBERAR LA BOMBA DESDE EL TPV -2"
                    };
                }
                if (finalizeSupplyTransactionResponse.Status == -3)
                {
                    Log("CODEVOL_FIN ERROR 70", "@SHELLMX- FALLO LA CONEXION DE CERRAR LA BOMBA EN EL DOMS <<EL SURTIDOR NO EXISTIO VERIFICAR SI SE ALMACENO EN CETEL CON EL ID_TRANSACTION.>>  ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + "\n LOG:" + "\n" +
                                       "FinalizeSupplyTransactionResponse: \n {" + "\n" +
                                       "    status: " + finalizeSupplyTransactionResponse.Status.ToString() + ", \n" + "    mesagge: " + finalizeSupplyTransactionResponse.Message.ToString() + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "<VENTA GENERADA EXITOSAMENTE> ERROR EN LIBERAR LA BOMBA DE LA VENTA, LIBERAR LA BOMBA DESDE EL TPV -3"
                    };
                }
                if (finalizeSupplyTransactionResponse.Status == -4)
                {
                    Log("CODEVOL_FIN ERROR 70", "@SHELLMX- FALLO LA CONEXION DE CERRAR LA BOMBA EN EL DOMS <<TRANSACCION SOLICITADA NO EXISTE VERIFICAR SI SE ALMACENO EN CETEL CON EL ID_TRANSACTION.>>  ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + "\n LOG:" + "\n" +
                                       "FinalizeSupplyTransactionResponse: \n {" + "\n" +
                                       "    status: " + finalizeSupplyTransactionResponse.Status.ToString() + ", \n" + "    mesagge: " + finalizeSupplyTransactionResponse.Message.ToString() + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "<VENTA GENERADA EXITOSAMENTE> ERROR EN LIBERAR LA BOMBA DE LA VENTA, LIBERAR LA BOMBA DESDE EL TPV -4"
                    };
                }
                if (finalizeSupplyTransactionResponse.Status == -5)
                {
                    Log("CODEVOL_FIN ERROR 70", "@SHELLMX- FALLO LA CONEXION DE CERRAR LA BOMBA EN EL DOMS <<TRANSACCION EN UN ESTADO INVALIDO PARA REALIZAR ESTA ACCION VERIFICAR SI SE ALMACENO EN CETEL CON EL ID_TRANSACTION.>>  ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + "\n LOG:" + "\n" +
                                       "FinalizeSupplyTransactionResponse: \n {" + "\n" +
                                       "    status: " + finalizeSupplyTransactionResponse.Status.ToString() + ", \n" + "    mesagge: " + finalizeSupplyTransactionResponse.Message.ToString() + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "<VENTA GENERADA EXITOSAMENTE> ERROR EN LIBERAR LA BOMBA DE LA VENTA, LIBERAR LA BOMBA DESDE EL TPV -5"
                    };
                }
                if (finalizeSupplyTransactionResponse.Status == -6)
                {
                    Log("CODEVOL_FIN ERROR 70", "@SHELLMX- FALLO LA CONEXION DE CERRAR LA BOMBA EN EL DOMS <<LA OPERACION HA SIDO RECHAZADA POR EL CONTROLADOR, VERIFICAR SI SE ALMACENO EN CETEL CON EL ID_TRANSACTION.>>  ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + "\n LOG:" + "\n" +
                                       "FinalizeSupplyTransactionResponse: \n {" + "\n" +
                                       "    status: " + finalizeSupplyTransactionResponse.Status.ToString() + ", \n" + "    mesagge: " + finalizeSupplyTransactionResponse.Message.ToString() + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "<VENTA GENERADA EXITOSAMENTE> ERROR EN LIBERAR LA BOMBA DE LA VENTA, LIBERAR LA BOMBA DESDE EL TPV -6"
                    };
                }
                if (finalizeSupplyTransactionResponse.Status == -7)
                {
                    Log("CODEVOL_FIN ERROR 70", "@SHELLMX- FALLO LA CONEXION DE CERRAR LA BOMBA EN EL DOMS <<ERROR GENERAL CHECAR EL ESTADO Y GUARDADO DEL TICKET CON EL ID_TRANSACTION.>>  ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + "\n LOG:" + "\n" +
                                                          "FinalizeSupplyTransactionResponse: \n {" + "\n" +
                                                          "    status: " + finalizeSupplyTransactionResponse.Status.ToString() + ", \n" + "    mesagge: " + finalizeSupplyTransactionResponse.Message.ToString() + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "<VENTA GENERADA EXITOSAMENTE> ERROR EN LIBERAR LA BOMBA DE LA VENTA, LIBERAR LA BOMBA DESDE EL TPV -7"
                    };
                }

                foreach (KeyValuePair<string, string> resultsupplyTransactionIdDOMS in finalizeSupplyTransactionResponse.ProvisionalSupplyIdToDefinitiveSupplyIdMapping)
                {
                    supplyTransactionIdDOMS = resultsupplyTransactionIdDOMS.Value;
                }
                
                Log("CODEVOL_TR INFO", "SE VALIDO EL RESPONSE FinalizeSupplyTransactionResponse DE MANERA  ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + " \n" + "FinalizeSupplyTransactionResponse: \n {" + "\n" +
                    "    status: " + finalizeSupplyTransactionResponse.Status.ToString() + "," + "\n" +
                    "    message: " + finalizeSupplyTransactionResponse.Message.ToString() + "," + "\n" +
                    "    ProvisionalSupplyIdToDefinitiveSupplyIdMapping: " + supplyTransactionIdDOMS + "\n" + "}");
                #endregion

                #region FINALIZADO DEL DOMS Y LIBERACION DE LA BOMBA Y LA VENTA

                List<string> SupplyTransactionIdListWS = new List<string>()
                {
                supplyTransactionIdDOMS
                };
                SetDefinitiveDocumentIdForSupplyTransactionsRequest setDefinitiveDocumentIdForSupplyTransactionsRequest = new SetDefinitiveDocumentIdForSupplyTransactionsRequest
                {
                    OperatorId = idOperator,
                    DefinitiveDocumentId = possibleDocumentId,
                    SupplyTransactionIdList = SupplyTransactionIdListWS
                };

                Log("CODEVOL_TR INFO", "SE CONSTRUYE EL RESQUEST DE SetDefinitiveDocumentIdForSupplyTransactionsRequest DE LA MANERA   ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + "\n" +
                    "SetDefinitiveDocumentIdForSupplyTransactionsRequest: \n {" + "\n" +
                    "    OperadorId: " + setDefinitiveDocumentIdForSupplyTransactionsRequest.OperatorId.ToString() + "," + "\n" +
                    "    DefinitiveDocumentId: " + setDefinitiveDocumentIdForSupplyTransactionsRequest.DefinitiveDocumentId.ToString() + "," + "\n" +
                    "    SupplyTransactionIdList: " + supplyTransactionIdDOMS + "\n" + "}");

                SetDefinitiveDocumentIdForSupplyTransactionsResponse setDefinitiveDocumentIdForSupplyTransactionsResponse = conectionSignalRDomsInform.SetDefinitiveDocumentIdForSupplyTransactionsWS(setDefinitiveDocumentIdForSupplyTransactionsRequest);
                if (setDefinitiveDocumentIdForSupplyTransactionsResponse.Status == -1)
                {
                    Log("CODEVOL_FIN ERROR 69", "@SHELLMX- ERROR REVISAR FALLA DE CIERRE DEL DOMS Y VENTA VERIFICAR BOS, SE ALMANCENO LA VENTA PERO NO SE LIBERO  <<ERROR DE VALIDACION VERIFICAR SI SE ALMACENO EN PLATAFORMA>>   ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + "\n" + "SetDefinitiveDocumentIdForSupplyTransactionsRequest: \n {" +
                        "\n" + "    OperatorId: " + setDefinitiveDocumentIdForSupplyTransactionsRequest.OperatorId.ToString() + ", \n" + "    DefinitiveDocumentId : " + setDefinitiveDocumentIdForSupplyTransactionsRequest.DefinitiveDocumentId.ToString() +
                        "    SupplyTransactionIdList : " + supplyTransactionIdDOMS + "\n" + "}" + "\n" + " EL RESPONSE DE SetDefinitiveDocumentIdForSupplyTransactionsResponse: {" + "\n" +
                        "    status: " + setDefinitiveDocumentIdForSupplyTransactionsResponse.Status.ToString() + ", \n" +
                        "    message: " + setDefinitiveDocumentIdForSupplyTransactionsResponse.Message.ToString() + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "<VENTA GENERADA EXITOSAMENTE> ERROR EN LIBERAR LA BOMBA DE LA VENTA, LIBERAR LA BOMBA DESDE EL TPV -1F"
                    };
                }
                if (setDefinitiveDocumentIdForSupplyTransactionsResponse.Status == -2)
                {
                    Log("CODEVOL_FIN ERROR 69", "@SHELLMX- ERROR REVISAR FALLA DE CIERRE DEL DOMS Y VENTA VERIFICAR BOS, SE ALMANCENO LA VENTA PERO NO SE LIBERO  <<AL MENOS UNO DE LOS INDETIFICADORES DE SUMINISTRO NO CORRESPONDE A NINGUNO EXISTENTE VERIFICAR SI SE ALMACENO EN PLATAFORMA>>   ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + "\n" + "SetDefinitiveDocumentIdForSupplyTransactionsRequest: \n {" +
                        "\n" + "    OperatorId: " + setDefinitiveDocumentIdForSupplyTransactionsRequest.OperatorId.ToString() + ", \n" + "    DefinitiveDocumentId : " + setDefinitiveDocumentIdForSupplyTransactionsRequest.DefinitiveDocumentId.ToString() +
                        "    SupplyTransactionIdList : " + supplyTransactionIdDOMS + "\n" + "}" + "\n" + " EL RESPONSE DE SetDefinitiveDocumentIdForSupplyTransactionsResponse: {" + "\n" +
                        "    status: " + setDefinitiveDocumentIdForSupplyTransactionsResponse.Status.ToString() + ", \n" +
                        "    message: " + setDefinitiveDocumentIdForSupplyTransactionsResponse.Message.ToString() + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "<VENTA GENERADA EXITOSAMENTE> ERROR EN LIBERAR LA BOMBA DE LA VENTA, LIBERAR LA BOMBA DESDE EL TPV -2F"
                    };
                }
                if (setDefinitiveDocumentIdForSupplyTransactionsResponse.Status == -3)
                {
                    Log("CODEVOL_FIN ERROR 69", "@SHELLMX- ERROR REVISAR FALLA DE CIERRE DEL DOMS Y VENTA VERIFICAR BOS, SE ALMANCENO LA VENTA PERO NO SE LIBERO  <<SE GENERO UN ERRROR GENERICO. VERIFICAR SI SE ALMACENO EN PLATAFORMA>>   ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + "\n" + "SetDefinitiveDocumentIdForSupplyTransactionsRequest: \n {" +
                        "\n" + "    OperatorId: " + setDefinitiveDocumentIdForSupplyTransactionsRequest.OperatorId.ToString() + ", \n" + "    DefinitiveDocumentId : " + setDefinitiveDocumentIdForSupplyTransactionsRequest.DefinitiveDocumentId.ToString() +
                        "    SupplyTransactionIdList : " + supplyTransactionIdDOMS + "\n" + "}" + "\n" + " EL RESPONSE DE SetDefinitiveDocumentIdForSupplyTransactionsResponse: {" + "\n" +
                        "    status: " + setDefinitiveDocumentIdForSupplyTransactionsResponse.Status.ToString() + ", \n" +
                        "    message: " + setDefinitiveDocumentIdForSupplyTransactionsResponse.Message.ToString() + "\n" + "}");
                    return new Salida_Info_Forma_Pago
                    {
                        Resultado = false,
                        Msj = "<VENTA GENERADA EXITOSAMENTE> ERROR EN LIBERAR LA BOMBA DE LA VENTA, LIBERAR LA BOMBA DESDE EL TPV -3F"
                    };
                }
                #endregion

                Log("CODEVOL_FIN INFO", "FINALIZO LA VENTA CON EL RESPONSE  ---> ID_TRANSACTION: " + request.Id_Transaccion.ToString() + " IDSEGUIMIENTO: " + criptoInfoFor + "\n SetDefinitiveDocumentIdForSupplyTransactionsResponse : \n" + "{" + "\n" +
                    "    status: " + setDefinitiveDocumentIdForSupplyTransactionsResponse.Status.ToString() + "," + "\n" +
                    "    message" + setDefinitiveDocumentIdForSupplyTransactionsResponse.Message.ToString() + "\n" + "}" + "\n" +
                    "CON EL JSON DE SALIDA QUE SE ENTREGA A PC." + "\n" + "{" +
                    "    Msj: " + "SHELLHUBBLE- VENTA SATISFACTORIA," + "\n" +
                    "    Resultado: " + "true," + "\n" +
                    "    EESS: " + getPOSInformationResponse.PosInformation.ShopCode.ToString() + "," + "\n" +
                    "    Nticket: " + possibleDocumentId + "," + "\n" +
                    "    WebId: " + webIDFact + "\n" + "}");

                salida_Info_Forma_Pago.Msj = "SHELLHUBBLE- VENTA SATISFACTORIA";
                salida_Info_Forma_Pago.Resultado = true;
                salida_Info_Forma_Pago.EESS = getPOSInformationResponse.PosInformation.ShopCode;
                salida_Info_Forma_Pago.Nticket = possibleDocumentId;
                salida_Info_Forma_Pago.WebId = webIDFact;
                
            }
            catch (Exception e)
            {
                salida_Info_Forma_Pago.Msj = "OCURRIO UN ERROR INTERNO DEL SERVICIO DE VENTA VERIFICAR LOGS.";
                salida_Info_Forma_Pago.Resultado = false;
                Log("CODEVOL_FIN ERROR 26", "OCURRIO UN ERROR AL ENTRAR AL METODO INFO_FORMA_PAGO 'INTENTAR NUEVAMENTE DE REALIZAR LA VENTA'. IDSEGUIMIENTO: " + criptoInfoFor + "LOG:: " + e.ToString());
            }
            /*finally  // <--  termine bien o mal liberamos el semaforo
            {
                try
                {
                    SemaphoreSlimInfo_Forma_Pago?.Release();
                }
                catch (Exception)
                {
                }
            }*/
            return salida_Info_Forma_Pago;
        }

        public async Task<Salida_DesbloquearCarga> DesbloquearCarga(Entrada_DesbloquearCarga request)
        {
            Salida_DesbloquearCarga salida_DesbloquearCarga = new Salida_DesbloquearCarga();
            LogsTPVHP exec = new LogsTPVHP();
            var criptoDesC = DateTime.Now.ToString("yyyyMM") + "_DESBLCA_" + DateTime.Now.ToString("h-mm-ss-ffff");
            try
            {
                // si está ocupado se espera.
                //await SemaphoreSlimDesbloquearCarga.WaitAsync();
                #region PROCESOS DE VALIDACION EN LAS ENTRADA.
                Log("CODEVOL_INI INFO", "@SHELLMX- SE INICIA EL METODO DESBLOQUEARCARGA PARA EL DESBLOQUEO DE BOMBA. IDSEGUIMIENTO: " + criptoDesC);
                Log("CODEVOL_TR INFO", "@SHELLMX- EL REQUEST QUE ENTRA PARA EL DESBLOQUEO:" + "\n" +
                    "{" + "\n" + "    id_Teller: " + request.Id_teller + "," + "\n" + 
                    "    id_Transaction: " + request.Id_Transaccion.ToString() + "," + "\n" + 
                    "    pos_Carga: " + request.Pos_Carga.ToString() + "," + "\n" + 
                    "    nHD: " + request.nHD.ToString() + "," + "\n" + 
                    "    pTID: " + request.PTID.ToString() + "," + "\n" + 
                    "    serial" + request.serial + "\n" + "}") ;
                try
                {
                    if (Convert.ToDecimal(request.Id_Transaccion) <= Convert.ToDecimal(0))
                    {
                        Log("CODEVOL_FIN WARNING 60", "@SHELLMX- ES ID TRANSACCION NO ES VALIDO O NO EXISTE. IDSEGUIMIENTO: " + criptoDesC);
                        return new Salida_DesbloquearCarga
                        {
                            Msj = "EL ID DE LA BOMBA NO ES VALIDO O NO EXISTE.",
                            Resultado = false
                        };
                    }
                    if (Convert.ToDecimal(request.Pos_Carga) <= Convert.ToDecimal(0))
                    {
                        Log("CODEVOL_FIN WARNING 59", "@SHELLMX- EL NUMERO DE LA BOMBA NO ES VALIDO O NO EXISTE. IDSEGUIMIENTO: " + criptoDesC);
                        return new Salida_DesbloquearCarga
                        {
                            Msj = "EL NUMERO DE LA BOMBA NO ES VALIDO O NO EXISTE.",
                            Resultado = false
                        };
                    }
                }
                catch (Exception e)
                {
                    Log("CODEVOL_FIN ERROR 58", "@SHELLMX- NO CORRESPONDE CON EL FORMATO EL ID_TRANSACION | POS_CARGA. IDSEGUIMIENTO:" + criptoDesC + " LOG: " + e.ToString());
                    return new Salida_DesbloquearCarga
                    {
                        Msj = "ERROR EN LOS DATOS DE ENTRADA VALIDAR LOGS.",
                        Resultado = false
                    };
                }

                if (request.idpos == null || request.PTID == null || request.serial == null || request.pss == null || request.nHD <= -1)
                {
                    Log("CODEVOL_FIN WARNING 57", "@SHELLMX- NO CORRESPONDE CON EL FORMATO EL idpos | PTID | serial | pss | nHD. IDSEGUIMIENTO: " + criptoDesC);
                    return new Salida_DesbloquearCarga
                    {
                        Msj = "ERROR EN LOS DATOS DE ENTRADA VALIDAR LOGS.",
                        Resultado = false
                    };
                }

                #endregion

                // SHELLMX- Se manda a consumir el Identity del POS a activar.
                var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
                TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
                InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();

                #region VALIDACION DEL OPERADOR ID | CODE
                List<SearchOperatorCriteria> SearchOperatorCriteriaOperator = new List<SearchOperatorCriteria>
                {
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Id , MatchingType = SearchOperatorCriteriaMatchingType.Exact },
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Code , MatchingType = SearchOperatorCriteriaMatchingType.Exact }
                };
                SearchOperatorRequest searchOperatorRequest = new SearchOperatorRequest
                {
                    Identity = bsObj.Identity,
                    CriteriaList = SearchOperatorCriteriaOperator,
                    CriteriaRelationshipType = SearchCriteriaRelationshipType.Or,
                    MustIncludeDischarged = false
                };

                SearchOperatorResponse searchOperatorResponse = await invokeHubbleWebAPIServices.SearchOperator(searchOperatorRequest);
                string idOperator = null;
                string codeOperator = null;
                string nameOperator = null;

                if (searchOperatorResponse.OperatorList.Count == 0)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    Log("CODEVOL_FIN WARNING 56", "@SHELLMX- ERROR OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA. ID_TELLER : " + request.Id_teller.ToString() + " IDSEGUIMIENTO: " + criptoDesC);
                    return new Salida_DesbloquearCarga
                    {
                        Msj = "OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA.",
                        Resultado = false
                    };
                }
                foreach (var searchOperator in searchOperatorResponse.OperatorList)
                {
                    if (searchOperatorResponse.OperatorList.Count == 1)
                    {
                        idOperator = searchOperator.Id;
                        codeOperator = searchOperator.Code;
                        nameOperator = searchOperator.Name;
                    }
                    if (searchOperatorResponse.OperatorList.Count == 2)
                    {
                        idOperator = searchOperator.Id;
                        codeOperator = searchOperator.Code;
                        nameOperator = searchOperator.Name;
                    }
                }

                #endregion

                ConectionSignalRDoms conectionSignalRDoms = new ConectionSignalRDoms();

                #region PROCESO DE VALIDACION DE DESBLOQUEO

                UnlockSupplyTransactionOfFuellingPointRequest unlockSupplyTransactionOfFuellingPointRequest = new UnlockSupplyTransactionOfFuellingPointRequest
                {
                    FuellingPointId = request.Pos_Carga,
                    OperatorId = idOperator,
                    SupplyTransactionId = Convert.ToUInt16(request.Id_Transaccion)
                };

                UnlockSupplyTransactionOfFuellingPointResponse unlockSupplyTransactionOfFuellingPointResponse = conectionSignalRDoms.UnlockSupplyTransactionOfFuellingPointWS(unlockSupplyTransactionOfFuellingPointRequest);
                if (unlockSupplyTransactionOfFuellingPointResponse.Status < 0)
                {
                    Log("CODEVOL_FIN ERROR 55", "@SHELLMX- NO SE PUDO DESBLOQUEAR LA BOMBA.  IDSEGUIMIENTO: "+ criptoDesC + "LOG:: RESPONSE: " + "\n" + "UnlockSupplyTransactionOfFuellingPointResponse: {" + "\n" +
                        "    status: " + unlockSupplyTransactionOfFuellingPointResponse.Status.ToString() + "," + "\n" +
                        "    message: " + unlockSupplyTransactionOfFuellingPointResponse.Message.ToString() + "\n" + "}");
                    return new Salida_DesbloquearCarga
                    {
                        Msj = "NO SE PUDO DESBLOQUEAR LA BOMBA.",
                        Resultado = false
                    };
                }
                Log("CODEVOL_TR INFO", "OPERADOR QUE SOLICTA EL DESBLOQUEO DE BOMBA : " + nameOperator + " IDSEGUIMIENTO: " + criptoDesC);
                Log("CODEVOL_FIN 1", "SE FINALIZO EN METODO DE DESBLOQUEAR_BOMBA SATISFACTORIAMENTE CON EL SIGUIENTE RESPONSE. IDSEGUIMIENTO: " + criptoDesC + "\n" + " Salida_DesbloquearCarga: {" + "\n" +
                    "    msj: " + "@ SHELLMX- SE HA INICIADO EL DESBLOQUEO DE LA BOMBA : " + request.Pos_Carga.ToString() + " CON STATUS : " + unlockSupplyTransactionOfFuellingPointResponse.Status.ToString() + "," + "\n" +
                    "    resultado: " + "true" + "\n" + "}");

                salida_DesbloquearCarga.Msj = "SE HA INICIADO EL DESBLOQUEO DE LA BOMBA : " + request.Pos_Carga.ToString() + " CON STATUS : " + unlockSupplyTransactionOfFuellingPointResponse.Status;
                salida_DesbloquearCarga.Resultado = true;

                #endregion
            }
            catch (Exception e)
            {
                salida_DesbloquearCarga.Msj = "ERROR INTERNO AL DESBLOQUEAR LA BOMBA VERIFICAR LOGS.";
                salida_DesbloquearCarga.Resultado = false;
                Log("CODEVOL_FIN ERROR 28", "@SHELLMX- HUBO UN PROBLEMA AL ENTRAR AL METODO DE DESBLOQUEARBOMBA IDSEGUIMIENTO: " + criptoDesC + " LOG:: " + e.ToString());
            }
            /*finally  // <--  termine bien o mal liberamos el semaforo
            {
                try
                {
                    SemaphoreSlimDesbloquearCarga?.Release();
                }
                catch (Exception)
                {
                }
            }*/
            return salida_DesbloquearCarga;
        }

        public async Task<Salida_getProductInfo> getProductInfo(Entrada_getProductInfo request)
        {
            Salida_getProductInfo salida = new Salida_getProductInfo();
            LogsTPVHP exec = new LogsTPVHP();
            var criptoGetProd = DateTime.Now.ToString("yyyyMM") + "_GETPRODU_" + DateTime.Now.ToString("h-mm-ss-ffff");
            try
            {
                // si está ocupado se espera.
                //await SemaphoreSlimGetProductInfo.WaitAsync();

                Log("CODEVOL_INI 0", "SE INICIA EL METODO GETPRODUCTINFO PARA OBTENER UN ARTICULO. IDSEGUIMIENTO: " + criptoGetProd);
                Log("CODEVOL_TR INFO", "@SHELLMX- EL REQUEST QUE ENTRA PARA OBTENER EL PRODUCTO:" + "\n" +
                    "{" + "\n" + "    id_Teller: " + request.Id_teller + "," + "\n" +
                    "    id_Product: " + request.IdProduct.ToString() + "," + "\n" +
                    "    pos_Carga: " + request.Pos_Carga.ToString() + "," + "\n" +
                    "    nHD: " + request.nHD.ToString() + "," + "\n" +
                    "    pTID: " + request.PTID.ToString() + "," + "\n" +
                    "    serial" + request.serial + "\n" + "}");
                try
                {
                    if (request.Pos_Carga < 0)
                    {
                        //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                        Log("CODEVOL_FIN WARNING 54", "@SHELLMX- DEBE DE INSERTAR UN SURTIDOR QUE ESTA LIGADO! IDSEGUIMIENTO: " + criptoGetProd);
                        return new Salida_getProductInfo
                        {
                            Resultado = false,
                            Msj = "DEBE DE INSERTAR UN SURTIDOR QUE ESTA LIGADO."
                        };
                    }
                }
                catch (Exception e)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    Log("CODEVOL_FIN ERROR 53", "@SHELLMX- DEBE DE INTRODUCIR EL FORMATO CORRECTO DE SURTIDOR NUMERO! IDSEGUIMIENTO: " + criptoGetProd + " LOG:: " + e.ToString());
                    return new Salida_getProductInfo
                    {
                        Resultado = false,
                        Msj = "LOS DATOS DE ENTRADA DEL PRODUCTO SON INCORRECTOS."
                    };
                    throw e;
                }

                // SHELLMX- Se consigue el Token del TPV para hacer las pruebas. 
                var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
                TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
                InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();

                //GetOperatorRequest getOperatorRequest = new GetOperatorRequest { Id = request.Id_teller, Identity = bsObj.Identity };
                //GetOperatorResponse getOperatorResponse = await invokeHubbleWebAPIServices.GetOperator(getOperatorRequest);
                //if (getOperatorResponse.Operator == null)
                //{
                //    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                //    return new Salida_getProductInfo
                //    {
                //        Resultado = false,
                //        Msj = "SHELLMX- OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA"
                //    };
                //}

                #region VALIDACION DEL OPERADOR ID | CODE
                List<SearchOperatorCriteria> SearchOperatorCriteriaOperator = new List<SearchOperatorCriteria>
                {
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Id , MatchingType = SearchOperatorCriteriaMatchingType.Exact },
                new SearchOperatorCriteria{ Text = request.Id_teller , Field = SearchOperatorCriteriaFieldType.Code , MatchingType = SearchOperatorCriteriaMatchingType.Exact }
                };
                SearchOperatorRequest searchOperatorRequest = new SearchOperatorRequest
                {
                    Identity = bsObj.Identity,
                    CriteriaList = SearchOperatorCriteriaOperator,
                    CriteriaRelationshipType = SearchCriteriaRelationshipType.Or,
                    MustIncludeDischarged = false
                };

                SearchOperatorResponse searchOperatorResponse = await invokeHubbleWebAPIServices.SearchOperator(searchOperatorRequest);
                string idOperatorObtTran = null;
                string codeOperatorOntTran = null;
                string nameOperator = null;

                if (searchOperatorResponse.OperatorList.Count == 0)
                {
                    //SHELLMX- Se manda una excepccion de que no corresponde el Operador en Turno.
                    Log("CODEVOL_FIN WARNING 52", "@SHELLMX- OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA. ID_TELLER: " + request.Id_teller.ToString() + " IDSEGUIMIENTO: " + criptoGetProd);
                    return new Salida_getProductInfo
                    {
                        Resultado = false,
                        Msj = "OPERADOR NO IDENTICADO O INEXISTENTE EN EL SISTEMA."
                    };
                }
                foreach (var searchOperator in searchOperatorResponse.OperatorList)
                {
                    if (searchOperatorResponse.OperatorList.Count == 1)
                    {
                        idOperatorObtTran = searchOperator.Id;
                        codeOperatorOntTran = searchOperator.Code;
                        nameOperator = searchOperator.Name;
                    }
                    if (searchOperatorResponse.OperatorList.Count == 2)
                    {
                        idOperatorObtTran = searchOperator.Id;
                        codeOperatorOntTran = searchOperator.Code;
                        nameOperator = searchOperator.Name;
                    }
                }

                #endregion

                GetProductForSaleRequest getProductForSaleRequest = new GetProductForSaleRequest()
                {
                    ProductId = request.IdProduct,
                    Quantity = 1,
                    Identity = bsObj.Identity,
                };
                GetProductForSaleResponse getProductForSaleResponse = await invokeHubbleWebAPIServices.GetProductForSale(getProductForSaleRequest);

                if (getProductForSaleResponse.Status == 1)
                {
                    salida.Resultado = true;
                    salida.Msj = getProductForSaleResponse.Message;
                    salida.producto = getProductForSaleResponse.ProductName;
                    salida.Id_producto = getProductForSaleResponse.ProductReference;
                    salida.importe = getProductForSaleResponse.FinalAmount;
                    salida.precio_Uni = getProductForSaleResponse.FinalAmount;
                    salida.mensajePromocion = "";
                    Log("CODEVOL_TR INFO", "OPERADOR QUE REALIZO LA PETICION DE OBTENER_INFO_PRODUCT : " + nameOperator + " IDSEGUIMIENTO: " + criptoGetProd);
                    Log("CODEVOL_FIN 1", "SE FINALIZO EL METODO DE GETPRODUCTINFO SATISFACTORIAMENTE CON EL RESPONSE IDSEGUIMIENTO: " + criptoGetProd + "\n {" + "\n" +
                        "    resultado: " + salida.Resultado.ToString() + "," + "\n" +
                        "    msj:  " + salida.Msj.ToString() + "," + "\n" +
                        "    producto: " + salida.producto.ToString() + "," + "\n" +
                        "    id_producto: " + salida.Id_producto.ToString() + "," + "\n" +
                        "    importe: " + salida.importe.ToString() + "," + "\n" +
                        "    precio_uni: " + salida.precio_Uni.ToString() + "\n" + "}");
                }
                else if (getProductForSaleResponse.Status < 0)
                {
                    Log("CODEVOL_FIN WARNING 51", "@SHELLMX- PRODUCTO NO EXISTE: RESPONSE. IDSEGUIMIENTO: " + criptoGetProd + "\n" + "Salida_getProductInfo: \n {" + "\n" +
                        "    status: " + getProductForSaleResponse.Status.ToString() + "," + "\n" +
                        "    message: " + getProductForSaleResponse.Message.ToString() + "\n" + "}");
                    salida.Resultado = false;
                    salida.Msj = "PRODUCTO NO EXISTE EN EL SISTEMA.";
                }
            }
            catch(Exception e)
            {
                salida.Msj = "ERROR INTERNO AL OBTENER INFO DEL PRODUCTO, VERIFICAR LOGS.";
                salida.Resultado = false;
                Log("CODEVOL_FIN ERROR 27", "@SHELLMX- HUBO UN ERROR AL ENTRAR AL METODO DE OBTENER_PRODUCTO. IDSEGUIMIENTO: " + criptoGetProd + "LOG:: " + e.ToString());
            }
            /*finally  // <--  termine bien o mal liberamos el semaforo
            {
                try
                {
                    SemaphoreSlimGetProductInfo?.Release();
                }
                catch (Exception)
                {
                }
            }*/
            return salida;
        }

        public async Task<Salida_Electronic_billing> Electronic_billing(Entrada_Electronic_billing request)
        {
            #region globales
            InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            Salida_Electronic_billing salida = new Salida_Electronic_billing();
            textosincaracterspc textosincarspecial = new textosincaracterspc();

            var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
            TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
            bool isFacturar = false;
            #endregion

            string numeroclientere = "";
            if (request.NoCliente == null || request.NoCliente.Trim() == "" || request.NoCliente == String.Empty)
            {
                numeroclientere = null;
            }

            if (request.EESS == null && request.NoCliente == null && request.Nticket == null && request.WebID == null)
            {
                salida.Msj = "DATOS INCORRECTOS";
                salida.Resultado = false;
            }

            if (request.EESS == null)
            {
                isFacturar = false;
                salida.Msj = "INTRODUSCA UN NUMERO DE ESTACION";
                salida.Resultado = false;
                return salida;
            }
            /* if (request.NoCliente == null)
             {
                 isFacturar = false;
                 salida.Msj = "INTRODUSCA UN NUMERO DE CLIENTE";
                 salida.Resultado = false;
                 return salida;
             }*/
            if (request.Nticket == null)
            {
                isFacturar = false;
                salida.Msj = "INTRODUSCA UN NUMERO DE TICKET";
                salida.Resultado = false;
                return salida;
            }
            if (request.WebID == null)
            {
                isFacturar = false;
                salida.Msj = "INTRODUSCA UN WEBID";
                salida.Resultado = false;
                return salida;
            }

            if (request.TipoOperacion == 0)
            {
                salida.Msj = "INTRODUSCA UNA OPERACION VALIDA PORFAVOR";
                salida.Resultado = false;
                return salida;
            }

            #region getprintingconfiguration

            //despues de crear la lista agregamos los siguientes campos para finalizar el reques de consumo en facturacion 
            GetPrintingConfigurationRequest requesGetPrinting = new GetPrintingConfigurationRequest
            {
                Identity = bsObj.Identity,
                UsecasesPrintingConfigurationList = new List<UsecasePrintingConfiguration> {
                    new UsecasePrintingConfiguration {
                        UseCase="SALE",
                        PrintingTemplatePlatformType= "VENTA HUBBLEPOS",
                        DefaultNumberOfCopies= 1,
                        Required= true
                    },
                    new UsecasePrintingConfiguration {
                        UseCase= "CLOSURE",
                        PrintingTemplatePlatformType= "CIERRE DE CAJA",
                        DefaultNumberOfCopies= 1,
                        Required= true
                    },
                    new UsecasePrintingConfiguration {
                          UseCase= "CASHBOX_RECORD",
                          PrintingTemplatePlatformType= "MOVIMIENTO DE CAJA HUBBLEPOS",
                          DefaultNumberOfCopies= 1,
                          Required= true
                    },
                    new UsecasePrintingConfiguration {
                          UseCase= "FUELLINGPOINT_TEST",
                          PrintingTemplatePlatformType= "SURTIDOR HUBBLEPOS",
                          DefaultNumberOfCopies= 1,
                          Required= false
                    },
                    new UsecasePrintingConfiguration {
                          UseCase= "REFUND",
                          PrintingTemplatePlatformType= "ANULACION HUBBLEPOS",
                          DefaultNumberOfCopies= 1,
                          Required= true
                    },
                    new UsecasePrintingConfiguration {
                        UseCase= "RUNAWAY",
                        PrintingTemplatePlatformType= "FUGA HUBBLEPOS",
                        DefaultNumberOfCopies= 1,
                        Required= true


                    },
                    new UsecasePrintingConfiguration {
                        UseCase= "COLLECTION",
                        PrintingTemplatePlatformType= "JUSTIFICANTE DE PAGO",
                        DefaultNumberOfCopies= 1,
                        Required= false
                    }

                }
            };

            GetPrintingConfigurationResponse responseGetPrinting = await invokeHubbleWebAPIServices.GetPrintingConfiguration(requesGetPrinting);



            List<string> listaPrinting = new List<string>();
            string key;
            foreach (var item in responseGetPrinting.GlobalSettings)
            {
                key = item.Value;
                if (key == null)

                {

                    key = string.Empty;

                }
                listaPrinting.Add(key);
            }
            listaPrinting.ToArray();

            //string Headerprin = listaPrinting[1];

            Headeresponse deserializeJsonheader = JsonConvert.DeserializeObject<Headeresponse>(listaPrinting[1]);
            footeresponse deserializeJsonfooter = JsonConvert.DeserializeObject<footeresponse>(listaPrinting[2]);





            // var responseGlobalSettings = responseGetPrinting.GlobalSettings.Values;


            //string ess = Count[1]=responseGlobalSettings.Values;
            #endregion

            #region GetDocument validacion del ticket y obtencion de datos ticket

            //despues de crear la lista agregamos los siguientes campos para finalizar el reques de consumo en facturacion 
            GetDocumentRequest requesgetdocument = new GetDocumentRequest
            {
                Identity = bsObj.Identity,
                Id = request.Nticket,
                UsageType = DocumentUsageType.PrintCopy,
            };

            GetDocumentResponse responsegetdocument = await invokeHubbleWebAPIServices.GetDocument(requesgetdocument);

            if (responsegetdocument.Document == null)
            {
                salida.Msj = "Ticket o Numero de cliente no valido";
                salida.Resultado = false;
                return salida;
            }

            #region datos del ticket si es !null

            string nticketorigin = responsegetdocument.Document.Id;
            int ntiquetn = nticketorigin.Length;
            string Folioidticket = nticketorigin.Substring((ntiquetn - 9), 9);


            string conletra = Convert.ToString(responsegetdocument.Document.TotalAmountWithTax);
            converletra nunletra = new converletra();
            //double numletra = Convert.ToDouble(conletra);
            string letraconvert = nunletra.enletras(conletra);

            // decimal caliva = (responsegetdocument.Document.TotalAmountWithTax) - (responsegetdocument.Document.TaxableAmount);


            //responseGetPrinting.GlobalSettings.Values;

            IList<Producto> listan = new List<Producto>();
            //lista = responsegetdocument.Document.LineList();
            foreach (DocumentLine item in responsegetdocument.Document.LineList)
            {
                listan.Add(new Producto { ProductName = item.ProductName, Quantity = item.Quantity, TotalAmountWithTax = (Math.Truncate(item.TotalAmountWithTax * 100) / 100).ToString("N2"), UnitaryPriceWithTax = (Math.Truncate(item.UnitaryPriceWithTax * 100) / 100).ToString("N2") });
            }

            IList<Iva> porcentaje = new List<Iva>();
            foreach (DocumentLine item in responsegetdocument.Document.LineList)
            {
                porcentaje.Add(new Iva { TaxPercentage = Convert.ToInt32(item.TaxPercentage), TaxAmount = (Math.Truncate(item.TaxAmount * 100) / 100).ToString("N2") });
            }
            
            //--------------------------------empieza iva ------------------------------------------------------------------------------
            string strImprime = String.Empty;
            string strImprime2 = String.Empty;
            int recorreUnicoIva = 0;
            string[] taxes;
            taxes = new string[porcentaje.Count()];
            IList<IvaUnico> ivaUnico = new List<IvaUnico>();
            int cuenta = 0;
            decimal decSumaIva = 0;
            foreach (var item in porcentaje)
            {
                if (Array.IndexOf(taxes, item.TaxPercentage.ToString()) == -1)
                {
                    ivaUnico.Add(new IvaUnico { Iva = item.TaxPercentage });
                    taxes[cuenta] = item.TaxPercentage.ToString();
                }
                cuenta += 1;
            }
            foreach (var item in ivaUnico)
            {
                foreach (var item2 in porcentaje)
                {
                    if (item.Iva.ToString() == item2.TaxPercentage.ToString())
                        decSumaIva += decimal.Parse(item2.TaxAmount);
                }
                if (recorreUnicoIva == 0)
                {
                    //strImprime = "IVA " + item.Iva.ToString() + "%:     " + (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                    strImprime = item.Iva.ToString() + "%:";
                    strImprime2 = (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                }
                else
                    //strImprime += " | IVA " + item.Iva.ToString() + "%:     " + (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                    strImprime += item.Iva.ToString() + "%:";
                strImprime2 = (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                decSumaIva = 0;
                recorreUnicoIva += 1;
            }
            string salidaiva = strImprime.ToString();
            string salidaivamonto = strImprime2.ToString();
            //----------------------------------------------------termina iva--------------------------------------------------------------------

            GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest { Identity = bsObj.Identity };
            GetPOSInformationResponse getPOSInformationResponse = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);

            //---------------------------------------------------mrtodo de pago------------------------------------------------------------------
            IList<PaymentDetail> paymentDetails = new List<PaymentDetail>();
            //lista = responsegetdocument.Document.LineList();
            foreach (DocumentPaymentDetail item in responsegetdocument.Document.PaymentDetailList)
            {
                paymentDetails.Add(new PaymentDetail { PaymentMethodId = item.PaymentMethodId});
            }
            string metodospayment = paymentDetails.ToString();

            string[] arraymetodopago = new string[paymentDetails.Count];
            int i = 0;
            foreach (PaymentDetail item in paymentDetails)
            {
                arraymetodopago[i++] = item.PaymentMethodId;
            }
            string metodopago = String.Join(" | ", arraymetodopago);

            metodopago = metodopago.Replace(
                 getPOSInformationResponse.PosInformation.CompanyCode + "00", "VARIOS")
                .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "01", "EFECTIVO")
                .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "02", "CHEQUE")
                .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "04", "TRANSFERENCIA")
                .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "07", "VALE")
                .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "08", "TARJETA")
                .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "09", "FUGA")
                .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "10", "PRUEBA")

                ;


            //---------------------------------------------------termina metodo de pago---------------------------------------------------------

            string serieWebId = null;


            GetSeriesRequest getSeriesRequest = new GetSeriesRequest { Identity = bsObj.Identity };
            GetSeriesResponse getSeriesResponse = await invokeHubbleWebAPIServices.GetSeries(getSeriesRequest);

            foreach (var series in getSeriesResponse.SeriesList)
            {
                serieWebId = series.Code;
            }




            string formatofecha = Convert.ToString(responsegetdocument.Document.EmissionLocalDateTime);
            DateTimeOffset fechaticketstring = DateTimeOffset.Parse(formatofecha);
            string fechaticket = Convert.ToString(fechaticketstring.DateTime);
            string nticketco = responsegetdocument.Document.Id;
            string horaFormatFact = fechaticket.Replace(" ", "");

            string hourWebID = horaFormatFact.Substring(10, 2);
            string companyEESS = getPOSInformationResponse.PosInformation.CompanyCode;
            string minutWebID = horaFormatFact.Substring(13, 2);
            string serieTicket = serieWebId;
            string secontWebID = horaFormatFact.Substring(16, 2);

            string webidnwe = string.Concat(hourWebID + companyEESS + minutWebID + serieTicket + secontWebID);





            //string formatofecha = Convert.ToString(responsegetdocument.Document.EmissionLocalDateTime);
            //DateTimeOffset fechaticketstring = DateTimeOffset.Parse(formatofecha);
            //string fechaticket = Convert.ToString(fechaticketstring.DateTime);
            //string nticketco = responsegetdocument.Document.Id;
            //string horaformatnews = fechaticket.Replace(" ", "");

            //string wid = horaformatnews.Substring(10, 2);
            //string wid2 = nticketco.Substring(0, 5);
            //string wid3 = horaformatnews.Substring(13, 2);
            //string wid4 = nticketco.Substring(5, 4);
            //string wid5 = horaformatnews.Substring(16, 2);

            //string webidnwe = string.Concat(wid + wid2 + wid3 + wid4 + wid5);
            #endregion


            #endregion


            #region cliente
            GetCustomerRequest resquestcustomer = new GetCustomerRequest
            {
                Id = request.NoCliente,
                Identity = bsObj.Identity
            };



            //InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            GetCustomerResponse responsecustomer = await invokeHubbleWebAPIServices.GetCustomer(resquestcustomer);
            string rfccliente = "";
            string razoonsocial = "";
            if (responsecustomer.Customer != null)
            {
                rfccliente = responsecustomer.Customer.TIN;
                razoonsocial = responsecustomer.Customer.BusinessName;

            }
            if (responsecustomer.Customer == null)
            {
                rfccliente = null;
                razoonsocial = null;

            }

            //if (responsecustomer.Customer.BusinessName == null && responsecustomer.Customer.TIN == null)
            //{
            //    isFacturar = false;
            //    salida.Resultado = false;
            //    salida.Msj = "NO SE PUDO ENCONTRAR LA INFORMACION DEL CLIENTE";
            //    return salida;
            //}

            #endregion


            if (request.TipoOperacion == 1)
            {
                if (responsegetdocument.Document == null && responsecustomer.Customer == null)
                {
                    salida.Msj = "Numero de cliente y numero de ticket no valido";
                    salida.Resultado = false;
                    return salida;
                }
                if (responsegetdocument.Document == null && responsecustomer.Customer != null)
                {
                    salida.Msj = "Numero de ticket no valido";
                    salida.Resultado = false;
                    return salida;
                }
                if (responsegetdocument.Document != null && responsecustomer.Customer == null)
                {
                    isFacturar = false;
                    salida.Resultado = true;
                    salida.Msj = "OPERACION REALIZADA CON EXITO";
                }
                if (responsegetdocument.Document != null && responsecustomer.Customer != null)
                {
                    salida.RazonSocial = textosincarspecial.transformtext(razoonsocial);
                    salida.RFC = rfccliente;
                    isFacturar = false;
                    salida.Resultado = true;
                    salida.Msj = "OPERACION REALIZADA CON EXITO";
                }


            }

            //if (request.TipoOperacion == 1 && responsecustomer.Customer == null || responsegetdocument.Document == null)
            //{
            //    isFacturar = false;

            //    salida.Resultado = false;
            //    salida.Msj = "El cliente y el ticket no son validos";
            //    return salida;
            //}







            if (request.TipoOperacion == 2)
            {
                if (responsegetdocument.Document == null && responsecustomer.Customer == null)
                {
                    salida.Msj = "Numero de cliente y numero de ticket no valido";
                    salida.Resultado = false;
                    return salida;
                }
                if (responsegetdocument.Document == null && responsecustomer.Customer != null)
                {
                    salida.Msj = "Numero de ticket no valido";
                    salida.Resultado = false;
                    return salida;
                }
                //if ((responsegetdocument.Document != null && request.NoCliente == null )||
                //    (responsegetdocument.Document != null && request.NoCliente.Trim() == "" )||
                //    (responsegetdocument.Document != null && request.NoCliente == String.Empty)
                //   )
                if (responsegetdocument.Document != null && numeroclientere == null)
                {
                    isFacturar = false;
                    salida.Resultado = true;
                    salida.Msj = "Numero de cliente no valido";

                }
                if (responsegetdocument.Document != null && responsecustomer.Customer == null && numeroclientere != null)
                {
                    isFacturar = false;
                    salida.Resultado = true;
                    salida.Msj = "Numero de cliente no valido";
                    return salida;
                }
                if (responsegetdocument.Document != null && responsecustomer.Customer != null)
                {
                    salida.RazonSocial = textosincarspecial.transformtext(razoonsocial);
                    salida.RFC = rfccliente;
                    isFacturar = true;
                    //salida.Resultado = true;
                    //salida.Msj = "OPERACION REALIZADA CON EXITO";
                }



                //if (responsecustomer.Customer == null)
                //{

                //    isFacturar = false;
                //    rfccliente = null;
                //    razoonsocial = null;
                //    salida.Msj = "OPERACION REALIZADA CON ÉXITO";
                //    salida.Resultado = true;


                //}
                //if (responsecustomer.Customer != null)
                //{
                //    isFacturar = true;
                //    salida.RazonSocial = textosincarspecial.transformtext(razoonsocial);
                //    salida.RFC = rfccliente;
                //}

            }




            #region information
            //// GetPOSInformationResponse  GetPOSInformation(GetPosInformationRequest getPosInformationRequest
            //GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest
            //{
            //    Identity = bsObj.Identity
            //};

            //   //   InvokeHubbleWebAPIServices invokeHubbleWebAPIServices3 = new InvokeHubbleWebAPIServices();
            //InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            GetPOSInformationResponse informationresponses = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);

            #endregion




            if (isFacturar)
            {

                GetCompanyRequest GetCompanyreques = new GetCompanyRequest
                {
                    Identity = bsObj.Identity
                };

                //InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
                GetCompanyResponse responseCompany = await invokeHubbleWebAPIServices.GetCompany(GetCompanyreques);



                #region facturacion
                //se nesesitan estos datos para facturar agregamos

                var res = new ListTicketDAO
                {
                    EESS = request.EESS,
                    NTicket = request.Nticket,
                    //RFC = "AAA010101AAA",
                    RFC = rfccliente,
                    WebID = request.WebID
                };

                //despues de crear la lista agregamos los siguientes campos para finalizar el reques de consumo en facturacion 

                GenerateElectronicInvoice requestfac = new GenerateElectronicInvoice
                {
                    EmpresaPortal = "01",
                    Company = responseCompany.Company.Id,
                    ListTicket = new List<ListTicketDAO>
                    {
                        new ListTicketDAO
                        {
                            EESS =res.EESS,
                            NTicket=res.NTicket,
                            RFC=res.RFC,
                            WebID=res.WebID
                        }
                    }
                };



                facresponse responsefacturacion = await invokeHubbleWebAPIServices.tpvfacturacionn(requestfac);



                if (responsefacturacion.mensaje == "DATOS DEL TICKET NO VALIDOS PARA FACTURAR")
                {
                    //salida.Ticket = request.Nticket;
                    //salida.FormaPago = metodopago;//"EFECTIVO"; //(responsegetdocument.Document.PaymentDetailList[0].PaymentMethodId);//pendiente por modificar
                    //salida.Subtotal = (Math.Truncate(responsegetdocument.Document.TaxableAmount * 100) / 100).ToString("N2");
                    //salida.Terminal = responsegetdocument.Document.PosId;
                    //salida.Operador = responsegetdocument.Document.OperatorName;
                    //salida.Folio = Folioidticket;
                    //salida.Total = (Math.Truncate(responsegetdocument.Document.TotalAmountWithTax * 100) / 100).ToString("N2");
                    //salida.ImporteEnLetra = letraconvert;
                    //salida.iva = salidaiva;
                    //salida.ivaMonto = salidaivamonto;
                    //salida.productos = listan;
                    //salida.Fecha = fechaticket;
                    //salida.WebID = webidnwe;
                    //salida.Estacion = informationresponses.PosInformation.ShopCode;
                    salida.Msj = responsefacturacion.mensaje;
                    salida.Resultado = false;
                    return salida;
                }
                if (responsefacturacion.mensaje == "DATOS DEL TICKET INCORRECTO PARA FACTURAR")
                {
                    //salida.Ticket = request.Nticket;
                    //salida.FormaPago = metodopago;//"EFECTIVO"; //(responsegetdocument.Document.PaymentDetailList[0].PaymentMethodId);//pendiente por modificar
                    //salida.Subtotal = (Math.Truncate(responsegetdocument.Document.TaxableAmount * 100) / 100).ToString("N2");
                    //salida.Terminal = responsegetdocument.Document.PosId;
                    //salida.Operador = responsegetdocument.Document.OperatorName;
                    //salida.Folio = Folioidticket;
                    //salida.Total = (Math.Truncate(responsegetdocument.Document.TotalAmountWithTax * 100) / 100).ToString("N2");
                    //salida.ImporteEnLetra = letraconvert;
                    //salida.iva = salidaiva;
                    //salida.ivaMonto = salidaivamonto;
                    //salida.productos = listan;
                    //salida.Fecha = fechaticket;
                    //salida.WebID = webidnwe;
                    //salida.Estacion = informationresponses.PosInformation.ShopCode;
                    salida.Msj = responsefacturacion.mensaje;
                    salida.Resultado = false;
                    return salida;
                }
                if (responsefacturacion.mensaje == "NO SE PUDO ENCONTRAR EL SERVICIO DE FACTURACION")
                {
                    salida.Msj = responsefacturacion.mensaje;
                    salida.Resultado = true;
                    return salida;
                }
                if (responsefacturacion.mensaje == "FACTURACION CORRECTA")
                {
                    salida.SelloDigitaSAT = responsefacturacion.SelloDigitaSAT;
                    salida.SelloDigitaCFDI = responsefacturacion.SelloDigitaCFDI;
                    salida.CadenaOrigTimbre = responsefacturacion.CadenaOrigTimbre;
                    salida.FolioFiscal = responsefacturacion.FolioFiscal;
                    salida.RFCProveedorCert = responsefacturacion.RFCProveedorCert;
                    salida.NumCertificado = responsefacturacion.NumCertificado;
                    salida.DateCertificacion = responsefacturacion.DateCertificacion;
                    salida.Msj = responsefacturacion.mensaje;
                    salida.Representacioncfdi = "Este documento es una representacion impresa de un CFDI";
                    salida.Resultado = true;
                }

                if (responsefacturacion.mensaje == "ERROR DE TIMBRADO AL FACTURAR")
                {
                    salida.Msj = responsefacturacion.mensaje;
                    salida.Resultado = true;
                    return salida;
                }

                if (responsefacturacion.mensaje == "NO SE PUDO ENCONTRAR EL SERVICIO DE FACTURACION")
                {
                    salida.Msj = "NO SE PUDO FACTURAR  INTENTELO MAS TARDE";
                    salida.Resultado = true;
                    return salida;

                }
                //else
                //{


                //    salida.Msj = "NO SE PUDO FACTURAR";
                //    salida.Resultado = false;

                //}
                #endregion




            }

            salida.Ticket = request.Nticket;
            salida.FormaPago = metodopago;//"EFECTIVO"; //(responsegetdocument.Document.PaymentDetailList[0].PaymentMethodId);//pendiente por modificar
            salida.Subtotal = (Math.Truncate(responsegetdocument.Document.TaxableAmount * 100) / 100).ToString("N2");
            salida.Terminal = responsegetdocument.Document.PosId;
            salida.Operador = responsegetdocument.Document.OperatorName;
            salida.Folio = Folioidticket;
            salida.Total = (Math.Truncate(responsegetdocument.Document.TotalAmountWithTax * 100) / 100).ToString("N2");
            salida.ImporteEnLetra = letraconvert;
            salida.iva = salidaiva;
            salida.ivaMonto = salidaivamonto;
            salida.productos = listan;
            salida.Fecha = fechaticket;
            salida.WebID = webidnwe;
            salida.Estacion = informationresponses.PosInformation.ShopCode;
            salida.HeaderTick1 = textosincarspecial.transformtext(deserializeJsonheader.Header1);
            salida.HeaderTick2 = textosincarspecial.transformtext(deserializeJsonheader.Header2);
            salida.HeaderTick3 = deserializeJsonheader.Header3;
            salida.HedaerTick4 = textosincarspecial.transformtext(deserializeJsonheader.Header4);
            salida.FooterTick1 = textosincarspecial.transformtext(deserializeJsonfooter.Footer1);
            salida.FooterTick2 = textosincarspecial.transformtext(deserializeJsonfooter.Footer2);
            salida.FooterTick3 = textosincarspecial.transformtext(deserializeJsonfooter.Footer3);
            salida.FooterTick4 = textosincarspecial.transformtext(deserializeJsonfooter.Footer4);
            salida.FooterTick5 = deserializeJsonfooter.Footer5;
            salida.CodigoPostalCompania = textosincarspecial.transformtext(listaPrinting[17]);
            salida.CodigoPostalTienda = textosincarspecial.transformtext(listaPrinting[27]);
            salida.ColoniaCompania = textosincarspecial.transformtext(listaPrinting[16]);
            salida.ColoniaTienda = textosincarspecial.transformtext(listaPrinting[29]);
            salida.DireccionCompania = textosincarspecial.transformtext(listaPrinting[14]);
            salida.DireccionTienda = textosincarspecial.transformtext(listaPrinting[24]);
            salida.EstadoCompania = textosincarspecial.transformtext(listaPrinting[19]);
            salida.EstadoTienda = textosincarspecial.transformtext(listaPrinting[31]);
            salida.ExpedicionTienda = textosincarspecial.transformtext(listaPrinting[27]);
            salida.MunicipioCompania = textosincarspecial.transformtext(listaPrinting[16]);
            salida.MunicipioTienda = textosincarspecial.transformtext(listaPrinting[26]);
            salida.NombreCompania = textosincarspecial.transformtext(listaPrinting[15]);
            salida.PaisCompania = textosincarspecial.transformtext(listaPrinting[20]);
            salida.PaisTienda = textosincarspecial.transformtext(listaPrinting[30]);
            salida.PermisoCRE = listaPrinting[32];
            salida.Tienda = textosincarspecial.transformtext(listaPrinting[25]);
            salida.RegFiscal = "REGIMEN GENERAL DE LEY PERSONAS MORALES";
            salida.RfcCompania = textosincarspecial.transformtext(listaPrinting[5]);
            return salida;
        }

        public async Task<Salida_Electronic_billing_FP> Electronic_billing_FP(Entrada_Electronic_billing_FP request)
        {
            InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
            Salida_Electronic_billing_FP salida = new Salida_Electronic_billing_FP();
            textosincaracterspc textosincarspecial = new textosincaracterspc();
            LogsTPVHP exec = new LogsTPVHP();
            var criptoElecB = DateTime.Now.ToString("yyyyMM") + "_ELECBIL_" + DateTime.Now.ToString("h-mm-ss-ffff");
            try
            {
                // si está ocupado se espera.
                //await SemaphoreSlimElectronic_billing_FP.WaitAsync();

                #region globales
                Log("CODEVOL_INI INFO", "SE INICIA CON EL METODO Electronic_billing PARA LA FACTURACION Y VARIOS. IDSEGUIMIENTO: " + criptoElecB);
                Log("CODEVOL_TR INFO", "EL REQUEST QUE ENTREGA PC " + criptoElecB  + "\n Electronic_Billing: \n {" + "\n" +
                    "    idPOS: " + request.idpos.ToString() + "," + "\n" +
                    "    nHD: " + request.nHD.ToString() + "," + "\n" +
                    "    noCliente: " + request.NoCliente.ToString() + "," + "\n" +
                    "    nTicket: " + request.Nticket.ToString() + "," + "\n" +
                    "    pss: " + request.pss.ToString() + "," + "\n" +
                    "    pTid: " + request.PTID.ToString() + "," + "\n" +
                    "    serial: " + request.Serial.ToString() + "," + "\n" +
                    "    tipoOperacion: " + request.TipoOperacion.ToString() + "," + "\n" +
                    "    webId: " + request.WebID.ToString() + "\n" + "}");
                var jsonTPVToken = System.IO.File.ReadAllText("C:/dist/tpv.config.json");
                TokenTPV bsObj = JsonConvert.DeserializeObject<TokenTPV>(jsonTPVToken);
                bool isFacturar = false;
                #endregion
               // bool isFacturar_publicogenral = false;
               // bool isFacturar_publicogenralEnero = false;
                string numeroclientere = "";
                if (request.NoCliente == null || request.NoCliente.Trim() == "" || request.NoCliente == String.Empty)
                {
                    numeroclientere = null;
                }

                if (request.EESS == null && request.NoCliente == null && request.Nticket == null && request.WebID == null)
                {
                    salida.Msj = "DATOS DE ENTRADA PARA TICKET INCORRECTOS.";
                    salida.Resultado = false;
                }

                if (request.EESS == null)
                {
                    Log("CODEVOL_FIN WARNING 50", "@SHELLMX- ERROR CON EL NUMERO DE LA ESTACION. IDSEGUIMIENTO: " + criptoElecB);
                    isFacturar = false;
                    salida.Msj = "INTRODUSCA UN NUMERO DE ESTACION VALIDO.";
                    salida.Resultado = false;
                    return salida;
                }
                /* if (request.NoCliente == null)
                 {
                     isFacturar = false;
                     salida.Msj = "INTRODUSCA UN NUMERO DE CLIENTE";
                     salida.Resultado = false;
                     return salida;
                 }*/
                if (request.Nticket == null)
                {
                    Log("CODEVOL_FIN WARNING 49", "@SHELLMX- ERROR CON EL NUMERO DEL TICKET. IDSEGUIMIENTO: " + criptoElecB);
                    isFacturar = false;
                    salida.Msj = "INTRODUSCA UN NUMERO DE TICKET CORRECTO.";
                    salida.Resultado = false;
                    return salida;
                }
                if (request.WebID == null)
                {
                    Log("CODEVOL_FIN WARNING 48", "@SHELLMX- ERROR CON EL NUMERO DEL WEBID. IDSEGUIMIENTO: " + criptoElecB);
                    isFacturar = false;
                    salida.Msj = "INTRODUSCA UN WEBID CORRECTO.";
                    salida.Resultado = false;
                    return salida;
                }

                if (request.TipoOperacion == 0)
                {
                    Log("CODEVOL_FIN WARNING 47", "@SHELLMX- ERROR CON EL TIPO DE OPERACION VALIDAR. IDSEGUIMIENTO: " + criptoElecB);
                    salida.Msj = "INTRODUSCA UN TIPO DE OPERACION VALIDO.";
                    salida.Resultado = false;
                    return salida;
                }

                #region getprintingconfiguration

                //despues de crear la lista agregamos los siguientes campos para finalizar el reques de consumo en facturacion 
                GetPrintingConfigurationRequest requesGetPrinting = new GetPrintingConfigurationRequest
                {
                    Identity = bsObj.Identity,
                    UsecasesPrintingConfigurationList = new List<UsecasePrintingConfiguration> {
                    new UsecasePrintingConfiguration {
                        UseCase="SALE",
                        PrintingTemplatePlatformType= "VENTA HUBBLEPOS",
                        DefaultNumberOfCopies= 1,
                        Required= true
                    },
                    new UsecasePrintingConfiguration {
                        UseCase= "CLOSURE",
                        PrintingTemplatePlatformType= "CIERRE DE CAJA",
                        DefaultNumberOfCopies= 1,
                        Required= true
                    },
                    new UsecasePrintingConfiguration {
                          UseCase= "CASHBOX_RECORD",
                          PrintingTemplatePlatformType= "MOVIMIENTO DE CAJA HUBBLEPOS",
                          DefaultNumberOfCopies= 1,
                          Required= true
                    },
                    new UsecasePrintingConfiguration {
                          UseCase= "FUELLINGPOINT_TEST",
                          PrintingTemplatePlatformType= "SURTIDOR HUBBLEPOS",
                          DefaultNumberOfCopies= 1,
                          Required= false
                    },
                    new UsecasePrintingConfiguration {
                          UseCase= "REFUND",
                          PrintingTemplatePlatformType= "ANULACION HUBBLEPOS",
                          DefaultNumberOfCopies= 1,
                          Required= true
                    },
                    new UsecasePrintingConfiguration {
                        UseCase= "RUNAWAY",
                        PrintingTemplatePlatformType= "FUGA HUBBLEPOS",
                        DefaultNumberOfCopies= 1,
                        Required= true


                    },
                    new UsecasePrintingConfiguration {
                        UseCase= "COLLECTION",
                        PrintingTemplatePlatformType= "JUSTIFICANTE DE PAGO",
                        DefaultNumberOfCopies= 1,
                        Required= false
                    }

                }
                };

                GetPrintingConfigurationResponse responseGetPrinting = await invokeHubbleWebAPIServices.GetPrintingConfiguration(requesGetPrinting);



                List<string> listaPrinting = new List<string>();
                string key;
                foreach (var item in responseGetPrinting.GlobalSettings)
                {
                    key = item.Value;
                    if (key == null)

                    {

                        key = string.Empty;

                    }
                    listaPrinting.Add(key);
                }
                listaPrinting.ToArray();

                //string Headerprin = listaPrinting[1];

                Headeresponse deserializeJsonheader = JsonConvert.DeserializeObject<Headeresponse>(listaPrinting[1]);
                footeresponse deserializeJsonfooter = JsonConvert.DeserializeObject<footeresponse>(listaPrinting[2]);





                // var responseGlobalSettings = responseGetPrinting.GlobalSettings.Values;


                //string ess = Count[1]=responseGlobalSettings.Values;
                #endregion

                #region GetDocument validacion del ticket y obtencion de datos ticket

                //despues de crear la lista agregamos los siguientes campos para finalizar el reques de consumo en facturacion 
                GetDocumentRequest requesgetdocument = new GetDocumentRequest
                {
                    Identity = bsObj.Identity,
                    Id = request.Nticket,
                    UsageType = DocumentUsageType.PrintCopy,
                };

                GetDocumentResponse responsegetdocument = await invokeHubbleWebAPIServices.GetDocument(requesgetdocument);

                if (responsegetdocument.Document == null)
                {
                    Log("CODEVOL_FIN WARNING 46", "@SHELLMX- TICKET O NUMERO DE CLIENTE NO VALIDO. IDSEGUIMIENTO: " + criptoElecB);
                    salida.Msj = "TICKET O NUMERO DE CLIENTE NO VALIDO O NO EXISTE.";
                    salida.Resultado = false;
                    return salida;
                }

                #region datos del ticket si es !null

                string nticketorigin = responsegetdocument.Document.Id;
                int ntiquetn = nticketorigin.Length;
                string Folioidticket = nticketorigin.Substring((ntiquetn - 9), 9);


                string conletra = Convert.ToString(responsegetdocument.Document.TotalAmountWithTax);
                converletra nunletra = new converletra();
                //double numletra = Convert.ToDouble(conletra);
                string letraconvert = nunletra.enletras(conletra);

                // decimal caliva = (responsegetdocument.Document.TotalAmountWithTax) - (responsegetdocument.Document.TaxableAmount);


                //responseGetPrinting.GlobalSettings.Values;

                IList<Producto> listan = new List<Producto>();
                //lista = responsegetdocument.Document.LineList();
                foreach (DocumentLine item in responsegetdocument.Document.LineList)
                {
                    listan.Add(new Producto { ProductName = item.ProductName, Quantity = item.Quantity, TotalAmountWithTax = (Math.Truncate(item.TotalAmountWithTax * 100) / 100).ToString("N2"), UnitaryPriceWithTax = (Math.Truncate(item.UnitaryPriceWithTax * 100) / 100).ToString("N2") });
                }

                IList<Iva> porcentaje = new List<Iva>();
                foreach (DocumentLine item in responsegetdocument.Document.LineList)
                {
                    porcentaje.Add(new Iva { TaxPercentage = Convert.ToInt32(item.TaxPercentage), TaxAmount = (Math.Truncate(item.TaxAmount * 100) / 100).ToString("N2") });
                }

                //alamacena metodo pago
                IList<PaymentDetail_FP> paymentDetails = new List<PaymentDetail_FP>();
                //lista = responsegetdocument.Document.LineList();
                foreach (DocumentPaymentDetail item in responsegetdocument.Document.PaymentDetailList)
                {
                    paymentDetails.Add(new PaymentDetail_FP { PaymentMethodId = item.PaymentMethodId, PrimaryCurrencyTakenAmount = item.PrimaryCurrencyTakenAmount });
                }

                //--------------------------------empieza iva ------------------------------------------------------------------------------
                string strImprime = String.Empty;
                string strImprime2 = String.Empty;
                int recorreUnicoIva = 0;
                string[] taxes;
                taxes = new string[porcentaje.Count()];
                IList<IvaUnico> ivaUnico = new List<IvaUnico>();
                int cuenta = 0;
                decimal decSumaIva = 0;
                foreach (var item in porcentaje)
                {
                    if (Array.IndexOf(taxes, item.TaxPercentage.ToString()) == -1)
                    {
                        ivaUnico.Add(new IvaUnico { Iva = item.TaxPercentage });
                        taxes[cuenta] = item.TaxPercentage.ToString();
                    }
                    cuenta += 1;
                }
                foreach (var item in ivaUnico)
                {
                    foreach (var item2 in porcentaje)
                    {
                        if (item.Iva.ToString() == item2.TaxPercentage.ToString())
                            decSumaIva += decimal.Parse(item2.TaxAmount);
                    }
                    if (recorreUnicoIva == 0)
                    {
                        //strImprime = "IVA " + item.Iva.ToString() + "%:     " + (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                        strImprime = item.Iva.ToString() + "%:";
                        strImprime2 = (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                    }
                    else
                        //strImprime += " | IVA " + item.Iva.ToString() + "%:     " + (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                        strImprime += item.Iva.ToString() + "%:";
                    strImprime2 = (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                    decSumaIva = 0;
                    recorreUnicoIva += 1;
                }
                string salidaiva = strImprime.ToString();
                string salidaivamonto = strImprime2.ToString();
                //----------------------------------------------------termina iva--------------------------------------------------------------------

                GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest { Identity = bsObj.Identity };
                GetPOSInformationResponse getPOSInformationResponse = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);

                //---------------------------------------------------mrtodo de pago------------------------------------------------------------------
                //IList<PaymentDetail> paymentDetails = new List<PaymentDetail>();
                ////lista = responsegetdocument.Document.LineList();
                //foreach (DocumentPaymentDetail item in responsegetdocument.Document.PaymentDetailList)
                //{
                //    paymentDetails.Add(new PaymentDetail { PaymentMethodId = item.PaymentMethodId, item.PrimaryCurrencyGivenAmount });
                //}
                // string metodospayment = paymentDetails.ToString();

                //string[] arraymetodopago = new string[paymentDetails.Count];
                //int i = 0;
                //foreach (PaymentDetail item in paymentDetails)
                //{
                //    arraymetodopago[i++] = item.PaymentMethodId;
                //}
                //string metodopago = String.Join(" | ", arraymetodopago);

                //metodopago = metodopago.Replace(
                //     getPOSInformationResponse.PosInformation.CompanyCode + "00", "VARIOS")
                //    .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "01", "EFECTIVO")
                //    .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "02", "CHEQUE")
                //    .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "04", "TRANSFERENCIA")
                //    .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "07", "VALE")
                //    .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "08", "TARJETA")
                //    .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "09", "FUGA")
                //    .Replace(getPOSInformationResponse.PosInformation.CompanyCode + "10", "PRUEBA")

                //    ;

                IList<pagosimprimir_FP> guardarpagos = new List<pagosimprimir_FP>();

                string strImprimepago = String.Empty;
                string strImprime2pago = String.Empty;

                int pagosunico = 0;
                string[] taxesp;
                taxesp = new string[paymentDetails.Count()];
                IList<Pagounico_FP> pagounico = new List<Pagounico_FP>();
                int cuentap = 0;
                decimal decSumapago = 0;
                foreach (var item in paymentDetails)
                {
                    if (Array.IndexOf(taxesp, item.PaymentMethodId.ToString()) == -1)
                    {
                        pagounico.Add(new Pagounico_FP { pago = item.PaymentMethodId });
                        taxesp[cuentap] = item.PaymentMethodId.ToString();
                    }
                    cuentap += 1;
                }
                foreach (var item in pagounico)
                {
                    foreach (var item2 in paymentDetails)
                    {
                        if (item.pago.ToString() == item2.PaymentMethodId.ToString())
                            decSumapago += item2.PrimaryCurrencyTakenAmount;
                    }
                    if (pagosunico == 0)
                    {
                        //strImprime = "IVA " + item.Iva.ToString() + "%:     " + (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");
                        strImprimepago = item.pago.ToString();
                        strImprime2pago = (Math.Truncate(decSumapago * 100) / 100).ToString("N2");
                        //porcentajelistaguardar.Add(new Iva { TaxPercentage = Convert.ToInt32(strImprime), TaxAmount = strImprime2 });
                    }
                    else
                        //strImprime += " | IVA " + item.Iva.ToString() + "%:     " + (Math.Truncate(decSumaIva * 100) / 100).ToString("N2");


                        strImprimepago = item.pago.ToString();
                    //if (strImprime=="16")
                    //{
                    //    strImprimefinal = "targeta";
                    //}


                    strImprime2pago = Convert.ToString((Math.Truncate(decSumapago * 100) / 100).ToString("N2"));
                    guardarpagos.Add(new pagosimprimir_FP { PaymentMethodId = strImprimepago, PrimaryCurrencyTakenAmount = strImprime2pago });

                    decSumapago = 0;
                    pagosunico += 1;

                }
                IList<pagosimprimir_FP> pagosimpreso = new List<pagosimprimir_FP>();


                foreach (pagosimprimir_FP item in guardarpagos)
                {
                    if (item.PaymentMethodId == getPOSInformationResponse.PosInformation.CompanyCode + "00")
                    {
                        item.PaymentMethodId = "VARIOS";

                    }
                    if (item.PaymentMethodId == getPOSInformationResponse.PosInformation.CompanyCode + "01")
                    {
                        item.PaymentMethodId = "EFECTIVO";

                    }                  
                    if (item.PaymentMethodId == getPOSInformationResponse.PosInformation.CompanyCode + "03")
                    {
                        item.PaymentMethodId = "AMERICAN EXPRESS";
                    }                  
                   
                    if (item.PaymentMethodId == getPOSInformationResponse.PosInformation.CompanyCode + "07")
                    {
                        item.PaymentMethodId = "VALE";
                        //porcentajelistaguardar2.Add(new Iva2 { TaxPercentage = "targeta", TaxAmount = strImprime2 });
                    }
                    if (item.PaymentMethodId == getPOSInformationResponse.PosInformation.CompanyCode + "08")
                    {
                        item.PaymentMethodId = "TARJETA";
                    }
                    if (item.PaymentMethodId == getPOSInformationResponse.PosInformation.CompanyCode + "09")
                    {
                        item.PaymentMethodId = "FUGA";
                        //porcentajelistaguardar2.Add(new Iva2 { TaxPercentage = "targeta", TaxAmount = strImprime2 });
                    }
                    if (item.PaymentMethodId == getPOSInformationResponse.PosInformation.CompanyCode + "10")
                    {
                        item.PaymentMethodId = "SODEXO";
                    }


                    pagosimpreso.Add(new pagosimprimir_FP { PaymentMethodId = item.PaymentMethodId, PrimaryCurrencyTakenAmount = item.PrimaryCurrencyTakenAmount });

                }

                //---------------------------------------------------termina metodo de pago---------------------------------------------------------

                string serieWebId = null;


                GetSeriesRequest getSeriesRequest = new GetSeriesRequest { Identity = bsObj.Identity };
                GetSeriesResponse getSeriesResponse = await invokeHubbleWebAPIServices.GetSeries(getSeriesRequest);

                foreach (var series in getSeriesResponse.SeriesList)
                {
                    serieWebId = series.Code;
                }




                string formatofecha = Convert.ToString(responsegetdocument.Document.EmissionLocalDateTime);
                DateTimeOffset fechaticketstring = DateTimeOffset.Parse(formatofecha);
                string fechaticket = Convert.ToString(fechaticketstring.DateTime);
                string nticketco = responsegetdocument.Document.Id;
                string horaFormatFact = fechaticket.Replace(" ", "");

                string hourWebID = horaFormatFact.Substring(10, 2);
                string companyEESS = getPOSInformationResponse.PosInformation.CompanyCode;
                string minutWebID = horaFormatFact.Substring(13, 2);
                string serieTicket = serieWebId;
                string secontWebID = horaFormatFact.Substring(16, 2);

                string webidnwe = string.Concat(hourWebID + companyEESS + minutWebID + serieTicket + secontWebID);





                //string formatofecha = Convert.ToString(responsegetdocument.Document.EmissionLocalDateTime);
                //DateTimeOffset fechaticketstring = DateTimeOffset.Parse(formatofecha);
                //string fechaticket = Convert.ToString(fechaticketstring.DateTime);
                //string nticketco = responsegetdocument.Document.Id;
                //string horaformatnews = fechaticket.Replace(" ", "");

                //string wid = horaformatnews.Substring(10, 2);
                //string wid2 = nticketco.Substring(0, 5);
                //string wid3 = horaformatnews.Substring(13, 2);
                //string wid4 = nticketco.Substring(5, 4);
                //string wid5 = horaformatnews.Substring(16, 2);

                //string webidnwe = string.Concat(wid + wid2 + wid3 + wid4 + wid5);
                #endregion


                #endregion


                #region cliente
                GetCustomerRequest resquestcustomer = new GetCustomerRequest
                {
                    Id = request.NoCliente,
                    Identity = bsObj.Identity
                };



                //InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
                GetCustomerResponse responsecustomer = await invokeHubbleWebAPIServices.GetCustomer(resquestcustomer);
                string rfccliente = "";
                string razoonsocial = "";
                if (responsecustomer.Customer != null)
                {
                    rfccliente = responsecustomer.Customer.TIN;
                    razoonsocial = responsecustomer.Customer.BusinessName;

                }
                if (responsecustomer.Customer == null)
                {
                    rfccliente = null;
                    razoonsocial = null;

                }

                //if (responsecustomer.Customer.BusinessName == null && responsecustomer.Customer.TIN == null)
                //{
                //    isFacturar = false;
                //    salida.Resultado = false;
                //    salida.Msj = "NO SE PUDO ENCONTRAR LA INFORMACION DEL CLIENTE";
                //    return salida;
                //}

                #endregion


                if (request.TipoOperacion == 1)
                {
                    Log("CODE_FACTURACION: ","ENTRO EN TIPO DE OPERACION = 1");
                    if (responsegetdocument.Document == null && responsecustomer.Customer == null)
                    {
                        Log("CODEVOL_FIN WARNING 45", "@SHELLMX- TICKET O NUMERO DE CLIENTE NO VALIDO CON TIPO DE OPERACION 1. IDSEGUIMIENTO: " + criptoElecB);
                        salida.Msj = "NUMERO DE CLIENTE O TICKET NO VALIDO.";
                        salida.Resultado = false;
                        return salida;
                    }
                    if (responsegetdocument.Document == null && responsecustomer.Customer != null)
                    {
                        Log("CODEVOL_FIN WARNING 44", "@SHELLMX- NUMERO DE TICKET NO VALIDO TIPO DE OPERACION 1. IDSEGUIMIENTO: " + criptoElecB);
                        salida.Msj = "NUMERO DE TICKET NO VALIDO.";
                        salida.Resultado = false;
                        return salida;
                    }
                    if (responsegetdocument.Document != null && responsecustomer.Customer == null)
                    {
                        //---se agrego para facturar nombre publico en genral---

                        //---termina facturacion a nombre de publico en genral---
                        isFacturar = false;
                        //isFacturar_publicogenralEnero = true;
                        //isFacturar_publicogenral = true;
                        salida.Resultado = true;
                        salida.Msj = "OPERACION REALIZADA CON EXITO";
                    }
                    if (responsegetdocument.Document != null && responsecustomer.Customer != null)
                    {
                        salida.RazonSocial = textosincarspecial.transformtext(razoonsocial);
                        salida.RFC = rfccliente;
                        isFacturar = false;
                        salida.Resultado = true;
                        salida.Msj = "OPERACION REALIZADA CON EXITO";
                        // isFacturar_publicogenralEnero = true;
                        //isFacturar_publicogenral = true;
                        //salida.Resultado = true;
                        //salida.Msj = "OPERACION REALIZADA CON EXITO";
                    }


                }

                //if (request.TipoOperacion == 1 && responsecustomer.Customer == null || responsegetdocument.Document == null)
                //{
                //    isFacturar = false;

                //    salida.Resultado = false;
                //    salida.Msj = "El cliente y el ticket no son validos";
                //    return salida;
                //}

                if (request.TipoOperacion == 2)
                {
                    Log("CODE_FACTURACION: ", "ENTRO EN TIPO DE OPERACION = 2");
                    if (responsegetdocument.Document == null && responsecustomer.Customer == null)
                    {
                        Log("CODEVOL_FIN 43", "@SHELLMX- NUMERO DE TICKET O CLIENTE NO VALIDO TIPO DE OPERACION 2. IDSEGUIMIENTO: " + criptoElecB);
                        salida.Msj = "NUMERO DE CLIENTE Y TICKET NO VALIDO.";
                        salida.Resultado = false;
                        return salida;
                    }
                    if (responsegetdocument.Document == null && responsecustomer.Customer != null)
                    {
                        Log("CODEVOL_FIN 42", "@SHELLMX- NUMERO DE TICKET NO VALIDO TIPO DE OPERACION 2. IDSEGUIMIENTO: " + criptoElecB);
                        salida.Msj = "NUMERO DE TICKET NO VALIDO.";
                        salida.Resultado = false;
                        return salida;
                    }
                    //if ((responsegetdocument.Document != null && request.NoCliente == null )||
                    //    (responsegetdocument.Document != null && request.NoCliente.Trim() == "" )||
                    //    (responsegetdocument.Document != null && request.NoCliente == String.Empty)
                    //   )
                    if (responsegetdocument.Document != null && numeroclientere == null)
                    {
                        isFacturar = false;
                        //isFacturar_publicogenralEnero = true;
                        salida.Resultado = true;
                        salida.Msj = "NUMERO DE CLIENTE NO VALIDO.";

                    }
                    if (responsegetdocument.Document != null && responsecustomer.Customer == null && numeroclientere != null)
                    {
                        Log("CODEVOL_FIN 41", "@SHELLMX- NUMERO DE CLIENTE NO VALIDO TIPO DE OPERACION 2. IDSEGUIMIENTO: " + criptoElecB);
                        isFacturar = false;
                        salida.Resultado = true;
                        salida.Msj = "NUMERO DE CLIENTE NO VALIDO.";
                        //return salida;
                    }
                    if (responsegetdocument.Document != null && responsecustomer.Customer != null)
                    {
                        salida.RazonSocial = textosincarspecial.transformtext(razoonsocial);
                        salida.RFC = rfccliente;
                        isFacturar = true;
                        //salida.Resultado = true;
                        //salida.Msj = "OPERACION REALIZADA CON EXITO";
                    }



                    //if (responsecustomer.Customer == null)
                    //{

                    //    isFacturar = false;
                    //    rfccliente = null;
                    //    razoonsocial = null;
                    //    salida.Msj = "OPERACION REALIZADA CON ÉXITO";
                    //    salida.Resultado = true;


                    //}
                    //if (responsecustomer.Customer != null)
                    //{
                    //    isFacturar = true;
                    //    salida.RazonSocial = textosincarspecial.transformtext(razoonsocial);
                    //    salida.RFC = rfccliente;
                    //}

                }

                #region information
                //// GetPOSInformationResponse  GetPOSInformation(GetPosInformationRequest getPosInformationRequest
                //GetPosInformationRequest getPosInformationRequest = new GetPosInformationRequest
                //{
                //    Identity = bsObj.Identity
                //};

                //   //   InvokeHubbleWebAPIServices invokeHubbleWebAPIServices3 = new InvokeHubbleWebAPIServices();
                //InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
                GetPOSInformationResponse informationresponses = await invokeHubbleWebAPIServices.GetPOSInformation(getPosInformationRequest);

                #endregion

                if (isFacturar)
                {

                    GetCompanyRequest GetCompanyreques = new GetCompanyRequest
                    {
                        Identity = bsObj.Identity
                    };

                    //InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
                    GetCompanyResponse responseCompany = await invokeHubbleWebAPIServices.GetCompany(GetCompanyreques);



                    #region facturacion
                    //se nesesitan estos datos para facturar agregamos

                    var res = new ListTicketDAO
                    {
                        EESS = request.EESS,
                        NTicket = request.Nticket,
                        //RFC = "AAA010101AAA",
                        RFC = rfccliente,
                        WebID = request.WebID
                    };

                    //despues de crear la lista agregamos los siguientes campos para finalizar el reques de consumo en facturacion 

                    GenerateElectronicInvoice requestfac = new GenerateElectronicInvoice
                    {
                        EmpresaPortal = "01",
                        Company = responseCompany.Company.Id,
                        ListTicket = new List<ListTicketDAO>
                    {
                        new ListTicketDAO
                        {
                            EESS =res.EESS,
                            NTicket=res.NTicket,
                            RFC=res.RFC,
                            WebID=res.WebID
                        }
                    }
                    };



                    facresponse responsefacturacion = await invokeHubbleWebAPIServices.tpvfacturacionn(requestfac);



                    if (responsefacturacion.mensaje == "DATOS DEL TICKET NO VALIDOS PARA FACTURAR")
                    {
                        Log("CODEVOL_FIN 40", "@SHELLMX- DATOS NO VALIDOS PARA FACTURAR : " + responsefacturacion.mensaje + " IDSEGUIMIENTO: " + criptoElecB);
                        //salida.Ticket = request.Nticket;
                        //salida.FormaPago = metodopago;//"EFECTIVO"; //(responsegetdocument.Document.PaymentDetailList[0].PaymentMethodId);//pendiente por modificar
                        //salida.Subtotal = (Math.Truncate(responsegetdocument.Document.TaxableAmount * 100) / 100).ToString("N2");
                        //salida.Terminal = responsegetdocument.Document.PosId;
                        //salida.Operador = responsegetdocument.Document.OperatorName;
                        //salida.Folio = Folioidticket;
                        //salida.Total = (Math.Truncate(responsegetdocument.Document.TotalAmountWithTax * 100) / 100).ToString("N2");
                        //salida.ImporteEnLetra = letraconvert;
                        //salida.iva = salidaiva;
                        //salida.ivaMonto = salidaivamonto;
                        //salida.productos = listan;
                        //salida.Fecha = fechaticket;
                        //salida.WebID = webidnwe;
                        //salida.Estacion = informationresponses.PosInformation.ShopCode;
                        salida.Msj = responsefacturacion.mensaje;
                        salida.Resultado = false;
                        //return salida;
                    }
                    if (responsefacturacion.mensaje == "DATOS DEL TICKET INCORRECTO PARA FACTURAR")
                    {
                        Log("CODEVOL_FIN 39", "@SHELLMX- DATOS INCORRECTOS PARA FACTURAR : " + responsefacturacion.mensaje + " IDSEGUIMIENTO: " + criptoElecB);
                        //salida.Ticket = request.Nticket;
                        //salida.FormaPago = metodopago;//"EFECTIVO"; //(responsegetdocument.Document.PaymentDetailList[0].PaymentMethodId);//pendiente por modificar
                        //salida.Subtotal = (Math.Truncate(responsegetdocument.Document.TaxableAmount * 100) / 100).ToString("N2");
                        //salida.Terminal = responsegetdocument.Document.PosId;
                        //salida.Operador = responsegetdocument.Document.OperatorName;
                        //salida.Folio = Folioidticket;
                        //salida.Total = (Math.Truncate(responsegetdocument.Document.TotalAmountWithTax * 100) / 100).ToString("N2");
                        //salida.ImporteEnLetra = letraconvert;
                        //salida.iva = salidaiva;
                        //salida.ivaMonto = salidaivamonto;
                        //salida.productos = listan;
                        //salida.Fecha = fechaticket;
                        //salida.WebID = webidnwe;
                        //salida.Estacion = informationresponses.PosInformation.ShopCode;
                        salida.Msj = responsefacturacion.mensaje;
                        salida.Resultado = false;
                        //return salida;
                    }
                    if (responsefacturacion.mensaje == "NO SE PUDO ENCONTRAR EL SERVICIO DE FACTURACION")
                    {
                        Log("CODEVOL_FIN 38", "@SHELLMX- NO SE ENCONTRO EL SERVICIO DE FACTURACION : " + responsefacturacion.mensaje + " IDSEGUIMIENTO: " + criptoElecB);
                        salida.Msj = responsefacturacion.mensaje;
                        salida.Resultado = true;
                        //return salida;
                    }
                    if (responsefacturacion.mensaje == "FACTURACION CORRECTA")
                    {
                        Log("CODEVOL_TR **", "FACTURACION CORRECTA. " + "IDSEGUIMIENTO: " + criptoElecB);
                        salida.SelloDigitaSAT = responsefacturacion.SelloDigitaSAT;
                        salida.SelloDigitaCFDI = responsefacturacion.SelloDigitaCFDI;
                        salida.CadenaOrigTimbre = responsefacturacion.CadenaOrigTimbre;
                        salida.FolioFiscal = responsefacturacion.FolioFiscal;
                        salida.RFCProveedorCert = responsefacturacion.RFCProveedorCert;
                        salida.NumCertificado = responsefacturacion.NumCertificado;
                        salida.DateCertificacion = responsefacturacion.DateCertificacion;
                        salida.Msj = responsefacturacion.mensaje;
                        salida.Representacioncfdi = "Este documento es una representacion impresa de un CFDI";
                        salida.Resultado = true;
                    }

                    if (responsefacturacion.mensaje == "ERROR DE TIMBRADO AL FACTURAR")
                    {
                        Log("CODEVOL_FIN 37", "@SHELLMX- ERROR DE TIMBRADO DE FACTURACION : " + responsefacturacion.mensaje + " IDSEGUIMIENTO: " + criptoElecB);
                        salida.Msj = responsefacturacion.mensaje;
                        salida.Resultado = true;
                        //return salida;
                    }

                    if (responsefacturacion.mensaje == "NO SE PUDO ENCONTRAR EL SERVICIO DE FACTURACION")
                    {
                        Log("CODEVOL_FIN 36", "@SHELLMX- NO SE ENCONTRO EL METODO DE FACTURACION : " + responsefacturacion.mensaje + " IDSEGUIMIENTO: " + criptoElecB);
                        salida.Msj = "NO SE PUDO FACTURAR  INTENTELO MAS TARDE";
                        salida.Resultado = true;
                        //return salida;

                    }
                    //else
                    //{


                    //    salida.Msj = "NO SE PUDO FACTURAR";
                    //    salida.Resultado = false;

                    //}
                    #endregion




                }
                Log("CODEVOL_TR **", "EL RESPONSE QUE SE ENTREGA DE ELECTRONIC_BILLING IDSEGUIMIENTO: " +criptoElecB + "\n RESPONSE " + "\n" + "{" + "\n" +
                    "    ticket: " + request.Nticket + "," + "\n" +
                    "    terminal: " + responsegetdocument.Document.PosId.ToString() + "," + "\n" +
                    "    operador: " + responsegetdocument.Document.OperatorName + "," + "\n" +
                    "    folio: " + Folioidticket + "," + "\n" +
                    "    total: " + (Math.Truncate(responsegetdocument.Document.TotalAmountWithTax * 100) / 100).ToString("N2") + "," + "\n" +
                    "    importeletra: " + letraconvert + "," + "\n" +
                    "    iva: " + salidaiva + "," + "\n" +
                    "    ivamonto: " + salidaivamonto + "," + "\n" +
                    "    fecha: " + fechaticket + "," + "\n" +
                    "    webid: " + webidnwe + "," + "\n" +
                    "    estacion: " + informationresponses.PosInformation.ShopCode + "," + "\n" +
                    "    header1: " + textosincarspecial.transformtext(deserializeJsonheader.Header1) + "," + "\n" +
                    "    header2: " + textosincarspecial.transformtext(deserializeJsonheader.Header2) + "," + "\n" +
                    "    header3: " + deserializeJsonheader.Header3 + "," + "\n" +
                    "    header4: " + textosincarspecial.transformtext(deserializeJsonheader.Header4) + "," + "\n" +
                    "    footer1: " + textosincarspecial.transformtext(deserializeJsonfooter.Footer1) + "," + "\n" +
                    "    footer2: " + textosincarspecial.transformtext(deserializeJsonfooter.Footer2) + "," + "\n" +
                    "    footer3: " + textosincarspecial.transformtext(deserializeJsonfooter.Footer3) + "," + "\n" +
                    "    footer4: " + textosincarspecial.transformtext(deserializeJsonfooter.Footer4) + "," + "\n" +
                    "    footer5 " + deserializeJsonfooter.Footer5 + "," + "\n" +
                    "    codigopostalcompany: " + textosincarspecial.transformtext(listaPrinting[17]) + "," + "\n" +
                    "    codigopostaltienda: " + textosincarspecial.transformtext(listaPrinting[27]) + "," + "\n" +
                    "    coloniacompany: " + textosincarspecial.transformtext(listaPrinting[16]) + "," + "\n" +
                    "    coloniatienda: " + textosincarspecial.transformtext(listaPrinting[29]) + "," + "\n" +
                    "    estadocompany: " + textosincarspecial.transformtext(listaPrinting[19]) + "," + "\n" +
                    "    estadotienda: " + textosincarspecial.transformtext(listaPrinting[31]) + "," + "\n" +
                    "    expediciontienda: " + textosincarspecial.transformtext(listaPrinting[27]) + "," + "\n" +
                    "    municipioCompany: " + textosincarspecial.transformtext(listaPrinting[16]) + "," + "\n" +
                    "    municipiotienda: " + textosincarspecial.transformtext(listaPrinting[26]) + "," + "\n" +
                    "    nombreCompania: " + textosincarspecial.transformtext(listaPrinting[15]) + "," + "\n" +
                    "    paisCompania: " + textosincarspecial.transformtext(listaPrinting[20]) + "," + "\n" +
                    "    paisTienda: " + textosincarspecial.transformtext(listaPrinting[30]) + "," + "\n" +
                    "    permisoCRE: " + listaPrinting[32] + "," + "\n" +
                    "    tienda: " + textosincarspecial.transformtext(listaPrinting[25]) + "," + "\n" +
                    "    regFiscal: " + "REGIMEN GENERAL DE LEY PERSONAS MORALES" + "," + "\n" +
                    "    rfcCompania: " + textosincarspecial.transformtext(listaPrinting[5]) + "\n" + "}");

                Log("CODEVOL_FIN 1", "SE TERMINA EL METODO DE ELETRONIC_BILLING SATISFACTORIAMENTE. IDSEGUIMIENTO: " + criptoElecB);
                salida.Ticket = request.Nticket;
                salida.FormaPago = pagosimpreso;//"EFECTIVO"; //(responsegetdocument.Document.PaymentDetailList[0].PaymentMethodId);//pendiente por modificar
                                                //salida.Subtotal = (Math.Truncate(responsegetdocument.Document.TaxableAmount * 100) / 100).ToString("N2");
                salida.Terminal = responsegetdocument.Document.PosId;
                salida.Operador = responsegetdocument.Document.OperatorName;
                salida.Folio = Folioidticket;
                salida.Total = (Math.Truncate(responsegetdocument.Document.TotalAmountWithTax * 100) / 100).ToString("N2");
                salida.ImporteEnLetra = letraconvert;
                salida.iva = salidaiva;
                salida.ivaMonto = salidaivamonto;
                salida.productos = listan;
                salida.Fecha = fechaticket;
                salida.WebID = webidnwe;
                salida.Estacion = informationresponses.PosInformation.ShopCode;
                salida.HeaderTick1 = textosincarspecial.transformtext(deserializeJsonheader.Header1);
                salida.HeaderTick2 = textosincarspecial.transformtext(deserializeJsonheader.Header2);
                salida.HeaderTick3 = deserializeJsonheader.Header3;
                salida.HedaerTick4 = textosincarspecial.transformtext(deserializeJsonheader.Header4);
                salida.FooterTick1 = textosincarspecial.transformtext(deserializeJsonfooter.Footer1);
                salida.FooterTick2 = textosincarspecial.transformtext(deserializeJsonfooter.Footer2);
                salida.FooterTick3 = textosincarspecial.transformtext(deserializeJsonfooter.Footer3);
                salida.FooterTick4 = textosincarspecial.transformtext(deserializeJsonfooter.Footer4);
                salida.FooterTick5 = deserializeJsonfooter.Footer5;
                salida.CodigoPostalCompania = textosincarspecial.transformtext(listaPrinting[17]);
                salida.CodigoPostalTienda = textosincarspecial.transformtext(listaPrinting[27]);
                salida.ColoniaCompania = textosincarspecial.transformtext(listaPrinting[16]);
                salida.ColoniaTienda = textosincarspecial.transformtext(listaPrinting[29]);
                salida.DireccionCompania = textosincarspecial.transformtext(listaPrinting[14]);
                salida.DireccionTienda = textosincarspecial.transformtext(listaPrinting[24]);
                salida.EstadoCompania = textosincarspecial.transformtext(listaPrinting[19]);
                salida.EstadoTienda = textosincarspecial.transformtext(listaPrinting[31]);
                salida.ExpedicionTienda = textosincarspecial.transformtext(listaPrinting[27]);
                salida.MunicipioCompania = textosincarspecial.transformtext(listaPrinting[16]);
                salida.MunicipioTienda = textosincarspecial.transformtext(listaPrinting[26]);
                salida.NombreCompania = textosincarspecial.transformtext(listaPrinting[15]);
                salida.PaisCompania = textosincarspecial.transformtext(listaPrinting[20]);
                salida.PaisTienda = textosincarspecial.transformtext(listaPrinting[30]);
                salida.PermisoCRE = listaPrinting[32];
                salida.Tienda = textosincarspecial.transformtext(listaPrinting[25]);
                salida.RegFiscal = "REGIMEN GENERAL DE LEY PERSONAS MORALES";
                salida.RfcCompania = textosincarspecial.transformtext(listaPrinting[5]);
            }
            catch(Exception e)
            {
                salida.Msj = "HUBO UN ERRRO INTERNO AL OBTENER EL TICKET FACTURA.";
                salida.Resultado = false;
                Log("CODEVOL_FIN 26", "@SHELLMX- HUBO UN ERRRO AL ENTRAR AL METODO DE ELECTRONIC_BILLING IDSEGUIMIENTO: "+ criptoElecB + "LOG:: " + e.ToString());
            }
            /*finally  // <--  termine bien o mal liberamos el semaforo
            {
                try
                {
                    SemaphoreSlimElectronic_billing_FP?.Release();
                }
                catch (Exception)
                {
                }
            }*/
            return salida;
        }

        // SLMX -  Obtener correspondiente medio pago de BOS para el valor de PC
        private string getPaymentTypeByIdPC(int idPC)
        {
            string ptBOS = "";

            switch (idPC)
            {
                case (int)PaymentMethodPC.AMERICAN_EXPRESS : { ptBOS = "03"; break; }
                case (int)PaymentMethodPC.MORRALLA: { ptBOS = "01"; break; }
                case (int)PaymentMethodPC.EFECTIVO: { ptBOS = "01"; break; }
                case (int)PaymentMethodPC.VALE_INBURSA: { ptBOS = "07"; break; }
                case (int)PaymentMethodPC.SERVIBONOS: { ptBOS = "07"; break; }
                case (int)PaymentMethodPC.VALES_GASOCHECK: { ptBOS = "07"; break; }
                case (int)PaymentMethodPC.VALES_ACCOR: { ptBOS = "07"; break; }
                case (int)PaymentMethodPC.VALES_EFECTIVALE: { ptBOS = "07"; break; }
                case (int)PaymentMethodPC.HIDROVALE: { ptBOS = "07"; break; }
                case (int)PaymentMethodPC.CHEQUE_TARJETA_DESECHABLE: { ptBOS = "07"; break; }
                case (int)PaymentMethodPC.TICKET_CAR_ACCOR: { ptBOS = "07"; break; }
                case (int)PaymentMethodPC.SODEXHOPASS: { ptBOS = "07";  break; }
                case (int)PaymentMethodPC.PUNTO_CLAVE: { ptBOS = "07"; break; }
                case (int)PaymentMethodPC.EFECTICAR: { ptBOS = "07"; break; }
                case (int)PaymentMethodPC.TARJETA_BANCARIA: { ptBOS = "08"; break; }
                case (int)PaymentMethodPC.MERCADO_PAGO: { ptBOS = "07"; break; }
                case (int)PaymentMethodPC.PUNTOS: { ptBOS = "07"; break; }                
                case (int)PaymentMethodPC.TARJETA_INTELIGENTE: { ptBOS = "07"; break; }
                default: { ptBOS = ""; break; } 
                
            }

            return ptBOS;
        }
        
    }
        
}
