using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BlackDuckCMDTools;
using System.Threading.Tasks;

namespace BlackDuckCMDTools
{
    public class BlackDuckRestAPI
    {
        private string _baseUrl;
        private string _apiToken;
        private string _bearerToken;
        private string _authorizationBearerString;
        private HttpClient _httpClient;


        public BlackDuckRestAPI(string url, string token, bool certValidation)
        {
            this._baseUrl = url;
            this._apiToken = token;
            if (certValidation)
            {
                // regular HttpClient with cert validation
                this._httpClient = new HttpClient(); 
            }

            else

            {
                // this is for handler without cert validation. for regular httpclient use this.httpClient = new HttpClient();
                this._httpClient = new CustomHTTPclientCertificateValidationHandler().CreateHTTPClientNoCertificateValidation();  
            }
           
            this._bearerToken = this.CreateBearerToken();
            this._authorizationBearerString = "Bearer " + _bearerToken;
        }

        public BlackDuckRestAPI(string url, string token, string bdserverhash) //
        {
            /// This is an overload constructor with server hash verification
            /// HTTPhandler is set to check specific server hash and validate by that hash
            

            this._baseUrl = url;
            this._apiToken = token;
            this._httpClient = new CustomHTTPclientCertificateValidationHandler().CreateHTTPClientCertificateValidationWithServerHash(bdserverhash); 
            this._bearerToken = this.CreateBearerToken();
            this._authorizationBearerString = "Bearer " + _bearerToken;
        }


        private string CreateBearerToken()
        {
            var authURL = "/api/tokens/authenticate";
            var fullURL = this._baseUrl + authURL;
            var tokenAuthString = "token " + this._apiToken;
            var acceptHeader = "application/vnd.blackducksoftware.user-4+json";
            var content = "";

            string bearerResponseString = this._httpClient.MakeHTTPRequestAsync(fullURL, tokenAuthString, HttpMethod.Post, acceptHeader, content).Result;

            BlackDuckBearerToken bearerResponse = JsonConvert.DeserializeObject<BlackDuckBearerToken>(bearerResponseString);         

            return bearerResponse.bearerToken;
        }




        public List<BlackDuckBOMComponent> GetBOMComponentsFromProjectVersion(string projectname, string versionname, string additionalSearchParams)
        {
            var projectId = this.GetProjectIdFromName(projectname);
            var versionId = this.GetProjectVersionIdByProjectNameAndVersionName(projectname, versionname);
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/components" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.bill-of-materials-6+json";
            var content = "";

            string componentListingJson = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            var componentListingJobject = JObject.Parse(componentListingJson);


            /// This is an alternative method of parsing the Listing API response, where we don't deserialize the entire response
            /// but parsing it with JObject.Parse and then casting the "items" list to appropriate type
            /// or we can return the entire ["items"] as string

            var componentList = componentListingJobject["items"].ToObject<List<BlackDuckBOMComponent>>();
            return componentList;

            /// This is the method in which you Deserialize the reply json by listing object BlackDuckAPIComponentsListing

            //var componentsListing = JsonConvert.DeserializeObject<BlackDuckAPIComponentsListing>(componentsString);
            //var components = componentsListing.items;
            //return components;
        }

        public string ReturnPolicyRules()
        {

            var fullURL = this._baseUrl + "/api/policy-rules";
            var acceptHeader = "application/vnd.blackducksoftware.policy-5+json";
            var content = "";

            string policyRules = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;
            return policyRules;
        }

        public string ReturnBearerToken()
        {
            return this._bearerToken;
        }


        public string GetProjectIdFromName(string projectName) //helper function for ProjectId
        {
            var additinalSearchParams = "?q=name:" + projectName;
            var fullURL = this._baseUrl + "/api/projects" + additinalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-4+json";
            var content = "";

            string projectsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            BlackDuckAPIProjectsListing projectsListing = JsonConvert.DeserializeObject<BlackDuckAPIProjectsListing>(projectsString);

            if (projectsListing.totalCount == 0)
            {
                //if no project found we're returning the whole reply string
                return projectsString; 
            }

            else
            {
                // First project in the list, should be the only one
                var project = projectsListing.items[0];
                var projectURL = project._meta.href;
                var projectId = projectURL.Split('/').Last();
                return projectId;
            }
        }


        public string GetProjectVersionIdByProjectNameAndVersionName(string projectName, string versionName) 
        {
            var projectID = this.GetProjectIdFromName(projectName);
            var additinalSearchParams = "?q=versionName:" + versionName;
            var fullURL = this._baseUrl + "/api/projects/" + projectID + "/versions" + additinalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-5+json";
            var content = "";

            string projectsVersionsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            BlackDuckAPIProjectVersionsListing projectsVersionListing = JsonConvert.DeserializeObject<BlackDuckAPIProjectVersionsListing>(projectsVersionsString);

            if (projectsVersionListing.totalCount == 0)
            {
                return projectsVersionsString; 
            }
                  
            else
            {
                BlackDuckProjectVersion version = projectsVersionListing.items[0]; 
                string versionURL = version._meta.href;
                string versionID = versionURL.Split('/').Last();
                return versionID;
            }
        }



        public List<BlackDuckMatchedFileWithComponent> GetBOMMatchedFilesWithComponent(string projectName, string versionName, string additionalSearchParams) 
        {

            /// api-doc/public.html#matched-file-with-component-representation
            /// 

            string projectId = this.GetProjectIdFromName(projectName);
            string versionId = this.GetProjectVersionIdByProjectNameAndVersionName(projectName, versionName);
            
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/matched-files" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.bill-of-materials-6+json";
            var content = "";

            string matchedFilesJson = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject matchedFilesListingJobject = JObject.Parse(matchedFilesJson);  

            if (matchedFilesListingJobject["items"] != null)
            {
                return matchedFilesListingJobject["items"].ToObject<List<BlackDuckMatchedFileWithComponent>>(); 
            }

            else return null;
        }


        public string ParseComponentId (string component)

        {
            var compSplit = component.Split('/');
            var compIndex = Array.IndexOf(compSplit, "components");
            if (compIndex == -1)
            {
                return "";
            }
            else
            {
                return compSplit[compIndex + 1];
            }
        }


    }
}
