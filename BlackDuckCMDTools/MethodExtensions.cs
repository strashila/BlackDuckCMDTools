using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;

namespace BlackDuckCMDTools
{

    /// <summary>
    /// Method extensions for the library
    /// </summary>
    public static class MethodExtensions
    {
        public static async System.Threading.Tasks.Task<string> MakeHTTPRequestAsync(this HttpClient httpClient, string url, string authorizationTokenString, HttpMethod method, string acceptHeader, StringContent content)
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(url),
                Headers = {
                                { HttpRequestHeader.Authorization.ToString(), authorizationTokenString },
                                { HttpRequestHeader.Accept.ToString(), acceptHeader },
                          },
                Content = content
            };

            var response = await httpClient.SendAsync(httpRequestMessage);
            var responseContent = response.Content.ReadAsStringAsync().Result;

            return responseContent;

        }


        public static async System.Threading.Tasks.Task<HttpResponseMessage> MakeHTTPRequestReturnFullResponseMessage(this HttpClient httpClient, string url, string authorizationTokenString, HttpMethod method, string acceptHeader, StringContent content)
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(url),
                Headers = {
                                { HttpRequestHeader.Authorization.ToString(), authorizationTokenString },
                                { HttpRequestHeader.Accept.ToString(), acceptHeader },
                          },
                Content = content
            };

            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            return response;

        }

    }    
}
