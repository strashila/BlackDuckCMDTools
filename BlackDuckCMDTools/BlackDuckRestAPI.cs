using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BlackDuckCMDTools;
using System.Threading.Tasks;
using System.Text;

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
                // This creates a regular HttpClient which does cert validation
                this._httpClient = new HttpClient(); 
            }

            else
            {
                // HttpClient without cert validation. For regular httpclient use this.httpClient = new HttpClient();
                this._httpClient = CustomHTTPclientCertificateValidationHandler.CreateHTTPClientNoCertificateValidation();  
            }
           
            this._bearerToken = this.CreateBearerToken();
            this._authorizationBearerString = "Bearer " + _bearerToken;
        }



        public BlackDuckRestAPI(string url, string token, bool certValidation, int timeout)
        {
            this._baseUrl = url;
            this._apiToken = token;
            if (certValidation)
            {
                // This creates a regular HttpClient which does cert validation
                this._httpClient = new HttpClient();
                _httpClient.Timeout = TimeSpan.FromSeconds(timeout);

            }

            else
            {
                // HttpClient without cert validation. For regular httpclient use this.httpClient = new HttpClient();
                this._httpClient = CustomHTTPclientCertificateValidationHandler.CreateHTTPClientNoCertificateValidation();
                _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
            }

            this._bearerToken = this.CreateBearerToken();
            this._authorizationBearerString = "Bearer " + _bearerToken;
        }



        public BlackDuckRestAPI(string url, string token, string bdServerHash)
        {
            /// This is an overload constructor with server hash verification
            /// HTTPhandler is set to check specific server hash and validate by that hash            

            this._baseUrl = url;
            this._apiToken = token;
            this._httpClient = CustomHTTPclientCertificateValidationHandler.CreateHTTPClientCertificateValidationWithServerHash(bdServerHash);
            this._bearerToken = this.CreateBearerToken();
            this._authorizationBearerString = "Bearer " + _bearerToken;
        }


        private string CreateBearerToken()
        {
            var authURL = "/api/tokens/authenticate";
            var fullURL = this._baseUrl + authURL;
            var tokenAuthString = "token " + this._apiToken;
            var acceptHeader = "application/vnd.blackducksoftware.user-4+json";
            var content = new StringContent("");

            string bearerResponseString = this._httpClient.MakeHTTPRequestAsync(fullURL, tokenAuthString, HttpMethod.Post, acceptHeader, content).Result;

            BlackDuckBearerToken bearerResponse = JsonConvert.DeserializeObject<BlackDuckBearerToken>(bearerResponseString);      

            return bearerResponse.bearerToken;
        }


        public void RefreshBearerToken()
        {
            this._bearerToken = this.CreateBearerToken();
            this._authorizationBearerString = "Bearer " + this._bearerToken;
        }


        public List<BlackDuckBOMComponent> GetBOMComponentsFromProjectNameVersionName(string projectName, string projectVersionName, string additionalSearchParams)
        {
            var projectId = this.GetProjectIdFromName(projectName);
            var versionId = this.GetVersionIdFromProjectNameAndVersionName(projectName, projectVersionName);
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/components" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.bill-of-materials-6+json";
            var content = new StringContent("");

            string componentListingJson = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            /// This is a main method of parsing the Listing API response, where we don't deserialize the entire response
            /// but parsing it with JObject.Parse and then casting the "items" list to appropriate type
            /// or we can return the entire ["items"] as string

            JObject componentListingJobject = JObject.Parse(componentListingJson);
            List<BlackDuckBOMComponent> componentList = componentListingJobject["items"].ToObject<List<BlackDuckBOMComponent>>();
            return componentList;

            /// This is the method in which you Deserialize the reply json by listing object BlackDuckAPIComponentsListing
            ///var componentsListing = JsonConvert.DeserializeObject<BlackDuckAPIComponentsListing>(componentsString);
            ///var components = componentsListing.items;
            ///return components;
        }




        public List<BlackDuckVulnerability> ListingVulnerabilitiesbyComponent(string componentId, string additionalSearchParams)

            // /api-doc/public.html#_listing_vulnerabilities_by_component
        {
            var fullURL = this._baseUrl + "/api/components/" + componentId + "/vulnerabilities" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.vulnerability-4+json";
            var content = new StringContent("");

            string vulnerabilityListingJson = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            /// This is a main method of parsing the Listing API response, where we don't deserialize the entire response
            /// but parsing it with JObject.Parse and then casting the "items" list to appropriate type
            /// or we can return the entire ["items"] as string

            JObject vulnerabilityListingJobject = JObject.Parse(vulnerabilityListingJson);
            List<BlackDuckVulnerability> vulnList = vulnerabilityListingJobject["items"].ToObject<List<BlackDuckVulnerability>>();
            return vulnList;
        }


        public List<BlackDuckLicense> GetLicenses (string additionalSearchParams)

        // /api-doc/public.html#_listing_vulnerabilities_by_component
        {
            var fullURL = this._baseUrl + "/api/licenses" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.component-detail-5+json";
            var content = new StringContent("");

            string licenseListingJson = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            /// This is a main method of parsing the Listing API response, where we don't deserialize the entire response
            /// but parsing it with JObject.Parse and then deserializing the "items" list to appropriate type
            /// or we can return the entire ["items"] as string

            JObject licensesListingJobject = JObject.Parse(licenseListingJson);
            List<BlackDuckLicense> licenseList = licensesListingJobject["items"].ToObject<List<BlackDuckLicense>>();
            return licenseList;
        }




        public BlackDuckLicense GetSingleLicense(string licenseUrl)

        {
            var fullURL = licenseUrl;
            var acceptHeader = "application/vnd.blackducksoftware.component-detail-5+json";
            var content = new StringContent("");

            string licenseJson = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            //In case we need to get a single license there is no [items] array, we can deserialize the json as is into the type
     
            var license = JsonConvert.DeserializeObject<BlackDuckLicense>(licenseJson);
            return license;
        }





        public string UpdateLicenseStatus(BlackDuckLicense license, string newLicenseStatus)
        {
            
            var fullUrl = license._meta.href;
            var acceptHeader = "application/vnd.blackducksoftware.component-detail-5+json";
            var contentTypeHeader = "application/vnd.blackducksoftware.component-detail-5+json";

            var licenseObject = new JObject(
                new JProperty("name", license.name),
                new JProperty("licenseFamily", JObject.FromObject(license.licenseFamily)),
                new JProperty("licenseStatus", newLicenseStatus.ToUpper())
                );

            var content = new StringContent(licenseObject.ToString(), Encoding.UTF8, contentTypeHeader);
            HttpResponseMessage responseMessage = this._httpClient.MakeHTTPRequestReturnFullResponseMessage(fullUrl, this._authorizationBearerString, HttpMethod.Put, acceptHeader, content).Result;

            return ((int)responseMessage.StatusCode).ToString() + " " + responseMessage.StatusCode.ToString();
        }




        public string DeleteProjectVersionByVersionName(string projectName, string projectVersionName)
        {
            string projectId = this.GetProjectIdFromName(projectName);
            string versionId = this.GetVersionIdFromProjectNameAndVersionName(projectName, projectVersionName);

            return this.DeleteProjectVersionByProjectIdVersionId(projectId, versionId);
        }

        public string DeleteProjectVersionByProjectIdVersionId(string projectId, string versionId)
        {
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId;
            var acceptHeader = "";
            var content = new StringContent("");

            HttpResponseMessage responseMessage = this._httpClient.MakeHTTPRequestReturnFullResponseMessage(fullURL, this._authorizationBearerString, HttpMethod.Delete, acceptHeader, content).Result;

            // Creating a readable status code response ourselves from HttpResponseMessage. Casting int to get status code number, like "204", and the StatusCode is "No Content"
            return ((int)responseMessage.StatusCode).ToString() + " " + responseMessage.StatusCode.ToString();
        }


        public string DeleteProjectByProjectId(string projectId)
        {
            var fullURL = this._baseUrl + "/api/projects/" + projectId;
            var acceptHeader = "";
            var content = new StringContent("");

            HttpResponseMessage responseMessage = this._httpClient.MakeHTTPRequestReturnFullResponseMessage(fullURL, this._authorizationBearerString, HttpMethod.Delete, acceptHeader, content).Result;

            // Creating a readable status code response ourselves from HttpResponseMessage. Casting int to get status code number, like "204", and the StatusCode is "No Content"
            return ((int)responseMessage.StatusCode).ToString() + " " + responseMessage.StatusCode.ToString();
        }

        public string DeleteCodelocation(string codelocationId)
        {
            var fullURL = this._baseUrl + "/api/codelocations/" + codelocationId;
            var acceptHeader = "";
            var content = new StringContent("");

            HttpResponseMessage responseMessage = this._httpClient.MakeHTTPRequestReturnFullResponseMessage(fullURL, this._authorizationBearerString, HttpMethod.Delete, acceptHeader, content).Result;

            // Creating a readable status code response ourselves from HttpResponseMessage. Casting int to get status code number, like "204", and the StatusCode is "No Content"
            return ((int)responseMessage.StatusCode).ToString() + " " + responseMessage.StatusCode.ToString();
        }



        public string CreateProjectReturnHttpMessage(string projectJson)
        {
            var fullURL = this._baseUrl + "/api/projects";
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-4+json";
            var content = new StringContent(projectJson, Encoding.UTF8, "application/vnd.blackducksoftware.project-detail-4+json");

            // Create Project APi does NOT return the result of HttpResponseMessage.
            // You need to read the FULL HttpResponseMessage and use Headers and StatusCode 
            // Then you parse the Headers with Headers.GetValues and get the value of "Location" header, which contains the newly created project ID /projects/7cb65d55-1194-48e4-a3b0-a831d97253ee

            HttpResponseMessage responseMessage = this._httpClient.MakeHTTPRequestReturnFullResponseMessage(fullURL, this._authorizationBearerString, HttpMethod.Post, acceptHeader, content).Result;
            return responseMessage.ToString();
        }


        public string CreateProjectReturnProjectId(string projectJson)
        {
            var fullURL = this._baseUrl + "/api/projects";
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-4+json";
            var content = new StringContent(projectJson, Encoding.UTF8, "application/vnd.blackducksoftware.project-detail-4+json");

            // Create Project APi does NOT return the result of HttpResponseMessage.
            // You need to read the FULL HttpResponseMessage and use Headers and StatusCode 
            // Then you parse the Headers with Headers.GetValues and get the value of "Location" header, which contains the newly created project ID /projects/7cb65d55-1194-48e4-a3b0-a831d97253ee

            HttpResponseMessage responseMessage = this._httpClient.MakeHTTPRequestReturnFullResponseMessage(fullURL, this._authorizationBearerString, HttpMethod.Post, acceptHeader, content).Result;
            string projectUrl = responseMessage.Headers.GetValues("Location").First();
            string projectId = projectUrl.Split('/').Last();
            return projectId;
        }



        public string GenerateSbomReport(string projectId, string versionId)
        {
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/sbom-reports";
            var acceptHeader = "application/json";
            var contentTypeHeader = "application/json";
            var bodyObject = new JObject(
                   new JProperty("reportFormat", "JSON"),
                   new JProperty("reportType", "SBOM"),
                   new JProperty("sbomType", "SPDX_22")
                   );

            var content = new StringContent(bodyObject.ToString(), Encoding.UTF8, contentTypeHeader);

            // Create SBOM report API is not documented, and it does NOT return the result of HttpResponseMessage.
            // You need to read the FULL HttpResponseMessage and use Headers and StatusCode 
            // Then you parse the Headers with Headers.GetValues and get the value of "Location" header, which contains the newly created full report ID, like "https://BDurl/api/projects/.../versions/.../reports/f114afac-0d1c-43ce-9bf5-6b4d24576129"
            // The report content link as for 27/04/22 is at .../reports/{reportId}/contents. Download zip link is at  .../reports/{reportId}/download.zip

            HttpResponseMessage responseMessage = this._httpClient.MakeHTTPRequestReturnFullResponseMessage(fullURL, this._authorizationBearerString, HttpMethod.Post, acceptHeader, content).Result;

            string reportUrl = responseMessage.Headers.GetValues("Location").First();
            return reportUrl;
        }



        public string CreateVersionLicenseReport(string versionId, string reportJson)
        {
            var fullURL = this._baseUrl + "/api/versions/" + versionId + "/license-reports";
            var acceptHeader = "application/vnd.blackducksoftware.report-4+json";
            var content = new StringContent(reportJson, Encoding.UTF8, "application/vnd.blackducksoftware.report-4+json");

            // POST request to this API does NOT return the result of HttpResponseMessage.
            // You need to read the FULL HttpResponseMessage and use Headers and StatusCode 
            // Then you parse the Headers with Headers.GetValues and get the value of the location, which contains the newly created report ID: Location: /vulnerability-reports/{reportId}

            HttpResponseMessage responseMessage = this._httpClient.MakeHTTPRequestReturnFullResponseMessage(fullURL, this._authorizationBearerString, HttpMethod.Post, acceptHeader, content).Result;
            return responseMessage.ToString();
        }



        public string DeactivateUser(BlackDuckUser user)
        {           
            var fullURL = user._meta.href;
            var acceptHeader = "application/vnd.blackducksoftware.user-4+json";
            var contentType = "application/vnd.blackducksoftware.user-4+json";

            JObject userUpdateJobject = new JObject(
                   new JProperty("userName", user.userName),
                   new JProperty("firstName", user.firstName),
                   new JProperty("lastName", user.lastName),
                   new JProperty("type", user.type),
                   new JProperty("externalUserName", user.externalUserName),
                   new JProperty("email", user.email),
                   new JProperty("active", false)
                   );

            var content = new StringContent(userUpdateJobject.ToString(), Encoding.UTF8, contentType);

            var response = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Put, acceptHeader, content).Result;

            return response;
        }



        public string ReturnPolicyRules()
        {
            var fullURL = this._baseUrl + "/api/policy-rules";
            var acceptHeader = "application/vnd.blackducksoftware.policy-5+json";
            var content = new StringContent("");

            string policyRules = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;
            return policyRules;
        }

        public string ShowBearerToken()
        {
            return this._bearerToken;
        }


        public string GetProjectIdFromName(string projectName) //helper function for ProjectId
        {
            var additinalSearchParams = "?limit=1&q=name:" + projectName;  // We only need one project
            var fullURL = this._baseUrl + "/api/projects" + additinalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-4+json";
            var content = new StringContent("");

            string projectsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;
            
            JObject projectJObject = JObject.Parse(projectsString);

            List<BlackDuckProject> projectList = projectJObject["items"].ToObject<List<BlackDuckProject>>();

            // First project in the list, should be the only one 
            BlackDuckProject project = projectList[0];

            string projectId = project._meta.href.Split('/').Last();
            return projectId;
        }

        public List<BlackDuckProject> GetAllProjects(string additionalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/projects" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-4+json";
            var content = new StringContent("");
            string projectsVersionsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject projectJObject = JObject.Parse(projectsVersionsString);
            List<BlackDuckProject> projectList = projectJObject["items"].ToObject<List<BlackDuckProject>>();
            return projectList;
        }





        public List<BlackDuckScanSummary> GetScanSummaries(string codeLocationId, string additionalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/codelocations/" + codeLocationId  + "/scan-summaries" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.scan-5+json";
            var content = new StringContent("");
            string scanSummaryString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject scanSummariesJObject = JObject.Parse(scanSummaryString);
            List<BlackDuckScanSummary> scanSummaryList = scanSummariesJObject["items"].ToObject<List<BlackDuckScanSummary>>();
            return scanSummaryList;
        }



        public BlackDuckScanSummary GetLatestScanSummary(string codeLocationId)
        {
            var fullURL = this._baseUrl + "/api/codelocations/" + codeLocationId + "/latest-scan-summary";
            var acceptHeader = "application/vnd.blackducksoftware.scan-5+json";
            var content = new StringContent("");
            string scanSummaryString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject scanSummaryJObject = JObject.Parse(scanSummaryString);
            BlackDuckScanSummary scanSummary = scanSummaryJObject.ToObject<BlackDuckScanSummary>();
            return scanSummary;
        }



        public string GetLatestScanSummaryJson(string codeLocationId)
        {
            var fullURL = this._baseUrl + "/api/codelocations/" + codeLocationId + "/latest-scan-summary";
            var acceptHeader = "application/vnd.blackducksoftware.scan-5+json";
            var content = new StringContent("");
            string scanSummaryString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            return scanSummaryString;
        }



        public List<BlackDuckCodeLocation> GetAllCodeLocations(string additionalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/codelocations" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.scan-5+json";
            var content = new StringContent("");
            string codeLocationsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject codeLocationsJObject = JObject.Parse(codeLocationsString);
            List<BlackDuckCodeLocation> codeLocationsList = codeLocationsJObject["items"].ToObject<List<BlackDuckCodeLocation>>();
            return codeLocationsList;
        }




        public List<BlackDuckCodeLocation> GetVersionCodeLocations(string projectId, string versionId, string additionalSearchParams)
        {
            // This is an "internal", undocumented API. Does not accept any custom "Accept" header, just generic application/json

            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/codelocations" + additionalSearchParams;
            var acceptHeader = "application/json";
            var content = new StringContent("");
            string codeLocationsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject codeLocationsJObject = JObject.Parse(codeLocationsString);
            List<BlackDuckCodeLocation> codeLocationsList = codeLocationsJObject["items"].ToObject<List<BlackDuckCodeLocation>>();
            return codeLocationsList;
        }




        public BlackDuckCodeLocation UpdateCodeLocationVersionMapping(string codeLocationId, string mappedProjectVersion)
        {
            // This is the codelocation map/unmap function
            // What we're doing here is putting either an empty string in "mappedProjectVersion" or a valid Full project version URL

            var fullURL = this._baseUrl + "/api/codelocations/" + codeLocationId;
            var acceptHeader = "application/vnd.blackducksoftware.scan-5+json";
            var contentTypeHeader = "application/vnd.blackducksoftware.scan-5+json";

            var bodyObject = new JObject(
                   new JProperty("mappedProjectVersion", mappedProjectVersion)  
                   );

            var content = new StringContent(bodyObject.ToString(), Encoding.UTF8, contentTypeHeader);

            string codeLocationString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Put, acceptHeader, content).Result;

            JObject codeLocationJObject = JObject.Parse(codeLocationString);
            BlackDuckCodeLocation codeLocation = codeLocationJObject.ToObject<BlackDuckCodeLocation>();
            return codeLocation;
        }




        public List<BlackDuckComponentVersion> GetComponentVersions (string componentId, string additionalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/components/" + componentId + "/versions" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.component-detail-5+json";
            var content = new StringContent("");
            string componentVersionsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject componentVersionsJObject = JObject.Parse(componentVersionsString);
            List<BlackDuckComponentVersion> componentVersionsList = componentVersionsJObject["items"].ToObject<List<BlackDuckComponentVersion>>();
            return componentVersionsList;
        }



        public string GetAllCodeLocationsJson(string additionalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/codelocations" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.scan-5+json";
            var content = new StringContent("");
            string codeLocationsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            return codeLocationsString;
        }




        public string GetSingleCodeLocationJson(string codeLocationId)
        {
            var fullURL = this._baseUrl + "/api/codelocations/" + codeLocationId;
            var acceptHeader = "application/vnd.blackducksoftware.scan-5+json";
            var content = new StringContent("");
            string codeLocationsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            return codeLocationsString;
        }



        public string GetAllCodeLocationsReturnHTTPResponse(string additionalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/codelocations" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.scan-5+json";
            var content = new StringContent("");
            HttpResponseMessage codeLocationsMessage = this._httpClient.MakeHTTPRequestReturnFullResponseMessage(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            return codeLocationsMessage.ToString();    
        }





        public string GetProjectNameByID(string projectId)
        {
            var fullURL = this._baseUrl + "/api/projects/" + projectId;
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-4+json";
            var content = new StringContent("");
            string projectString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            BlackDuckProject project = JsonConvert.DeserializeObject<BlackDuckProject>(projectString);

            return project.name;
        }




        public List<BlackDuckBOMComponent> GetBOMComponents(string projectId, string versionId, string additionalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/components/" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.bill-of-materials-6+json";
            var content = new StringContent("");
            string componentString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject componentObj = JObject.Parse(componentString);
            List<BlackDuckBOMComponent> componentList = componentObj["items"].ToObject<List<BlackDuckBOMComponent>>();
            return componentList;
        }

        public string GetBOMComponentsJson(string projectId, string versionId, string additionalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/components/" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.bill-of-materials-6+json";
            var content = new StringContent("");
            string componentString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            return componentString;
        }


        public string GetBOMComponenCopyrightJson(BlackDuckBOMComponent bomComponent)
        {
            //Listing Copyrights for a Component Version and Origin. We don't check that origin exists here

            var componentOriginHref = bomComponent.origins[0].origin; // we only look at the first origin here
            var fullCopyrightURL = componentOriginHref + "/copyrights";
            var acceptHeader = "application/vnd.blackducksoftware.copyright-4+json";
            var content = new StringContent("");
            string copyrightsString = this._httpClient.MakeHTTPRequestAsync(fullCopyrightURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;
            return copyrightsString;         
        }




        public BlackDuckKBcomponentOrigin GetKBComponenOrigin(BlackDuckBOMComponent bomComponent)
        {
            //Getting KB component origin from BOM component

            var fullOriginURL = bomComponent.origins[0].origin; // this is the complete KB origin href for the first origin only
            var acceptHeader = "application/vnd.blackducksoftware.component-detail-5+json";
            var content = new StringContent("");
            string originJsonString = this._httpClient.MakeHTTPRequestAsync(fullOriginURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject originObj = JObject.Parse(originJsonString);
            BlackDuckKBcomponentOrigin componentOrigin = originObj.ToObject<BlackDuckKBcomponentOrigin>();
            return componentOrigin;
        }



        public string GetKBComponenOriginCopyrightsJson (BlackDuckKBcomponentOrigin origin)
        {
            //Getting the complete copyrights JSON for KB component origin

            var componentOriginHref = origin._meta.href;

            var fullCopyrightURL = componentOriginHref + "/copyrights"; 
            var acceptHeader = "application/vnd.blackducksoftware.copyright-4+json";
            var content = new StringContent("");

            string copyrightsString = this._httpClient.MakeHTTPRequestAsync(fullCopyrightURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;
            return copyrightsString;
        }





        public string GetComponentsWithHeadersJson(string projectId, string versionId, string additionalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/components/" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.bill-of-materials-6+json";
            var content = new StringContent("");

            string message = this._httpClient.MakeHTTPRequestReturnFullResponseMessage(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result.ToString();
            string componentString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            return message + componentString;
        }


        public string GetProjectVersionNameByID(string projectId, string versionId)
        {
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId;
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-5+json";
            var content = new StringContent("");
            string versionString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            BlackDuckProjectVersion version = JsonConvert.DeserializeObject<BlackDuckProjectVersion>(versionString);

            return version.versionName;
        }



        public List<BlackDuckProjectVersion> GetProjectVersionsFromProjectId(string projectId, string additinalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions" + additinalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-5+json";
            var content = new StringContent("");

            string projectsVersionsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject versionJObject = JObject.Parse(projectsVersionsString);
            List<BlackDuckProjectVersion> versionList = versionJObject["items"].ToObject<List<BlackDuckProjectVersion>>();

            return versionList;
        }


        public List<BlackDuckBOMComponent> ListingBomComponents(string projectId, string versionId, string additinalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/components" + additinalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.bill-of-materials-6+json";
            var content = new StringContent("");

            string componentsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject componentsJObject = JObject.Parse(componentsString);
            List<BlackDuckBOMComponent> componentsList = componentsJObject["items"].ToObject<List<BlackDuckBOMComponent>>();

            return componentsList;
        }


        public int CountBomComponents(string projectId, string versionId)
        {
            var additinalSearchParams = "?offset=0&limit=1"; //Getting only one component because we only want the totalCount property
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/components" + additinalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.bill-of-materials-6+json";
            var content = new StringContent("");

            string componentsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject componentsJObject = JObject.Parse(componentsString);
            string componentsCount = componentsJObject["totalCount"].ToString();

            return int.Parse(componentsCount);
        }

        public int CountVersions(string projectId)
        {
            var additinalSearchParams = "?offset=0&limit=1"; //Getting only one version because we only want the totalCount property
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + additinalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-5+json";
            var content = new StringContent("");

            string versionString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject versionsJObject = JObject.Parse(versionString);
            string componentsCount = versionsJObject["totalCount"].ToString();

            return int.Parse(componentsCount);
        }



        public string GetVersionIdFromProjectNameAndVersionName(string projectName, string projectVersionName) 
        {
            var projectId = this.GetProjectIdFromName(projectName);
            var additinalSearchParams = "?limit=1&q=versionName:" + projectVersionName; // we only want one projectVersionName
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions" + additinalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-5+json";
            var content = new StringContent("");

            string projectsVersionsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject versionJObject = JObject.Parse(projectsVersionsString);
            List<BlackDuckProjectVersion> versionList = versionJObject["items"].ToObject<List<BlackDuckProjectVersion>>();

            if (versionList.Count == 0)
            {
                return projectsVersionsString; 
            }
                  
            else
            {
                BlackDuckProjectVersion version = versionList[0];
                string versionID = version._meta.href.Split('/').Last();
                return versionID;
            }
        }



        public Dictionary<string, string> GetProjectIdVersionIdFromNames(string projectName, string projectVersionName, string additionalSearchParams)
        {           
            
            var projectId = this.GetProjectIdFromName(projectName);
            var additinalSearchParams = "?limit=1&q=versionName:" + projectVersionName; // we only want one projectVersionName
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions" + additinalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.project-detail-5+json";
            var content = new StringContent("");

            string projectsVersionsString = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject versionJObject = JObject.Parse(projectsVersionsString);
            List<BlackDuckProjectVersion> versionList = versionJObject["items"].ToObject<List<BlackDuckProjectVersion>>();

            BlackDuckProjectVersion version = versionList[0]; // we only want one version
            string versionId = version._meta.href.Split('/').Last();


            var projectAndVersion = new Dictionary<string, string>()
            {
                { "projectId", projectId},
                { "versionId",  versionId}

            };

            return projectAndVersion;
        }


        public List<BlackDuckRole> GetRoles(string additionalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/roles" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.user-4+json";
            var content = new StringContent("");

            string rolesJson = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject rolesListObject = JObject.Parse(rolesJson);
            List<BlackDuckRole> rolesList = rolesListObject["items"].ToObject<List<BlackDuckRole>>();
            return rolesList;
        }



        public List<BlackDuckMatchedFileWithComponent> GetBOMMatchedFilesWithComponent(string projectName, string projectVersionName, string additionalSearchParams) 
        {
            /// api-doc/public.html#matched-file-with-component-representation
            /// 

            string projectId = this.GetProjectIdFromName(projectName);
            string versionId = this.GetVersionIdFromProjectNameAndVersionName(projectName, projectVersionName);
            
            var fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/matched-files" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.bill-of-materials-6+json";
            var content = new StringContent("");

            string matchedFilesJson = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject matchedFilesListingJobject = JObject.Parse(matchedFilesJson);  

            if (matchedFilesListingJobject["items"] != null)
            {
                return matchedFilesListingJobject["items"].ToObject<List<BlackDuckMatchedFileWithComponent>>(); 
            }
            else return null;
        }


        public string GetSourceTrees(string projectName, string versionName)
        {
            string projectId = this.GetProjectIdFromName(projectName);
            string versionId = this.GetVersionIdFromProjectNameAndVersionName(projectName, versionName);
            string fullURL = this._baseUrl + "/api/projects/" + projectId + "/versions/" + versionId + "/source-trees";
            var acceptHeader = "";

            var content = new StringContent("");
            string sourceTrees = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            return sourceTrees;
        }


        public string GetCurrentUserJson()
        {
            var fullURL = this._baseUrl + "/api/current-user";
            var acceptHeader = "application/vnd.blackducksoftware.user-4+json";
            var content = new StringContent("");
            string currentUserJson = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;
            return currentUserJson;
        }


        public List<BlackDuckRole> GetUserRoles(string userId, string additionalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/users/" + userId + "/roles" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.user-4+json";
            var content = new StringContent("");
            string userRolesJson = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject rolesListObject = JObject.Parse(userRolesJson);
            List<BlackDuckRole> rolesList = rolesListObject["items"].ToObject<List<BlackDuckRole>>();
            return rolesList;
        }


        public List<BlackDuckUser> GetUsers(string additionalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/users" + additionalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.user-4+json";
            var content = new StringContent("");
            string usersJson = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;

            JObject usersListObject = JObject.Parse(usersJson);
            List<BlackDuckUser> usersList = usersListObject["items"].ToObject<List<BlackDuckUser>>();
            return usersList;
        }

        public string GetUserRolesJson(string userId, string additinalSearchParams)
        {
            var fullURL = this._baseUrl + "/api/users/" + userId+ "/roles" + additinalSearchParams;
            var acceptHeader = "application/vnd.blackducksoftware.user-4+json";
            var content = new StringContent("");
            string userRolesJson = this._httpClient.MakeHTTPRequestAsync(fullURL, this._authorizationBearerString, HttpMethod.Get, acceptHeader, content).Result;
            return userRolesJson;
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
