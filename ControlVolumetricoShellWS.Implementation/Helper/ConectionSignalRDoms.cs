using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Client;
using Conection.HubbleWS;
using System.Threading.Tasks;
using MX_LogsHPTPV;
using Conection.HubbleWS.Models.Hubble;

namespace ControlVolumetricoShellWS.Implementation
{
    public class ConectionSignalRDoms
    {
        public string serverAddress = "http://localhost:8092/signalr";
        public event Action OnConnected;
        public event Action OnDisconnected;
        public short Status = 0;

        public ConectionSignalRDoms()
        {
            ConnectToServer();
        }

        private IHubProxy hubProxy;
        private HubConnection hubConnection;

        public void ConnectToServer()
        {
            hubConnection = new HubConnection(serverAddress);
            hubProxy = hubConnection.CreateHubProxy("pssHub");
            bool isConnected = false;
            StatusConectionHubble(0);

            while (!isConnected)
            {
                try
                {
                    hubConnection.Start().Wait();
                    Console.WriteLine("Comienza con la Conexion pssHub");
                    hubConnection.Closed += OnHubConnectionClosed;

                    isConnected = true;
                    OnConnected?.Invoke();
                    StatusConectionHubble(1);
                }
                catch(Exception e)
                {
                    StatusConectionHubble(-1);
                    Console.WriteLine(e);
                    return;
                }
            }
        }

        private void StatusConectionHubble(short statutus)
        {
            Status = statutus;
        }
        public short StatusConectionHubbleR()
        {
            return Status;
        }

        private void OnHubConnectionClosed()
        {
            //LogInfo("Disconnected from server");
            OnDisconnected?.Invoke();
        }

        public bool IsConnected()
        {
            return hubConnection != null && hubConnection.State == ConnectionState.Connected;
        }

        public void Disconnect()
        {
            hubConnection.Stop();
            Console.WriteLine("Disconnected from server");
            OnDisconnected?.Invoke();
        }

        public void CleanObject()
        {

        }

