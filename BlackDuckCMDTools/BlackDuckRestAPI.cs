using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlackDuckCMDTools
{
    public class BlackDuckRestAPI
    {
        private string baseUrl;
        private string APIToken;
        private string bearerToken;
        private string authorizationBearerString;
        private HttpClient httpClient;


        public BlackDuckRestAPI(string url, string token, bool certValidation)
        {
            this.baseUrl = url;
            this.APIToken = token;
            if (certValidation)
            {
                this.httpClient = new HttpClient(); // regular HttpClient with cert validation
            }

            else
            {
                this.httpClient = new CustomHTTPclientCertificateValidationHandler().CreateHTTPClientNoCertificateValidation();  // this is for handler without cert validation. for regular httpclient use this.httpClient = new HttpClient();
            }
           
            this.bearerToken = this.CreateBearerToken();
            this.authorizationBearerString = "Bearer " + bearerToken;
        }

        public BlackDuckRestAPI(string url, string token, string bdserverhash) // overload with server hash verification
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


        public string getProjectIdFromName(string projectName) //helper function
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
                return ""; //placeholder for throwing exception
            }

            else
            {
                var project = projectsListing.items[0]; // First project by that name, should be the only one
                var projectURL = project._meta.href;
                var projectID = projectURL.Split('/').Last();
                return projectID;
            }
        }


        public string getProjectVersionIdByProjectNameAndVersionName(string projectName, string versionName) //helper function
        {
            var projectID = this.getProjectIdFromName(projectName);
            var additinalSearchParams = "?q=versionName:" + versionName;
            var fullURL = this.baseUrl + "/api/projects/" + projectID + "/versions" + additinalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-5+json";
            var content = "";

            var localRequestHandler = new AsyncRequestHandler();
            var projectsVersionsString = localRequestHandler.ReturnHTTPRequestResult(fullURL, this.authorizationBearerString, HttpMethod.Get, acceptHeader, content, this.httpClient);
            var projectsVersionListing = JsonConvert.DeserializeObject<BlackDuckAPIProjectVersionsListing>(projectsVersionsString);

            if (projectsVersionListing.totalCount == 0)
            {
                return ""; //placeholder for throwing exception
            }
                  
            else
            {
                var version = projectsVersionListing.items[0]; // First version by that name, should be the only one
                var versionURL = version._meta.href;
                var versionID = versionURL.Split('/').Last();
                return versionID;
            }
        }



        public string getBOMMatchedFilesWithComponent(string projectName, string versionName, string additionalSearchParams) 
        {

            /// api-doc/public.html#matched-file-with-component-representation
            /// 

            var projectId = this.getProjectIdFromName(projectName);
            var versionId = this.getProjectVersionIdByProjectNameAndVersionName(projectName, versionName);
            
            var fullURL = this.baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/matched-files" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.bill-of-materials-6+json";
            var content = "";

            var localRequestHandler = new AsyncRequestHandler();
            var matchedFilesJson = localRequestHandler.ReturnHTTPRequestResult(fullURL, this.authorizationBearerString, HttpMethod.Get, acceptHeader, content, this.httpClient);

            var matchedFilesListingJobject = JObject.Parse(matchedFilesJson);

            return matchedFilesListingJobject["items"].ToString();
        }


        public string getComponentMatchedFiles(string projectName, string versionName, string componentId, string additionalSearchParams)
        {

            /// api-doc/public.html#matched-file-representation
            /// 

            var projectId = this.getProjectIdFromName(projectName);
            var versionId = this.getProjectVersionIdByProjectNameAndVersionName(projectName, versionName);

            var fullURL = this.baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/components/" + componentId + "/matched-files" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.bill-of-materials-6+json";
            var content = "";

            var localRequestHandler = new AsyncRequestHandler();
            var componentMatchedFilesJson = localRequestHandler.ReturnHTTPRequestResult(fullURL, this.authorizationBearerString, HttpMethod.Get, acceptHeader, content, this.httpClient);

            var listingJobject = JObject.Parse(componentMatchedFilesJson);

            return listingJobject["items"].ToString();

        }


        public List<BlackDuckBOMComponent> getComponentsFromProjectNameAndVersionName(string projectname, string versionname, string additionalSearchParams)
        {
            var projectId = this.getProjectIdFromName(projectname);
            var versionId = this.getProjectVersionIdByProjectNameAndVersionName(projectname, versionname);
            var fullURL = this.baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/components" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.bill-of-materials-6+json";
            var content = "";

            var localRequestHandler = new AsyncRequestHandler();
            var componentListingJson = localRequestHandler.ReturnHTTPRequestResult(fullURL, this.authorizationBearerString, HttpMethod.Get, acceptHeader, content, this.httpClient);

            var componentListingJobject = JObject.Parse(componentListingJson);


            /// This is an alternative method of parsing the Listing API response, where we don't deserialize the entire response
            /// but parsing it with JObject.Parse and then casting the "items" list to appropriate type

            var componentList = componentListingJobject["items"].ToObject<List<BlackDuckBOMComponent>>();
            return componentList;



            /// This is the method in which you Deserialize the reply json by listing object BlackDuckAPIComponentsListing

            //var componentsListing = JsonConvert.DeserializeObject<BlackDuckAPIComponentsListing>(componentsString);
            //var components = componentsListing.items;
            //return components;
        }
    }
}
