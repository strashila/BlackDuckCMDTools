using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Text;

namespace BlackDuckCMDTools
{
    public static class CustomHTTPclientCertificateValidationHandler
    {
        /// <summary>
        /// This is a class that handles custom SSL certificate validation
        /// It returns HTTPhandlers set either to trust all certs or to check server hash
        /// </summary>
        /// <returns></returns>

        //public CustomHTTPclientCertificateValidationHandler()
        //{

        //}

        public static HttpClient CreateHTTPClientNoCertificateValidation()
        {
            // This HTTPhandler is set to make .NET to ignore all Certificate Validation errors https://stackoverflow.com/questions/2675133/c-sharp-ignore-certificate-errors
            // We need that since all of our internal support BD hubs don't have proper SSL certificate installed
            // If the error not ignored, the SSL connection would not be established, and there would be a System.Security.Authentication.AuthenticationException: The remote certificate is invalid according to the validation procedure. 


            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => {
                return true;                               
            };
                
            return new HttpClient(httpClientHandler);
        }


        public static HttpClient CreateHTTPClientCertificateValidationWithServerHash(string hash)
        {
            // This HTTPhandler is set to check specific server hash and validate by that hash
            // In Chrome click on Secure or Not Secure in the address bar. Then click on Certificate -> Details -> Thumbprint and copy the value.

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;   
                }

              
                if (cert.GetCertHashString().ToLower() == hash) 
                {
                    return true;
                }
                return false;
            };

            return new HttpClient(httpClientHandler);
        }
    }
}
