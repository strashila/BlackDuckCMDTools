using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Threading;
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
                 description: "Optional: Project Version Name. If this is not passed, the tool iterates over ALL the versions of the project"
                 );



            var _notsecure = new Option<bool>(
                "--not-secure",
                description: "Trust the certificate of the BD server",
                getDefaultValue: () => false
                );



            var rootCommand = new RootCommand
            {
                _bdurl,
                _token,
                _projectname,
                _versionname,
                _notsecure
                
            };


            // The new command handler API
            // https://github.com/dotnet/command-line-api/issues/1537


            rootCommand.SetHandler(
            (string bdUrl, string token, string projectname, string versionname, bool notSecure) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;

                var offset = 0;
                var limit = 1000; // 1000 versions for project hardcoded
                var additionalSearchParams = $"?offset={offset}&limit={limit}";

                if (token == null || bdUrl == null || projectname == null)
                {
                    Console.WriteLine("Parameters missing, use --help");
                    return;
                }

                if (versionname == null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Are you sure you want to run this script over ALL the versions of the project {projectname}? This may take a while");
                    Console.WriteLine("Type \"Y\" or \"Yes\" to confirm");
                    
                    string confirm = Console.ReadLine();
                    string[] correctAnswers = { "yes", "y" };
                    
                    if (correctAnswers.Contains(confirm.ToLower()) == false)
                    {
                        Console.WriteLine("Choice was not confirmed, exiting");
                        return;
                    }
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
                    string versionName = "";
                    if (versionname != null)
                    {
                        versionName = "Version " + versionname;
                    }

                    Console.WriteLine();
                    Console.WriteLine($"Getting requested codelocations for Project {projectname} {versionName}");
                    Console.WriteLine();

                    var projectId = bdapi.GetProjectIdFromName(projectname);

                    // Not paginating for versions with 1000 limit
                    var versions = bdapi.GetProjectVersionsFromProjectId(projectId, additionalSearchParams);

                    // Creating a dict with all versions and all codeLocations

                    var versionCodeLocationDict = new Dictionary<BlackDuckProjectVersion, List<string>>();
                    var versionCodeLocationJsonArray = new JArray();

                    foreach (var version in versions)
                    {                    
                        
                        // Checking if we're adding code locations for the specific version or for everything

                        if (versionname == null || (versionname != null && versionname == version.versionName))

                        {
                            var versionId = version._meta.href.Split("/").Last();

                            // Not paginating for code locations with 1000 limit as well
                            var codelocationsList = bdapi.GetVersionCodeLocations(projectId, versionId, additionalSearchParams);

                            var codeLocationIdList = new List<string>();

                            JArray codelocationJArray = new JArray();

                            foreach (BlackDuckCodeLocation codelocation in codelocationsList)
                            {
                                var codelocationId = codelocation._meta.href.Split("/").Last();
                                codeLocationIdList.Add(codelocationId);
                                codelocationJArray.Add(codelocationId);
                            }

                            versionCodeLocationDict.Add(version, codeLocationIdList);

                            // Adding all the relevant code locations to Json for console output

                            versionCodeLocationJsonArray.Add(
                                new JObject
                                {
                                {"versionName", version.versionName },
                                {"url", version._meta.href},
                                {"codeLocations", codelocationJArray}
                                });
                        }
                    }


                    // Writing all this precious info from the dict to console output just in case

                    Console.WriteLine(new JObject(new JProperty("versions", versionCodeLocationJsonArray)).ToString());


                    foreach (var versionClPair in versionCodeLocationDict)
                    {
                        // Iterating over our dict and unmapping all codelocations

                        Console.WriteLine();
                        Console.WriteLine($"Unmmapping codelocations for project {projectname} version {versionClPair.Key.versionName}...");
                        Console.WriteLine();
    
                        var codelocationsIdList = versionClPair.Value;
                        foreach (var codelocationId in codelocationsIdList)
                        {
                            var unmappedCodeloc = bdapi.UpdateCodeLocationVersionMapping(codelocationId, "");
                            Console.WriteLine(codelocationId);
                        }
                    }


                    // Iterating over our dict and mapping all codelocations back

                    
                    Console.WriteLine();
                    Console.WriteLine();

                    if (versionCodeLocationDict.Count == 0)
                    {
                        Console.WriteLine("No code locations found for project version");
                        return; // nothing to do, non existing project version
                    }

                    Console.WriteLine("Re-mapping codelocations...");
                    Console.WriteLine();

                    Thread.Sleep(5000);                                   

                    foreach (var versionClPair in versionCodeLocationDict)
                    {
                        Console.WriteLine($"Re-Mapping codelocations for project {projectname} version {versionClPair.Key.versionName}...");
                        Console.WriteLine();

                        var codelocationIdList = versionClPair.Value;
                        foreach (var codelocationId in codelocationIdList)
                        {
                            var mappedCodeloc = bdapi.UpdateCodeLocationVersionMapping(codelocationId, versionClPair.Key._meta.href);
                            Console.WriteLine(codelocationId);
                        }
                    }
                }


                catch (Exception ex)
                {
                    // Catching Serialization or other errors
                    Console.WriteLine("\nError:" + ex.Message + " Please verify that you have correct BDurl, token, and Project Name");
                    return;
                }
            },

            _bdurl, _token, _projectname, _versionname, _notsecure);

            return rootCommand.InvokeAsync(args).Result;
        }

       
    }
}