        public GetAllSupplyTransactionsOfFuellingPointResponse GetAllSupplyTransactionsOfFuellingPoint(GetAllSupplyTransactionsOfFuellingPointRequest getAllSupplyTransactionsOfFuellingPointRequest, string idSeguimiento , bool isjarreo = false)
        {
            LogsTPVHP exec = new LogsTPVHP();
            try
            {
                // Se pregunta si esta conecta
                if (!IsConnected())
                {
                    ConnectToServer();
                }

                //string lockSupplyTransaction = "";
                GetAllSupplyTransactionsOfFuellingPointRequest request = new GetAllSupplyTransactionsOfFuellingPointRequest() { OperatorId = getAllSupplyTransactionsOfFuellingPointRequest.OperatorId, FuellingPointId = getAllSupplyTransactionsOfFuellingPointRequest.FuellingPointId };

                // SHELLMX- Se invoca al cliente de SignalR para el consumo del PSS, que se necesita para las transaccion que se tiene en el surtidor seleccinado.
                //        donde se entrega un objeto de la transaccion que esta habilitado en un surtudor. 

                GetAllSupplyTransactionsOfFuellingPointResponse supplyTransactionOfFuellingPoint;
                supplyTransactionOfFuellingPoint = hubProxy.Invoke<GetAllSupplyTransactionsOfFuellingPointResponse>("GetAllSupplyTransactionsOfFuellingPoint", request).Result;

                //SHELLMX- Si no entrega ninguna informacion.
                if (supplyTransactionOfFuellingPoint.Status <= -4 || supplyTransactionOfFuellingPoint.Status == -1)
                {
                    exec.GeneraLogInfo("CODEVOL_TR ERROR", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + supplyTransactionOfFuellingPoint.Message);
                    return new GetAllSupplyTransactionsOfFuellingPointResponse { Message = "LA BOMBA : " + request.FuellingPointId + " INSERTADA NO EXISTE O NO HAY CARGA. VERIFICAR LOGS", Status = supplyTransactionOfFuellingPoint.Status };
                }
                if (supplyTransactionOfFuellingPoint.Status == -3)
                {
                    exec.GeneraLogInfo("CODEVOL_TR WARNING", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + supplyTransactionOfFuellingPoint.Message);
                    return new GetAllSupplyTransactionsOfFuellingPointResponse { Message = "LA BOMBA: " + request.FuellingPointId + " NO EXISTE.", Status = supplyTransactionOfFuellingPoint.Status };
                }
                if (supplyTransactionOfFuellingPoint.Status == -2)
                {
                    exec.GeneraLogInfo("CODEVOL_TR ERROR", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + supplyTransactionOfFuellingPoint.Message);
                    return new GetAllSupplyTransactionsOfFuellingPointResponse { Message = "VERIFICAR LA COMUNICACION DEL DOMS CON EL TPV. VERIFICAR LOGS", Status = supplyTransactionOfFuellingPoint.Status };
                }

                int countAuxSupplyTransactionsOfFuellingPoint = 0;
                foreach (SupplyTransaction supplyTransaction in supplyTransactionOfFuellingPoint.SupplyTransactionList) { countAuxSupplyTransactionsOfFuellingPoint++; }
                if (countAuxSupplyTransactionsOfFuellingPoint == 0)
                {
                    exec.GeneraLogInfo("CODEVOL_TR WARNING", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + " NO SE ENCONTRO NINGUNA RECARGA EN EL SURTIDOR.");
                    return new GetAllSupplyTransactionsOfFuellingPointResponse { Message = "LA BOMBA : " + request.FuellingPointId + " NO TIENE NINGUNA RACARGA ESTA <VACIA>.", Status = 0 };
                }

                if(!isjarreo)
                {
                    int? lockSupply = null;
                    foreach (SupplyTransaction supplyTransaction in supplyTransactionOfFuellingPoint.SupplyTransactionList)
                    {
                        lockSupply = supplyTransaction.LockingPOSId;
                    }
                    if (lockSupply != null)
                    {
                        exec.GeneraLogInfo("CODEVOL_TR WARNING", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + " TRANSACCION BLOQUEADA POR OTRA TERMINAL, VERIFICAR.");
                        return new GetAllSupplyTransactionsOfFuellingPointResponse { Message = "TRANSACCION BLOQUEADA POR OTRA TERMINAL, VERIFICAR.", Status = -1 };
                    }
                }

                return supplyTransactionOfFuellingPoint;
            }
            catch(Exception e)
            {
                exec.GeneraLogInfo("CODEVOL_FIN ERROR" , "@SHELLMX- SE OBTUBO UN ERROR INTERNO AL OBTENER LA INFOMACION DEL SURTIDOR CON EL SIGUIENTE ERROR LOG: " + e.ToString());
                return new GetAllSupplyTransactionsOfFuellingPointResponse { Message = "SE OBTUVO UN ERROR INTERNO EN LOS SUMINISTROS CON EL SISGUIENTE LOG: " + e.ToString() , Status = -5 };
            }
        }

