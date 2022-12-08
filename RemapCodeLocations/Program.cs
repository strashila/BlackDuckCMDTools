using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net.Http;
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


            var _notsecure = new Option<bool>(
                "--not-secure",
                description: "Trust the certificate of the BD server",
                getDefaultValue: () => false
                );


            var _filename = new Option<string>(
                "--filename",
                description: "Log all Project Codelocations to a json file. If no filename is specified a json will be created in the run directory"                
                );
        


            var rootCommand = new RootCommand
            {
                _bdurl,
                _token,
                _projectname,
                _notsecure,
                _filename
            };


            // The new command handler API
            // https://github.com/dotnet/command-line-api/issues/1537


            rootCommand.SetHandler(
            (string bdUrl, string token, string projectname, bool notSecure, string filename) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;

                var offset = 0;
                var limit = 1000;
                var additionalSearchParams = $"?offset={offset}&limit={limit}";

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
                    Console.WriteLine($"Getting codelocations for project \"{projectname}\"...");
                    var projectId = bdapi.GetProjectIdFromName(projectname);

                    // Not paginating for versions with 1000 limit
                    var versions = bdapi.GetProjectVersionsFromProjectId(projectId, additionalSearchParams);

                    // Creating a dict with all versions and all codeLocations

                    var versionCodeLocationDict = new Dictionary<string, List<string>>();
                    var versionCodeLocationJsonArray = new JArray();

                    foreach (var version in versions)
                    {
                        var fullVersionUrl = version._meta.href;
                        var versionId = fullVersionUrl.Split("/").Last();

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

                        versionCodeLocationDict.Add(fullVersionUrl, codeLocationIdList);

                        versionCodeLocationJsonArray.Add(
                            new JObject
                            {
                                {"versionName", version.versionName },
                                {"url", fullVersionUrl},
                                {"codeLocations", codelocationJArray}
                            });
                    }

                    var versionCodeLocationJsonObject = new JObject(new JProperty("versions", versionCodeLocationJsonArray));

                    // Writing all this precious info from the dict to JSON just in case

                    var defaultLogFilename = projectname + "_codelocations.json";

                    if (filename != "" && filename != null)
                    {
                        defaultLogFilename = filename;
                    }

                    try
                    {
                        Logger.Log(defaultLogFilename, versionCodeLocationJsonObject.ToString());
                        Console.WriteLine($"Writing codelocations to file {defaultLogFilename}");
                    }

                    catch (Exception ex)
                    //Catching exception for invalid FilePath input
                    {
                        if (ex is DirectoryNotFoundException || ex is UnauthorizedAccessException)
                        {
                            Console.WriteLine($"\n{ex.Message}");
                            return;
                        }
                    }



                    // Iterating over our dict and unmapping all codelocations

                    Console.WriteLine($"Unmmapping codelocations for project \"{projectname}\"...");

                    foreach (var versionClPair in versionCodeLocationDict)
                    {
                        var codelocationsIdList = versionClPair.Value;
                        foreach (var codelocationId in codelocationsIdList)
                        {
                            var unmappedCodeloc = bdapi.UpdateCodeLocation(codelocationId, "");
                        }
                    }


                    // Iterating over our dict and mapping all codelocations back

                    Console.WriteLine("Re-mapping codelocation...");

                    Thread.Sleep(5000);                                   

                    foreach (var versionClPair in versionCodeLocationDict)
                    {
                        var codelocationIdList = versionClPair.Value;
                        foreach (var codelocationId in codelocationIdList)
                        {
                            var mappedCodeloc = bdapi.UpdateCodeLocation(codelocationId, versionClPair.Key);
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

            _bdurl, _token, _projectname, _notsecure, _filename);

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
