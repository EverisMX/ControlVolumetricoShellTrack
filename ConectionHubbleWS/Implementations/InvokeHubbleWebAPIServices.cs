using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Conection.HubbleWS.Models.Facturacion;
using Conection.HubbleWS.Models.Hubble;

namespace Conection.HubbleWS
{
    public class InvokeHubbleWebAPIServices
    {
        #region BASE WEBAPI
        // SHELLMX- Este metodo se encuentra en <<<HubblePOSWebAPI/Controller/MainController.cs>>> se extrajo del metodo Original del GetProductForSaleRequest
        public async Task<GetProductForSaleResponse> GetProductForSale(GetProductForSaleRequest getProductForSaleRequest)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/GetProductForSale", getProductForSaleRequest))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;
                       
                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetProductForSaleResponse deserializeJson = JsonConvert.DeserializeObject<GetProductForSaleResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }
        #endregion

        #region facturacion
        public async Task<facresponse> tpvfacturacionn(GenerateElectronicInvoice requestfac)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:8091");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                    using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/Electronicbill", requestfac))
                    {

                        //response.EnsureSuccessStatusCode();
                        if (response.IsSuccessStatusCode)
                        {
                            var responseJson = response.Content.ReadAsStringAsync().Result;
                            //como es un objeto nos genera basura por lo que la remplazamos 
                            string responsejsonn = responseJson.Replace("\\", "").Replace("\"{", "{").Replace("}\"", "}");
                            //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                            facresponse deserializeJson = JsonConvert.DeserializeObject<facresponse>(responsejsonn);


                            return deserializeJson;
                        }
                        else
                            return new facresponse();
                    }

                }
            }
            catch (Exception e)
            {
                throw e;
                //OnConnectionFailed?.Invoke(e.Message);
            }

        }



        #endregion

        #region customer
        public async Task<GetCustomerResponse> GetCustomer(GetCustomerRequest resquestcustomer)
        {



            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/GetCustomer", resquestcustomer))
                {

                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;
                        ////como es un objeto nos genera basura por lo que la remplazamos 
                        //string responsejsonn = responseJson.Replace("\\", "").Replace("\"{", "{").Replace("}\"", "}");
                        ////SHELLMX- Se desSerializa para transformarlo en un Objeto.

                        GetCustomerResponse deserializeJson = JsonConvert.DeserializeObject<GetCustomerResponse>(responseJson);


                        return deserializeJson;


                    }
                    else
                        return null;
                }

            }

        }

        #endregion

        #region getdocumet
        public async Task<GetDocumentResponse> GetDocument(GetDocumentRequest requesgetdocument)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/GetDocument", requesgetdocument))
                {

                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;
                        ////como es un objeto nos genera basura por lo que la remplazamos 
                        //string responsejsonn = responseJson.Replace("\\", "").Replace("\"{", "{").Replace("}\"", "}");
                        ////SHELLMX- Se desSerializa para transformarlo en un Objeto.

                        GetDocumentResponse deserializeJson = JsonConvert.DeserializeObject<GetDocumentResponse>(responseJson);


                        return deserializeJson;


                    }
                    else
                        return null;
                }

            }

        }

        #endregion

        #region getprint
        public async Task<GetPrintingConfigurationResponse> GetPrintingConfiguration(GetPrintingConfigurationRequest requesGetPrinting)
        {



            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/GetPrintingConfiguration", requesGetPrinting))
                {

                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;
                        ////como es un objeto nos genera basura por lo que la remplazamos 
                        //string responsejsonn = responseJson.Replace("\\", "").Replace("\"{", "{").Replace("}\"", "}");
                        ////SHELLMX- Se desSerializa para transformarlo en un Objeto.

                        GetPrintingConfigurationResponse deserializeJson = JsonConvert.DeserializeObject<GetPrintingConfigurationResponse>(responseJson);


                        return deserializeJson;


                    }
                    else
                        return null;
                }

            }

        }

        #endregion

        #region get post information
        // SHELLMX- Este metodo se encuentra en <<<HubblePOSWebAPI/Controller/MainController.cs>>> se extrajo del metodo Original del GetPOSInformation
        public async Task<GetPOSInformationResponse> GetPOSInformation(GetPosInformationRequest getPosInformationRequest)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));



                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/GetPOSInformation", getPosInformationRequest))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;



                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetPOSInformationResponse deserializeJson = JsonConvert.DeserializeObject<GetPOSInformationResponse>(responseJson);



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
