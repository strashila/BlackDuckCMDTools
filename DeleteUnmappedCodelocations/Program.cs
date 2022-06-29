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


            var bdUrl = new Option<string>("--bdurl");
            bdUrl.Description = "REQUIRED: BlackDuck URL";

            var token = new Option<string>("--token");
            token.Description = "REQUIRED: BD Token";


            var filename = new Option<string>("--filename");
            filename.Description = "Optional: filename";

            var notSecure = new Option<bool>("--not-secure");
            //secureConnection.SetDefaultValue(false);
            notSecure.Description = "Disable secure connection to BlackDuck server";


            var rootCommand = new RootCommand
            {
                bdUrl,
                token,
                notSecure,
                filename
            };

            rootCommand.Handler = CommandHandler.Create<string, string, bool, string>((bdUrl, token, notSecure, filename) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;

                var offset = 0;
                var limit = 1000;
                long totalSize = 0;

                var additionalSearchParams = $"?offset={offset}&limit={limit}";

                if (filename == null || filename == "")
                {
                    filename = "deletedCodelocations.json";
                }

                
                if (token == "" || bdUrl == "")
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
                    Console.WriteLine("Getting codelocations...");
                    var codeLocations = bdapi.GetAllCodeLocations(additionalSearchParams);
                    var unmappedCodelocationsCount = 0;
                    var unmappedCodelocationsList = new List<BlackDuckCodeLocation>();

                    Console.WriteLine();
                    Console.WriteLine("Listing Unmapped codelocations...");

                    var codeLocationJArray = new JArray();
                    
                    while (codeLocations.Count > 0)
                    {
                        foreach (var codelocation in codeLocations)
                        {
                            if (codelocation.mappedProjectVersion == null)
                            {
                                var codeLocationId = codelocation._meta.href.Split("/").Last();

                                var codelocationsJObject = new JObject
                                {
                                  {"codeLocationName", codelocation.name},
                                  {"codeLocationId", codeLocationId}
                                };
                                codeLocationJArray.Add(codelocationsJObject);

                                Console.WriteLine($"{codelocation.name} {codeLocationId}");

                                totalSize += codelocation.scanSize;
                                unmappedCodelocationsCount++;                                
                                unmappedCodelocationsList.Add(codelocation);
                             }
                        }


                        if (codeLocations.Count < limit)
                        {
                            break;
                        }
                        else
                        {
                            offset = offset + limit;
                            additionalSearchParams = $"?offset={offset}&limit={limit}";
                            codeLocations = bdapi.GetAllCodeLocations(additionalSearchParams);
                        }
                    }

                    if (unmappedCodelocationsCount == 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("No Unmapped Codelocations, nothing to delete");
                        return;
                    }

                    Console.WriteLine();
                    Console.WriteLine("Important: You are about to delete all the listed above Unmapped Codelocations (scans) for your instance. To proceed type Yes");
                    string concent = Console.ReadLine();

                    if (concent.ToLower() != "yes")
                    {
                        return;
                    }

                    Console.WriteLine("Deleting codelocations...");

                    foreach (BlackDuckCodeLocation codelocation in unmappedCodelocationsList)
                    {
                        var codeLocationId = codelocation._meta.href.Split("/").Last();
                        Console.WriteLine($"Deleting codeLocation {codelocation.name} {codeLocationId} {bdapi.DeleteCodelocation(codeLocationId)}");
                    }

                    Console.WriteLine();
                    Console.WriteLine($"{unmappedCodelocationsCount} unmapped codelocations deleted. Total scan size: {totalSize}");
                    Console.WriteLine();


                    try
                    {
                        Logger.Log(filename, codeLocationJArray.ToString());
                        Console.WriteLine($"Writing deleted codelocations to file {filename}");
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
                }

                catch (Exception ex)
                {
                    // Catching Serialization errors
                    Console.WriteLine("\nError:" + ex.Message + " Please verify that you have correct BDurl and token ");
                    return;
                }                
            });
            

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;

        }
    }
}
