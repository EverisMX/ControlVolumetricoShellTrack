using Conection.HubbleWS.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Conection.HubbleWS
{
    public class InvokeWSIntegracionShellMXServices
    {
        #region Variables
        private static string URL_SERVICE_INTEGRACIONSHL = "http://localhost:50490/api/"; //PRE
        private readonly string USER_BBDD = "arr"; // PRE
        #endregion Variables

        // SHELLMX- Este metodo se encuentra en <<<WSIntegracionShellMX/Controller/TipController.cs>>> se extrajo del metodo Original del RegisterTip
        public async Task<RegisterTipResponse> RegisterTip(RegisterTipRequest registerTipRequest)
        {
            URL_SERVICE_INTEGRACIONSHL += "api/tip/";

            //SHELLMX- Completa campos de DSN
            registerTipRequest.User = USER_BBDD;
            registerTipRequest.nCliente = registerTipRequest.Company;

            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                object objInput = registerTipRequest;
                string timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                string signature = Convert.ToString(GenericHelper.GeneratedSignature(objInput, timestamp, "0x12A6"));

                client.BaseAddress = new Uri(URL_SERVICE_INTEGRACIONSHL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("TimeStamp", timestamp);
                client.DefaultRequestHeaders.Add("Signature", signature);

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("tipregister", registerTipRequest))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        RegisterTipResponse deserializeJson = JsonConvert.DeserializeObject<RegisterTipResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

    }
}
