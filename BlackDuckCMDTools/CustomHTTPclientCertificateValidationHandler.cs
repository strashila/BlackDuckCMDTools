using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Text;

namespace BlackDuckCMDTools
{
    public class CustomHTTPclientCertificateValidationHandler
    {
        public CustomHTTPclientCertificateValidationHandler()
        {

        }

        public HttpClient CreateHTTPClientNoCertificateValidation()
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


        public HttpClient CreateHTTPClientCertificateValidationWithServerHash(string hash)
        {
            // This HTTPhandler is set to check specific server hash abd validate by that hash
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
