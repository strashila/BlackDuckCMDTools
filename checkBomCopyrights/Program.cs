using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net.Http;
using BlackDuckCMDTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GetAllProjectsWithVersionCount
{
    class Program
    {
        static int Main(string[] args)
        {
            /// Uses the beta System.CommandLine library to parse command line arguments 
            /// https://github.com/dotnet/command-line-api/blob/main/docs/Your-first-app-with-System-CommandLine.md


            var _bdurl = new Option<string>(
                "--bdurl",
                description: "REQUIRED: BlackDuck URL"
                );

            var _token = new Option<string>(
                "--token",
                description: "REQUIRED: BD Token"
                );

            var _projectname = new Option<string>(
                "--projectname",
                description: "REQUIRED: Project Name"
                );

            var _versionname = new Option<string>(
                "--versionname",
                description: "Project Version name. If not specified the tool will iterate over all the versions of the project"
                );

            var _notsecure = new Option<bool>(
                "--not-secure",
                description: "Trust the certificate of the BD server",
                getDefaultValue: () => false
                );

            var _filename = new Option<string>(
                "--filename",
                description: "Write output to file. If not specified a default file will be created in the run directory, {projectName}_copyrights.txt",
                getDefaultValue: () => ""
                );

            _filename.AddAlias("-f");


            var rootCommand = new RootCommand
            {
                _bdurl,
                _token,
                _projectname,
                _versionname,
                _notsecure,
                _filename
            };


            // The new command handler API
            // https://github.com/dotnet/command-line-api/issues/1537


            rootCommand.SetHandler(
            (string bdUrl, string token, string projectname, string versionname, bool notSecure, string filename) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;
                string defaultFileName = projectname + "_copyrights.txt";

                if (token == null || bdUrl == null || projectname == null)
                {
                    Console.WriteLine("Parameters missing, use --help");
                    return;
                }

                else
                {
                    if (bdUrl.LastIndexOf("/") == bdUrl.Length - 1) // URL ends with "/"
                    {
                        bdUrl = bdUrl.Remove(bdUrl.LastIndexOf("/"));
                    }
                }

                /// Trying to create connection to the API both secure and not-secure methods (trusting all SSL certificates or not).
                /// Catching errors both times

                if (notSecure)
                {
                    try
                    {
                        bdapi = new BlackDuckCMDTools.BlackDuckRestAPI(bdUrl, token, false, 7200);  // Setting 2 hours timeout in the constructor overload     
                    }

                    catch (Exception ex)
                    {
                        // catching AuthenticationException or HttpRequestException
                        if (ex is System.AggregateException || ex is HttpRequestException || ex is System.Net.Sockets.SocketException || ex is System.Security.Authentication.AuthenticationException)
                        {
                            Console.WriteLine($"\nError: {ex.Message}");
                        }
                        return;
                    }
                }

                else
                {
                    try
                    {
                        bdapi = new BlackDuckCMDTools.BlackDuckRestAPI(bdUrl, token, true, 7200);
                    }

                    catch (Exception ex)
                    {
                        // catching AuthenticationException or HttpRequestException

                        if (ex is System.AggregateException || ex is HttpRequestException || ex is System.Net.Sockets.SocketException || ex is System.Security.Authentication.AuthenticationException)
                        {
                            Console.WriteLine($"\nError: {ex.Message}");
                        }
                        return;
                    }
                }

                Console.WriteLine($"\nStart time: {DateTime.Now}\n");
                Console.WriteLine("Getting components...\n");                

                var columnString = "ProjectName;ProjectVersionName;ComponenName;ComponentVersionName;OriginExternalId;OriginReleasedOn;CopyrightsCount";

                if (filename == "" || filename == null)                
                {
                    // Not writing anything to console, file output only
                    filename = defaultFileName;                   
                }

                Console.WriteLine($"Writing output to file {filename}");

                try
                {
                    using (StreamWriter sw = new StreamWriter(@filename))
                    {
                        sw.WriteLine(columnString);
                    }
                }

                catch (Exception ex)
                {
                    if (ex is DirectoryNotFoundException || ex is UnauthorizedAccessException)
                    {
                        Console.WriteLine($"\nError: {ex.Message}");
                        return;
                    }
                }


                try
                {
                    // We are working up to 1000 versions per project, no pagination

                    var offset = 0;
                    var limit = 1000;
                    var additionalSearchParams = $"?offset={offset}&limit={limit}";

                    var projectId = bdapi.GetProjectIdFromName(projectname);

                    if (versionname == null)
                    // if no version is specified we will iterate over all the versions of the project
                    {
                        var versions = bdapi.GetProjectVersionsFromProjectId(projectId, additionalSearchParams);

                        foreach (var version in versions)
                        {
                            var versionId = version._meta.href.Split("/").Last();
                            printProjectVersionCopyrights(bdapi, projectname, projectId, version.versionName, versionId, filename);
                            bdapi.RefreshBearerToken();
                        }
                    }
                    else if (versionname != null)
                    {
                        var projectVersionId = bdapi.GetVersionIdFromProjectNameAndVersionName(projectname, versionname);
                        printProjectVersionCopyrights(bdapi, projectname, projectId, versionname, projectVersionId, filename);
                    }
                }

                catch (Exception ex)
                {
                   // Catching ALL exceptions
                   Console.WriteLine($"\nError: { ex.Message} \nPlease make sure that you have the correct BlackDuck URL, Token, Project Name and Version Name");
                   return;                    
                }

                Console.WriteLine($"\nEnd time: {DateTime.Now}\n");
            },

            _bdurl, _token, _projectname, _versionname, _notsecure, _filename);

            return rootCommand.InvokeAsync(args).Result;
        }

        public static void printProjectVersionCopyrights(BlackDuckRestAPI bdapi, string projectName, string projectId, string versionName, string versionId, string filePath)
        {
            var offset = 0;
            var limit = 1000;
            var additionalSearchParams = $"?offset={offset}&limit={limit}";
            StreamWriter sw = new StreamWriter(@filePath, true);
            
            var bomComponents = bdapi.GetBOMComponents(projectId, versionId, additionalSearchParams);

            while (bomComponents.Count > 0)
            {
                foreach (var bomComponent in bomComponents)
                {
                    var totalCopyrightsString = "";                    
                    var originExternalId = "No origin specified";
                    DateTime? releasedOn = null;

                    if (bomComponent.origins != null && bomComponent.origins.Count > 0)                    
                    // Copyrights are a function of component origin. No origin - no copyrights                    
                    {
                        var copyrightsJson = bdapi.GetBOMComponenCopyrightJson(bomComponent);
                        var origin = bdapi.GetKBComponenOrigin(bomComponent);
                        
                        // updating variables
                        totalCopyrightsString = JObject.Parse(copyrightsJson)["totalCount"].ToString();
                        originExternalId = origin.originId;
                        releasedOn = origin.releasedOn;
                    }

                    string logString = string.Format(projectName + ";" + versionName + ";" + bomComponent.componentName + ";" + bomComponent.componentVersionName + ";" + originExternalId + ";" + releasedOn + ";" + totalCopyrightsString);

                    // Not writing to console anymore, only to file
                    sw.WriteLine(logString);
                }

                if (bomComponents.Count < limit) // checking if this is the last iteration
                {
                    break;
                }

                else
                {
                    offset = offset + limit;
                    additionalSearchParams = $"?offset={offset}&limit={limit}";
                    bomComponents = bdapi.GetBOMComponents(projectId, versionId, additionalSearchParams);
                }
            }

            sw.Close(); // Closing the file, disposing of the StreamWriter
        }
    }
}