        public async Task<LockSupplyTransactionOfFuellingPointResponse> LockSupplyTransactionOfFuellingPoint(string identity, GetAllSupplyTransactionsOfFuellingPointRequest getAllSupplyTransactionsOfFuellingPointRequest, string idSeguimiento)
        {
            LogsTPVHP exec = new LogsTPVHP();
            try
            {
                // SHELLMX- Se manda a consumir el metodo para traer la informacion del Operador-
                // 1- Para el Proceso se debe traer en primera instancia el operador en turno.
                // 2- Debe de estar almacenado en una proceso o proyecto. 

                // Se pregunta si esta conecta
                if (!IsConnected())
                {
                    ConnectToServer();
                }

                //string lockSupplyTransaction = "";
                GetAllSupplyTransactionsOfFuellingPointRequest request = new GetAllSupplyTransactionsOfFuellingPointRequest() { OperatorId = getAllSupplyTransactionsOfFuellingPointRequest.OperatorId, FuellingPointId = getAllSupplyTransactionsOfFuellingPointRequest.FuellingPointId };

                // SHELLMX- Se invoca al cliente de SignalR para el consumo del PSS, que se necesita para las transaccion que se tiene en el surtidor seleccinado.
                //        donde se entrega un objeto de la transaccion que esta habilitado en un surtudor. 

                GetAllSupplyTransactionsOfFuellingPointResponse supplyTransactionOfFuellingPoint;
                supplyTransactionOfFuellingPoint = hubProxy.Invoke<GetAllSupplyTransactionsOfFuellingPointResponse>("GetAllSupplyTransactionsOfFuellingPoint", request).Result;

                //SHELLMX- Si no entrega ninguna informacion.
                if (supplyTransactionOfFuellingPoint.Status <= -4 || supplyTransactionOfFuellingPoint.Status == -1)
                {
                    exec.GeneraLogInfo("CODEVOL_TR ERROR" , "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + supplyTransactionOfFuellingPoint.Message);
                    return new LockSupplyTransactionOfFuellingPointResponse { Message = "LA BOMBA : " + request.FuellingPointId + " INSERTADA NO EXISTE O NO HAY CARGA. VERIFICAR LOGS", Status = supplyTransactionOfFuellingPoint.Status };
                }
                if (supplyTransactionOfFuellingPoint.Status == -3)
                {
                    exec.GeneraLogInfo("CODEVOL_TR WARNING", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + supplyTransactionOfFuellingPoint.Message);
                    return new LockSupplyTransactionOfFuellingPointResponse { Message = "LA BOMBA: " + request.FuellingPointId + " NO EXISTE.", Status = supplyTransactionOfFuellingPoint.Status };
                }
                if (supplyTransactionOfFuellingPoint.Status == -2)
                {
                    exec.GeneraLogInfo("CODEVOL_TR ERROR", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + supplyTransactionOfFuellingPoint.Message);
                    return new LockSupplyTransactionOfFuellingPointResponse { Message = "VERIFICAR LA COMUNICACION DEL DOMS CON EL TPV. VERIFICAR LOGS", Status = supplyTransactionOfFuellingPoint.Status };
                }

                int countAuxSupplyTransactionsOfFuellingPoint = 0;
                foreach (SupplyTransaction supplyTransaction in supplyTransactionOfFuellingPoint.SupplyTransactionList) { countAuxSupplyTransactionsOfFuellingPoint++; }
                if(countAuxSupplyTransactionsOfFuellingPoint == 0)
                {
                    exec.GeneraLogInfo("CODEVOL_TR WARNING", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + " NO SE ENCONTRO NINGUNA RECARGA EN EL SURTIDOR.");
                    return new LockSupplyTransactionOfFuellingPointResponse { Message = "LA BOMBA : " + request.FuellingPointId + " NO TIENE NINGUNA RACARGA ESTA <VACIA>.", Status = 0 };
                }

                int? lockSupply = null;
                foreach(SupplyTransaction supplyTransaction in supplyTransactionOfFuellingPoint.SupplyTransactionList)
                {
                    lockSupply = supplyTransaction.LockingPOSId;
                }
                if(lockSupply != null)
                {
                    exec.GeneraLogInfo("CODEVOL_TR WARNING", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + " TRANSACCION BLOQUEADA POR OTRA TERMINAL, VERIFICAR.");
                    return new LockSupplyTransactionOfFuellingPointResponse { Message = "TRANSACCION BLOQUEADA POR OTRA TERMINAL, VERIFICAR.", Status = -1 };
                }

                // SHELLMX- Se manda a traer la informacion sobre el Cliente Contado para este proceso.
                InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
                GetPOSConfigurationRequest getPOSConfigurationRequest = new GetPOSConfigurationRequest { Identity = identity };
                GetPOSConfigurationResponse getPOSConfigurationResponse = await invokeHubbleWebAPIServices.GetPOSConfiguration(getPOSConfigurationRequest);

                // SHELLMX- Al momento de traer la informacion sobre la transaccion que hay en parte sobre un surtidor, bloquea en el TVP que Action lo este usando, Se contruye el objeto
                //          a llenar de lock para traer la demas informacion sobre la transaccion del Surtidor seleccinado.

                LockSupplyTransactionOfFuellingPointRequest lockRequest = new LockSupplyTransactionOfFuellingPointRequest()
                {
                    CustomerId = getPOSConfigurationResponse.UnknownCustomerId,
                    FuellingPointId = request.FuellingPointId,
                    OperatorId = request.OperatorId
                };

                int grade = 0;
                decimal gradeUnitPrice = 0.0M;
                foreach (var supply in supplyTransactionOfFuellingPoint.SupplyTransactionList)
                {
                    lockRequest.SupplyTransactionId = supply.Id;
                    gradeUnitPrice = supply.GradeUnitPrice;
                    grade = supply.GradeId;
                }

                LockSupplyTransactionOfFuellingPointResponse lockSupplyTransactionOfFuellingPoint = hubProxy.Invoke<LockSupplyTransactionOfFuellingPointResponse>("LockSupplyTransactionOfFuellingPoint", lockRequest).Result;

                if (lockSupplyTransactionOfFuellingPoint.Status < 0)
                {
                    exec.GeneraLogInfo("CODEVOL_TR ERROR", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + lockSupplyTransactionOfFuellingPoint.Message);
                    return new LockSupplyTransactionOfFuellingPointResponse { Message = "NO EXISTE UN ID PARA EL SURTIDOR DE LA RECARGA, VERIFICAR SI HAY UNA RECARGA ASOCIADA.", Status = supplyTransactionOfFuellingPoint.Status };
                }

                lockSupplyTransactionOfFuellingPoint.Id = lockRequest.SupplyTransactionId;
                lockSupplyTransactionOfFuellingPoint.GradeUnitPrice = gradeUnitPrice;
                lockSupplyTransactionOfFuellingPoint.GradeId = grade;

                // SHELLMX- Se invoca al cliente de SignalR para el consumo del PSS, que se necesita para las transaccion que se tiene en el surtidor seleccinado.
                //        donde se entrega un objeto de la transaccion que esta habilitado en un surtudor. 

                GetAllSupplyTransactionsOfFuellingPointResponse supplyTransactionOfFuellingPoint1;
                supplyTransactionOfFuellingPoint1 = hubProxy.Invoke<GetAllSupplyTransactionsOfFuellingPointResponse>("GetAllSupplyTransactionsOfFuellingPoint", request).Result;

                foreach (var supply1 in supplyTransactionOfFuellingPoint1.SupplyTransactionList)
                {
                    lockSupplyTransactionOfFuellingPoint.posID = Convert.ToInt32(supply1.LockingPOSId);
                }

                return lockSupplyTransactionOfFuellingPoint;
            }
            catch (Exception e)
            {
                exec.GeneraLogInfo("CODEVOL_TR ERROR" , "@SHELLMX- ERROR INTERNO EN LA CONFIGURACION AL OBTENER LA INFORMACION DEL SURTIDOR IDSEGUIMIENTO: " + idSeguimiento + " CON EL LOG: " + e.ToString());
                return new LockSupplyTransactionOfFuellingPointResponse { Message = "SE OBTIVO UN ERROR INTERNO AL OBTENER LA BOMBA REVISAR TPV.", Status = -5 };
                throw e;
                //OnConnectionFailed?.Invoke(e.Message);
            }
        }

        #region VALIDADOR NO THREADING PARA EL BLOQUEO DE BOMBA EN MODO VENTA.
        public async Task<LockSupplyTransactionOfFuellingPointResponse> LockSupplyTransactionOfFuellingPointToSale(string identity, GetAllSupplyTransactionsOfFuellingPointRequest getAllSupplyTransactionsOfFuellingPointRequest, string idSeguimiento)
        {
            LogsTPVHP exec = new LogsTPVHP();
            try
            {
                if (!IsConnected())
                {
                    ConnectToServer();
                }

                GetAllSupplyTransactionsOfFuellingPointResponse supplyTransactionOfFuellingPoint;
                supplyTransactionOfFuellingPoint = hubProxy.Invoke<GetAllSupplyTransactionsOfFuellingPointResponse>("GetAllSupplyTransactionsOfFuellingPoint", getAllSupplyTransactionsOfFuellingPointRequest).Result;

                //SHELLMX- Si no entrega ninguna informacion.
                if (supplyTransactionOfFuellingPoint.Status <= -4 || supplyTransactionOfFuellingPoint.Status == -1)
                {
                    exec.GeneraLogInfo("CODEVOL_TR ERROR", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + supplyTransactionOfFuellingPoint.Message);
                    return new LockSupplyTransactionOfFuellingPointResponse { Message = "LA BOMBA : " + getAllSupplyTransactionsOfFuellingPointRequest.FuellingPointId + " INSERTADA NO EXISTE O NO HAY CARGA. VERIFICAR LOGS", Status = supplyTransactionOfFuellingPoint.Status };
                }
                if (supplyTransactionOfFuellingPoint.Status == -3)
                {
                    exec.GeneraLogInfo("CODEVOL_TR WARNING", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + supplyTransactionOfFuellingPoint.Message);
                    return new LockSupplyTransactionOfFuellingPointResponse { Message = "LA BOMBA: " + getAllSupplyTransactionsOfFuellingPointRequest.FuellingPointId + " NO EXISTE.", Status = supplyTransactionOfFuellingPoint.Status };
                }
                if (supplyTransactionOfFuellingPoint.Status == -2)
                {
                    exec.GeneraLogInfo("CODEVOL_TR ERROR", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + supplyTransactionOfFuellingPoint.Message);
                    return new LockSupplyTransactionOfFuellingPointResponse { Message = "VERIFICAR LA COMUNICACION DEL DOMS CON EL TPV. VERIFICAR LOGS", Status = supplyTransactionOfFuellingPoint.Status };
                }

                int countAuxSupplyTransactionsOfFuellingPoint = 0;
                foreach (SupplyTransaction supplyTransaction in supplyTransactionOfFuellingPoint.SupplyTransactionList) { countAuxSupplyTransactionsOfFuellingPoint++; }
                if (countAuxSupplyTransactionsOfFuellingPoint == 0)
                {
                    exec.GeneraLogInfo("CODEVOL_TR WARNING", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + " NO SE ENCONTRO NINGUNA RECARGA EN EL SURTIDOR.");
                    return new LockSupplyTransactionOfFuellingPointResponse { Message = "LA BOMBA : " + getAllSupplyTransactionsOfFuellingPointRequest.FuellingPointId + " NO TIENE NINGUNA RACARGA ESTA <VACIA>.", Status = 0 };
                }

                int? lockSupply = null;
                foreach (SupplyTransaction supplyTransaction in supplyTransactionOfFuellingPoint.SupplyTransactionList)
                {
                    lockSupply = supplyTransaction.LockingPOSId;
                }
                if (lockSupply != null)
                {
                    exec.GeneraLogInfo("CODEVOL_TR WARNING", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + " TRANSACCION BLOQUEADA POR OTRA TERMINAL, VERIFICAR.");
                    return new LockSupplyTransactionOfFuellingPointResponse { Message = "TRANSACCION BLOQUEADA POR OTRA TERMINAL, VERIFICAR.", Status = -1 };
                }

                // SHELLMX- Se manda a traer la informacion sobre el Cliente Contado para este proceso.
                InvokeHubbleWebAPIServices invokeHubbleWebAPIServices = new InvokeHubbleWebAPIServices();
                GetPOSConfigurationRequest getPOSConfigurationRequest = new GetPOSConfigurationRequest { Identity = identity };
                GetPOSConfigurationResponse getPOSConfigurationResponse = await invokeHubbleWebAPIServices.GetPOSConfiguration(getPOSConfigurationRequest);

                // SHELLMX- Al momento de traer la informacion sobre la transaccion que hay en parte sobre un surtidor, bloquea en el TVP que Action lo este usando, Se contruye el objeto
                //          a llenar de lock para traer la demas informacion sobre la transaccion del Surtidor seleccinado.

                LockSupplyTransactionOfFuellingPointRequest lockRequest = new LockSupplyTransactionOfFuellingPointRequest()
                {
                    CustomerId = getPOSConfigurationResponse.UnknownCustomerId,
                    FuellingPointId = getAllSupplyTransactionsOfFuellingPointRequest.FuellingPointId,
                    OperatorId = getAllSupplyTransactionsOfFuellingPointRequest.OperatorId
                };

                int grade = 0;
                decimal gradeUnitPrice = 0.0M;
                foreach (var supply in supplyTransactionOfFuellingPoint.SupplyTransactionList)
                {
                    lockRequest.SupplyTransactionId = supply.Id;
                    gradeUnitPrice = supply.GradeUnitPrice;
                    grade = supply.GradeId;
                }

                LockSupplyTransactionOfFuellingPointResponse lockSupplyTransactionOfFuellingPoint = hubProxy.Invoke<LockSupplyTransactionOfFuellingPointResponse>("LockSupplyTransactionOfFuellingPoint", lockRequest).Result;

                if (lockSupplyTransactionOfFuellingPoint.Status < 0)
                {
                    exec.GeneraLogInfo("CODEVOL_TR ERROR", "@SHELLMX- OCURRIO UN ERROR AL OBTENER EL SUMINISTRO. IDSEGUIMIENTO: " + idSeguimiento + "  LOG: " + lockSupplyTransactionOfFuellingPoint.Message);
                    return new LockSupplyTransactionOfFuellingPointResponse { Message = "NO EXISTE UN ID PARA EL SURTIDOR DE LA RECARGA, VERIFICAR SI HAY UNA RECARGA ASOCIADA.", Status = supplyTransactionOfFuellingPoint.Status };
                }

                lockSupplyTransactionOfFuellingPoint.Id = lockRequest.SupplyTransactionId;
                lockSupplyTransactionOfFuellingPoint.GradeUnitPrice = gradeUnitPrice;
                lockSupplyTransactionOfFuellingPoint.GradeId = grade;

                // SHELLMX- Se invoca al cliente de SignalR para el consumo del PSS, que se necesita para las transaccion que se tiene en el surtidor seleccinado.
                //        donde se entrega un objeto de la transaccion que esta habilitado en un surtudor. 

                GetAllSupplyTransactionsOfFuellingPointResponse supplyTransactionOfFuellingPoint1;
                supplyTransactionOfFuellingPoint1 = hubProxy.Invoke<GetAllSupplyTransactionsOfFuellingPointResponse>("GetAllSupplyTransactionsOfFuellingPoint", getAllSupplyTransactionsOfFuellingPointRequest).Result;

                foreach (var supply1 in supplyTransactionOfFuellingPoint1.SupplyTransactionList)
                {
                    lockSupplyTransactionOfFuellingPoint.posID = Convert.ToInt32(supply1.LockingPOSId);
                }

                return lockSupplyTransactionOfFuellingPoint;
            }
            catch (Exception e)
            {
                exec.GeneraLogInfo("CODEVOL_TR ERROR", "@SHELLMX- ERROR INTERNO EN LA CONFIGURACION AL OBTENER LA INFORMACION DEL SURTIDOR IDSEGUIMIENTO: " + idSeguimiento + " CON EL LOG: " + e.ToString());
                return new LockSupplyTransactionOfFuellingPointResponse { Message = "SE OBTIVO UN ERROR INTERNO AL OBTENER LA BOMBA REVISAR TPV.", Status = -5 };
                throw e;
                //OnConnectionFailed?.Invoke(e.Message);
            }
        }
        #endregion

        public async Task<int[]> ValidateSupplyTransactionOfFuellingPoint(string identity, GetAllSupplyTransactionsOfFuellingPointRequest getAllSupplyTransactionsOfFuellingPointRequest, string idSeguimiento, string istransaccion)
        {
            LogsTPVHP exec = new LogsTPVHP();
            try
            {
                // Se pregunta si esta conecta
                exec.GeneraLogInfo("CODEVOL_TR INFO", "@SHELLMX- ESTADO DE CONEXION POR PARTE DEL SIGNALR DEL WS CON LA DEL HUBBLEPOS: " + IsConnected() + "  --->ID_TRANSACCION_BOMBA: " + istransaccion + " IDSEGUIMIENTO: " + idSeguimiento);
                if (!IsConnected())
                {
                    ConnectToServer();
                }

                GetAllSupplyTransactionsOfFuellingPointRequest request = new GetAllSupplyTransactionsOfFuellingPointRequest() { OperatorId = getAllSupplyTransactionsOfFuellingPointRequest.OperatorId, FuellingPointId = getAllSupplyTransactionsOfFuellingPointRequest.FuellingPointId };

                GetAllSupplyTransactionsOfFuellingPointResponse supplyTransactionOfFuellingPoint;
                supplyTransactionOfFuellingPoint = hubProxy.Invoke<GetAllSupplyTransactionsOfFuellingPointResponse>("GetAllSupplyTransactionsOfFuellingPoint", request).Result;

                if(supplyTransactionOfFuellingPoint.Status < 0)
                {
                    return new int[] { supplyTransactionOfFuellingPoint.Status, supplyTransactionOfFuellingPoint.Status };
                }
                int countAuxSupply = 0;
                foreach(SupplyTransaction supplyTransaction in supplyTransactionOfFuellingPoint.SupplyTransactionList) { countAuxSupply++; }
                if(countAuxSupply == 0)
                {
                    return new int[] { 0, 0 };
                }

                int validateFuellingPoint = 0;
                string validateLockFuelling = null;
                foreach (var supplyValidate in supplyTransactionOfFuellingPoint.SupplyTransactionList)
                {
                    validateFuellingPoint = supplyValidate.Id;
                    validateLockFuelling = supplyValidate.LockingPOSId.ToString();
                    exec.GeneraLogInfo("CODEVOL_TR INFO", "LO QUE RESPONDE EL DOMS SOBRE EL SURTIDOR SELECCIONADO  --->ID_TRANSACCION_BOMBA: " + istransaccion + "  IDSEGUIMIENTO: " + idSeguimiento + "\n" + "GetAllSupplyTransactionsOfFuellingPointResponse: \n {" + "\n" + 
                        "    Id: " + supplyValidate.Id.ToString() + "," + "\n" + 
                        "    GradeUnitPrice: " + supplyValidate.GradeUnitPrice.ToString() + "," + "\n" +
                        "    LockingPOSID: " + supplyValidate.LockingPOSId.ToString() + "," + "\n" + 
                        "    GradeId: " + supplyValidate.GradeId.ToString() + "," + "\n" + 
                        "    GradeReference: " + supplyValidate.GradeReference.ToString() + "," + "\n" + 
                        "    Money: " + supplyValidate.Money.ToString() + "," + "\n" +
                        "    ServiceModeType: " + supplyValidate.ServiceModeType.ToString() + "," + "\n" + 
                        "    DateStart: " + supplyValidate.StartDateTime.ToString() + "," + "\n" +
                        "    Type: " + supplyValidate.Type.ToString() + "," + "\n" + 
                        "    Volume: " + supplyValidate.Volume.ToString() + "\n" + "}");
                }

                // SHLMX- Se hace la validacion, sobre si la bomba esta desbloqueada.
                if(validateLockFuelling == String.Empty)
                {
                    LockSupplyTransactionOfFuellingPointResponse lockSupplyTransactionOfFuellingPointResponse = await LockSupplyTransactionOfFuellingPointToSale(identity, getAllSupplyTransactionsOfFuellingPointRequest, idSeguimiento);
                    if(lockSupplyTransactionOfFuellingPointResponse.Status < 0)
                    {
                        exec.GeneraLogInfo("CODEVOL_WARNING", "@SHELLMX- EL PSSCONTROLLER ENTREGO VACIO EL IDINTERNOPOS, SE TRATO DE BLOQUEAR NUEVAMENTE LA BOMBA PARA SEGUIR LA TRANSACCION <SURGUIO UN ERROR SE TERMINA LA VENTA SIN SER REGISTRADA>.  IDSEGUIMIENTO: " + idSeguimiento);
                        return new int[] { -98, -98 };
                    }
                    else
                    {
                        exec.GeneraLogInfo("CODEVOL_WARNING", "@SHELLMX- INTENTO DE BLOQUEAR LA BOMBA " + request.FuellingPointId.ToString() + " <EXITOSAMENTE> EN LA VENTA, SE SIGUE LA TRANSACCION CORRECTAMENTE.  IDSEGUIMIENTO: " + idSeguimiento);
                        return new int[] { lockSupplyTransactionOfFuellingPointResponse.Id, 1 };
                    }
                }
                else
                {
                    return new int[] { validateFuellingPoint, int.Parse(validateLockFuelling.ToString()) }; //Se retorna el primer y unico surtidor..
                }
            }
            catch(Exception e)
            {
                exec.GeneraLogInfo("CODEVOL_TR ERROR", "@SHELLMX- ERROR EN LA PARTE DEL IDINTERNOPOS POR PARTE DEL PSSCONTROLLER EN PARTE DEL CASTEO DONDE SE PRESENTA EL SIGUIENTE LOG  --->ID_TRANSACCION_BOMBA: " + istransaccion +  " IDSEGUIMIENTO: " + idSeguimiento + "   " + e.ToString());
                return new int[] { -99 , -99 };
                throw e;
            }
        }

        public FinalizeSupplyTransactionResponse FinalizeSupplyTransactionWS(FinalizeSupplyTransactionRequest request)
        {
            return hubProxy.Invoke<FinalizeSupplyTransactionResponse>("FinalizeSupplyTransaction", request).Result;
        }

        public SetDefinitiveDocumentIdForSupplyTransactionsResponse SetDefinitiveDocumentIdForSupplyTransactionsWS(SetDefinitiveDocumentIdForSupplyTransactionsRequest request)
        {
            return hubProxy.Invoke<SetDefinitiveDocumentIdForSupplyTransactionsResponse>("SetDefinitiveDocumentIdForSupplyTransactions", request).Result;
        }

        public UnlockSupplyTransactionOfFuellingPointResponse UnlockSupplyTransactionOfFuellingPointWS(UnlockSupplyTransactionOfFuellingPointRequest request)
        {
            return hubProxy.Invoke<UnlockSupplyTransactionOfFuellingPointResponse>("UnlockSupplyTransactionOfFuellingPoint", request).Result;
        }

        public FinalizeSupplyTransactionForFuelTestResponse FinalizeSupplyTransactionForFuelTestWS(FinalizeSupplyTransactionForFuelTestRequest request)
        {
            return hubProxy.Invoke<FinalizeSupplyTransactionForFuelTestResponse>("FinalizeSupplyTransactionForFuelTest", request).Result;
        }
    }
}
