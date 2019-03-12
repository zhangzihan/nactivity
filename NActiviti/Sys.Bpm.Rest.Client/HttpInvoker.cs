using Newtonsoft.Json;
using Sys.Bpm.api.http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Bpm.Rest.Client
{
    public class HttpInvoker
    {
        private readonly HttpClient httpClient;

        public HttpInvoker(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<T> PostAsync<T>(string uri, object data)
        {
            HttpResponseMessage response = await httpClient.PostAsync(uri, new JsonContent(data));

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default(T);
            }

            string responseData = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(responseData);
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            HttpResponseMessage response = await httpClient.GetAsync(uri);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default(T);
            }

            string responseData = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(responseData);
        }
    }
}
