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
                _notsecure,
                _filename
            };


            // The new command handler API
            // https://github.com/dotnet/command-line-api/issues/1537


            rootCommand.SetHandler(
            (string bdUrl, string token, bool notSecure, string filename) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;

                var offset = 0;
                var limit = 1000;
                var componentAdditionalSearchParams = $"?offset={offset}&limit={limit}";


                if (token == null || bdUrl == null)
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
                        bdapi = new BlackDuckCMDTools.BlackDuckRestAPI(bdUrl, token, false);
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
                        bdapi = new BlackDuckCMDTools.BlackDuckRestAPI(bdUrl, token, true);
                        bdapi.SetHttpClientLocalTimeout(1200);
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

                Console.WriteLine($"Getting Data...");
                Console.WriteLine();
                var startTime = DateTime.Now; 

                var columnString = "ProjectName;ProjectVersionName;ComponenName;ComponentVersionName;OriginExternalId;CopyrightsCount";

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


                try
                {
                    // Hardcoding limit of 1000 projects, and 1000 versions per project just because. TODO - create project/projectVersion pagination
                    
                    string hardcodedAddiditonalSearchParams = "?offset=0&limit=1000";                    

                    var projects = bdapi.GetAllProjects("?offset=0&limit=1000");

                    foreach (BlackDuckProject proj in projects)
                    {
                        string projectId = proj._meta.href.Split("/").Last();
                        var versions = bdapi.GetProjectVersionsFromProjectId(projectId, hardcodedAddiditonalSearchParams);

                        foreach (var version in versions)
                        {
                            string versionId = version._meta.href.Split("/").Last();

                            var bomComponents = bdapi.GetBOMComponents(projectId, versionId, componentAdditionalSearchParams);

                            while (bomComponents.Count > 0)
                            {
                                foreach (var bomComponent in bomComponents)
                                {
                                    var totalCopyrightsString = "";
                                    var originExternalId = "No origin specified";

                                    if (bomComponent.origins != null && bomComponent.origins.Count > 0)
                                    {
                                        string copyrightsJson = bdapi.GetBOMComponenCopyrightJson(bomComponent);
                                        totalCopyrightsString = JObject.Parse(copyrightsJson)["totalCount"].ToString();
                                        originExternalId = bomComponent.origins[0].externalId;
                                    }


                                    string logString = proj.name + ";" + version.versionName + ";" + bomComponent.componentName + ";" + bomComponent.componentVersionName + ";" + originExternalId + ";" + totalCopyrightsString;

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
                                    componentAdditionalSearchParams = $"?offset={offset}&limit={limit}";
                                    bomComponents = bdapi.GetBOMComponents(projectId, versionId, componentAdditionalSearchParams);
                                }
                            }
                        }
                    }        


                }

                catch (Exception ex)
                {
                    // Catching Serialization errors
                    Console.WriteLine("\nError:" + ex.Message + " Please verify that you have correct BDurl, token, Project Name and Project Version Name");
                    return;
                }

                Console.WriteLine();
                var endTime = DateTime.Now;
                var timespan = endTime.Subtract(startTime);
                Console.WriteLine($"Script start time: {startTime}, Script end time: {endTime}, Script total run: {timespan.Hours} hours {timespan.Minutes} minutes {timespan.Seconds} seconds ");
            },

            _bdurl, _token, _notsecure, _filename);

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
