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
                description: "Write output to file",
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

                Console.WriteLine("Start time: " + DateTime.Now);
                Console.WriteLine("Getting components...");
                Console.WriteLine();

                var columnString = "ProjectName;ProjectVersionName;ComponenName;ComponentVersionName;OriginExternalId;OriginReleasedOn;CopyrightsCount";

                if (filename != "")
                {
                    try
                    {
                        Logger.Log(filename, columnString);
                        Console.WriteLine($"Writing output to file {filename}");
                    }

                    catch (Exception ex)
                    //Catching exception for invalid FilePath input
                    {
                        if (ex is DirectoryNotFoundException || ex is UnauthorizedAccessException)
                        {
                            Console.WriteLine($"\n{ex.Message} or filename not specified");
                            return;
                        }
                    }
                }

                else Console.WriteLine(columnString);


                //try
                //{
                    
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

                //}

                //catch (Exception ex)
                //{
                //    // Catching Serialization errors
                //    Console.WriteLine("\nError:" + ex.Message + " Please verify that you have correct BDurl, token, Project Name or Project Version Name");
                //    return;
                //}

                Console.WriteLine("End time: " + DateTime.Now);
            },

            _bdurl, _token, _projectname, _versionname, _notsecure, _filename);

            return rootCommand.InvokeAsync(args).Result;
        }

        public static void printProjectVersionCopyrights(BlackDuckRestAPI bdapi, string projectName, string projectId, string versionName, string versionId,string filename)
        {           

            var offset = 0;
            var limit = 1000;
            var additionalSearchParams = $"?offset={offset}&limit={limit}";
            

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
                    // Now we are getting into the origin
                    {
                        var copyrightsJson = bdapi.GetBOMComponenCopyrightJson(bomComponent);
                        var origin = bdapi.GetKBComponenOrigin(bomComponent);
                        

                        // updating variables
                        totalCopyrightsString = JObject.Parse(copyrightsJson)["totalCount"].ToString();
                        originExternalId = origin.originId;
                        releasedOn = origin.releasedOn;
                    }


                    string logString = string.Format(projectName + ";" + versionName + ";" + bomComponent.componentName + ";" + bomComponent.componentVersionName + ";" + originExternalId + ";" + releasedOn + ";" + totalCopyrightsString);

                    if (filename != "")
                    {
                        Logger.Log(filename, logString);
                    }
                    else
                    {
                        Console.WriteLine(logString);
                    }
                }

                if (bomComponents.Count < limit)
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
        }
    }
}
