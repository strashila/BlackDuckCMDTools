using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace BlackDuckCMDTools
{
    public class BlackDuckRestAPI
    {
        private string baseUrl;
        private string APIToken;
        private string bearerToken;
        private string authorizationBearerString;
        private HttpClient httpClient;
        private HttpResponseMessage responseMessage;
        private HttpStatusCode responsestatuscode;



        public BlackDuckRestAPI(string url, string token)
        {
            this.baseUrl = url;
            this.APIToken = token;
            this.httpClient = new CustomHTTPclientCertificateValidationHandler().CreateHTTPClientNoCertificateValidation();  // this is for handler without cert validation. for regular httpclient use this.httpClient = new HttpClient();
            this.bearerToken = this.CreateBearerToken();
            this.authorizationBearerString = "Bearer " + bearerToken;
        }

        public BlackDuckRestAPI(string url, string token, string bdserverhash) // overload for server hash
        {
            this.baseUrl = url;
            this.APIToken = token;
            this.httpClient = new CustomHTTPclientCertificateValidationHandler().CreateHTTPClientCertificateValidationWithServerHash(bdserverhash); // This HTTPhandler is set to check specific server hash abd validate by that hash
            this.bearerToken = this.CreateBearerToken();
            this.authorizationBearerString = "Bearer " + bearerToken;
        }


        private string CreateBearerToken()
        {
            var authURL = "/api/tokens/authenticate";
            var fullURL = this.baseUrl + authURL;
            var tokenAuthString = "token " + this.APIToken;
            var acceptHeader = "application/vnd.blackducksoftware.user-4+json";
            var localRequestHandler = new AsyncRequestHandler();
            var content = "";

            var bearerResponseString = localRequestHandler.ReturnHTTPRequestResult(fullURL, tokenAuthString, HttpMethod.Post, acceptHeader, content, this.httpClient);

            var bearerResponse = JsonConvert.DeserializeObject<BlackDuckBearerToken>(bearerResponseString);         

            return bearerResponse.bearerToken;
        }



        public string ReturnPolicyRules()
        {

            var fullURL = this.baseUrl + "/api/policy-rules";
            var acceptHeader = "application/vnd.blackducksoftware.policy-5+json";
            var content = "";

            var localRequestHandler = new AsyncRequestHandler();

            var policyRules = localRequestHandler.ReturnHTTPRequestResult(fullURL, this.authorizationBearerString, HttpMethod.Get, acceptHeader, content, this.httpClient);
          
            return policyRules;
        }

        public string ReturnBearerToken()
        {
            return this.bearerToken;
        }


        public string getProjectIDFromName(string projectName) //helper function
        {
            var additinalSearchParams = "?q=name:" + projectName;
            var fullURL = this.baseUrl + "/api/projects" + additinalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-4+json";
            var content = "";
            
            var localRequestHandler = new AsyncRequestHandler();
            var projectsString = localRequestHandler.ReturnHTTPRequestResult(fullURL, this.authorizationBearerString, HttpMethod.Get, acceptHeader, content, this.httpClient);

            var projectsListing = JsonConvert.DeserializeObject<BlackDuckAPIProjectsListing>(projectsString);

            if (projectsListing.totalCount == 0)
            {
                return null; //placeholder for throwing exception
            }

            else
            {
                var project = projectsListing.items[0];
                var projectURL = project._meta.href;
                var projectID = projectURL.Split('/').Last();

                return projectID;
            }

        }

        public string getProjectVersionsFromName(string projectName)
        {
            var projectID = this.getProjectIDFromName(projectName);
            if (projectID == null)
            {
                return null; //placeholder for throwing exception
            }
            var fullURL = this.baseUrl + "/api/projects/" + projectID + "/versions";
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-5+json";
            var content = "";

            var localRequestHandler = new AsyncRequestHandler();
            var projectsVersionsString = localRequestHandler.ReturnHTTPRequestResult(fullURL, this.authorizationBearerString, HttpMethod.Get, acceptHeader, content, this.httpClient);

            return projectsVersionsString;



        }


        public HttpResponseMessage ReturnHttpResponseMessage()
        {
            return this.responseMessage;
        }













    }
}
