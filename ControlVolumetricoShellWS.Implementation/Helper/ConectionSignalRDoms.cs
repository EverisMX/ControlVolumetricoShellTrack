using System;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Client;

namespace ControlVolumetricoShellWS.Implementation
{
    public class ConectionSignalRDoms
    {
        public string serverAddress = "http://localhost:8092/signalr";
        public event Action OnConnected;
        public event Action OnDisconnected;

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
            while (!isConnected)
            {
                try
                {
                    hubConnection.Start().Wait();
                    Console.WriteLine("Comienza con la Conexion pssHub");
                    hubConnection.Closed += OnHubConnectionClosed;

                    isConnected = true;
                    OnConnected?.Invoke();
                    Console.ReadKey();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //LogError("Connection to server failed: " + e.Message);
                }
            }
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

        public string LockSupplyTransactionOfFuellingPoint(string value)
        {
            try
            {
                // SHELLMX- Se manda a consumir el metodo para traer la informacion del Operador-
                // 1- Para el Proceso se debe traer en primera instancia el operador en turno.
                // 2- Debe de estar almacenado en una proceso o proyecto. 
                // * SE COLOCARA EN DURO EL PROCESO PARA PRUEBAS.
                // Se pregunta si esta conecta
                if (!IsConnected())
                {
                    ConnectToServer();
                }

                string lockSupplyTransaction = "";
                GetAllSupplyTransactionsOfFuellingPointRequest request = new GetAllSupplyTransactionsOfFuellingPointRequest() { OperatorId = "", FuellingPointId = 8 };

                // SHELLMX- Se invoca al cliente de SignalR para el consumo del PSS, que se necesita para las transaccion que se tiene en el surtidor seleccinado.
                //        donde se entrega un objeto de la transaccion que esta habilitado en un surtudor. 

                GetAllSupplyTransactionsOfFuellingPointResponse supplyTransactionOfFuellingPoint = hubProxy.Invoke<GetAllSupplyTransactionsOfFuellingPointResponse>("GetAllSupplyTransactionsOfFuellingPoint", request).Result;

                // SHELLMX- Al momento de traer la informacion sobre la transaccion que hay en parte sobre un surtidor, bloquea en el TVP que Action lo este usando, Se contruye el objeto
                //          a llenar de lock para traer la demas informacion sobre la transaccion del Surtidor seleccinado.

                LockSupplyTransactionOfFuellingPointRequest lockRequest = new LockSupplyTransactionOfFuellingPointRequest()
                {
                    CustomerId = "0393300000",
                    FuellingPointId = request.FuellingPointId,
                    OperatorId = request.OperatorId
                };

                foreach (var supply in supplyTransactionOfFuellingPoint.SupplyTransactionList)
                {
                    lockRequest.SupplyTransactionId = supply.Id;
                }

                LockSupplyTransactionOfFuellingPointResponse lockSupplyTransactionOfFuellingPoint = hubProxy.Invoke<LockSupplyTransactionOfFuellingPointResponse>("LockSupplyTransactionOfFuellingPoint", lockRequest).Result;
                lockSupplyTransaction = JsonConvert.SerializeObject(lockSupplyTransactionOfFuellingPoint);

                return lockSupplyTransaction;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "ERRROR";
                //OnConnectionFailed?.Invoke(e.Message);
            }
        }
    }
}
