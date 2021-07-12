//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Http;
//using System.Text;

//namespace BlackDuckCMDTools
//{
//    public class AsyncRequestHandler
//    {              
//        public string ReturnHTTPRequestResult(string url, string authorizationTokenString, HttpMethod method, string acceptHeader, string content, HttpClient httpClient)
//        {
//            async System.Threading.Tasks.Task<string> MakeHTTPRequestAsync()
//            {

//                var httpRequestMessage = new HttpRequestMessage
//                {
//                    Method = method,
//                    RequestUri = new Uri(url),
//                    Headers = {
//                                { HttpRequestHeader.Authorization.ToString(), authorizationTokenString },
//                                { HttpRequestHeader.Accept.ToString(), acceptHeader },
//                            },
//                    Content = new StringContent(content)
//                };

//                var response = await httpClient.SendAsync(httpRequestMessage);
//                var responseContent = response.Content.ReadAsStringAsync().Result;

//                return responseContent;

//            }

//            return MakeHTTPRequestAsync().Result;
//        }
//    }
//}
