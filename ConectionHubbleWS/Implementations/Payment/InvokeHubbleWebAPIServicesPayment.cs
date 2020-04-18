using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Conection.HubbleWS
{
    public class InvokeHubbleWebAPIServicesPayment
    {
        #region PROCESO PARA EL PINPAD HUBBLE

        // SHELLLMX- Este metodo se encuentra en <<<HubblePOSWebAPI/Controller/MainController.cs>>> se extrajo del metodo Original del ServiciosPinpad
        public async Task<string> ServiciosPinpad(Request_IC0 request_IC0)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/ServiciosPinpad", request_IC0))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        string deserializeJson = JsonConvert.DeserializeObject<string>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        #endregion
    }
}
