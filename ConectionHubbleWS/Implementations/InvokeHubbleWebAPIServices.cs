using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Conection.HubbleWS.Models.Facturacion;

namespace Conection.HubbleWS
{
    public class InvokeHubbleWebAPIServices
    {
        #region BASE WEBAPI DONDE SE MANDAN A LLAMAR DEL HUBBLE

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

        // SHELLMX- Este metodo se encuentra en <<<HubblePOSWebAPI/Controller/MainController.cs>>> se extrajo del metodo Original del GetPOSConfiguration
        public async Task<GetPOSConfigurationResponse> GetPOSConfiguration(GetPOSConfigurationRequest getPOSConfigurationRequest)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/GetPOSConfiguration", getPOSConfigurationRequest))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetPOSConfigurationResponse deserializeJson = JsonConvert.DeserializeObject<GetPOSConfigurationResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

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

        // SHELLMX- Este metodo se encuentra en <<<HubblePOSWebAPI/Controller/MainController.cs>>> se extrajo del metodo Original del GetOperator
        public async Task<GetOperatorResponse> GetOperator(GetOperatorRequest getOperatorRequest)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/GetOperator", getOperatorRequest))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetOperatorResponse deserializeJson = JsonConvert.DeserializeObject<GetOperatorResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

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
                        else if (response.IsSuccessStatusCode != true)
                        {
                            return new facresponse();
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
            try
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
                            return new GetCustomerResponse();
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
            try
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
                            return new GetPrintingConfigurationResponse();
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

        // SHELLMX- Este metodo se encuentra en <<<HubblePOSWebAPI/Controller/MainController.cs>>> se extrajo del metodo Original del GetSeries
        public async Task<GetSeriesResponse> GetSeries(GetSeriesRequest getSeriesRequest )
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/GetSeries", getSeriesRequest))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetSeriesResponse deserializeJson = JsonConvert.DeserializeObject<GetSeriesResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        // SHELLMX- Este metodo se encuentra en <<<HubblePOSWebAPI/Controller/MainController.cs>>> se extrajo del metodo Original del GetPaymentMethods
        public async Task<GetPaymentMethodsResponse> GetPaymentMethods(GetPaymentMethodsRequest getPaymentMethodsRequest)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/GetPaymentMethods", getPaymentMethodsRequest))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetPaymentMethodsResponse deserializeJson = JsonConvert.DeserializeObject<GetPaymentMethodsResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        // SHELLMX- Este metodo se encuentra en <<<HubblePOSWebAPI/Controller/MainController.cs>>> se extrajo del metodo Original del GetCurrencies
        public async Task<GetCurrenciesResponse> GetCurrencies(GetCurrenciesRequest getCurrenciesRequest)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/GetCurrencies", getCurrenciesRequest))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetCurrenciesResponse deserializeJson = JsonConvert.DeserializeObject<GetCurrenciesResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        // SHELLMX- Este metodo se encuentra en <<<HubblePOSWebAPI/Controller/MainController.cs>>> se extrajo del metodo Original del CreateDocuments
        public async Task<CreateDocumentsResponse> CreateDocuments(CreateDocumentsRequest createDocumentsRequest)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/CreateDocuments", createDocumentsRequest))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        CreateDocumentsResponse deserializeJson = JsonConvert.DeserializeObject<CreateDocumentsResponse>(responseJson);

                        return deserializeJson;
                    }
                    else
                        return null;
                }
            }
        }

        public async Task<GetCompanyResponse> GetCompany(GetCompanyRequest GetCompanyreques)
        {
            //SHELLMX- Se manda a llamar el metodo HttpClient.
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8091");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //SHELLMX se crea el llamado de la solicitud para la peticion HTTP.
                using (HttpResponseMessage response = await client.PostAsJsonAsync("/main/GetCompany", GetCompanyreques))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = response.Content.ReadAsStringAsync().Result;

                        //SHELLMX- Se desSerializa para transformarlo en un Objeto.
                        GetCompanyResponse deserializeJson = JsonConvert.DeserializeObject<GetCompanyResponse>(responseJson);

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
