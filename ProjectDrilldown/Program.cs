using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net.Http;
using BlackDuckCMDTools;
using Newtonsoft.Json;

namespace GetAllProjectsWithVersionCount
{
    class Program
    {
        static int Main(string[] args)
        {

            /// Uses the beta System.CommandLine library to parse command line arguments 
            /// https://github.com/dotnet/command-line-api/blob/main/docs/Your-first-app-with-System-CommandLine.md


            var bdUrl = new Option<string>("--bdurl");
            bdUrl.Description = "REQUIRED: BlackDuck URL";

            var token = new Option<string>("--token");
            token.Description = "REQUIRED: BD Token";


            var projectName = new Option<string>("--projectname");
            //secureConnection.SetDefaultValue(false);
            projectName.Description = "Project Name";


            var notSecure = new Option<bool>("--not-secure");
            //secureConnection.SetDefaultValue(false);
            notSecure.Description = "Disable secure connection to BlackDuck server";


            var filePath = new Option<string>("--filepath");
            filePath.AddAlias("-f");
            filePath.Description = "Output filepath. If not present in options, the tool will print the output to a default file";

            var rootCommand = new RootCommand
            {
                bdUrl,
                token,
                projectName,
                notSecure,
                filePath,
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string, bool, string>((bdUrl, token, projectName, notSecure, filePath) =>
            {
                
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;

                var defaultFileName = projectName + "_drilldown.csv";

                var offset = 0;
                var limit = 1000;

                var additionalSearchParams = $"?offset={offset}&limit={limit}";

                if (token == "" || bdUrl == "" || projectName == "")
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

               
                //Checking valid FilePath input
                var columnString = "VersionNum;VersionName;VersionURL;CodelocationName;CodelocationID;ScanSize;MatchCount";
                if (filePath != "")
                {
                    try
                    {
                        Logger.Log(filePath, columnString);
                    }

                    catch (Exception ex)

                    {
                        if (ex is DirectoryNotFoundException || ex is UnauthorizedAccessException)
                        {
                            Console.WriteLine($"\n{ex.Message} or filename not specified");
                            return;
                        }
                    }
                }
                else filePath = defaultFileName;


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


                try
                {
                    Console.WriteLine($"Drilldown of project {projectName}");
                    string projectId = bdapi.GetProjectIdFromName(projectName);



                    List<BlackDuckProjectVersion> versions = bdapi.GetProjectVersionsFromProjectId(projectId, additionalSearchParams);

                    Console.WriteLine("Getting versions...");
                    var versionUrls = new List<string>();

                    foreach (var version in versions)
                    {
                        versionUrls.Add(version._meta.href);
                    }

                    Console.WriteLine("Getting Codelocations...");
                    var codelocations = bdapi.GetAllCodeLocations(additionalSearchParams);

                    Console.WriteLine("Building project file...");

                    while (codelocations.Count > 0)
                    {
                        foreach (BlackDuckCodeLocation codelocation in codelocations)
                        {
                            if (versionUrls.Contains(codelocation.mappedProjectVersion))
                            {
                                var versionName = bdapi.GetVersionNameByID(projectId, codelocation.mappedProjectVersion.Split('/').Last());
                                var codeLocationId = codelocation._meta.href.Split('/').Last();
                                var latestScanSummary = bdapi.GetLatestScanSummary(codeLocationId);

                                Logger.Log(filePath, $"{versionUrls.IndexOf(codelocation.mappedProjectVersion)};{versionName};{codelocation.mappedProjectVersion};{codelocation.name};{codeLocationId};{codelocation.scanSize};{latestScanSummary.matchCount}");
                            }
                        }
                        offset += limit;
                        codelocations = bdapi.GetAllCodeLocations(additionalSearchParams);
                    }

                    Console.WriteLine();
                    Console.WriteLine();
                }

                // Catching Serialization errors
                catch (Exception ex)
                {                    
                    Console.WriteLine("\nError:" + ex.Message + " Please verify that you have correct BDurl and token ");
                    return;
                }

                Console.WriteLine($"\nFinished logging to file {filePath}");
            });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;

        }
    }
}
