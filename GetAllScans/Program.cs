﻿using System;
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

            var notSecure = new Option<bool>("--not-secure");
            //secureConnection.SetDefaultValue(false);
            notSecure.Description = "Disable secure connection to BlackDuck server";

            var filePath = new Option<string>("--filepath");
            filePath.AddAlias("-f");
            filePath.Description = "Output filepath. If not present in options, the tool will print the output to console";

            var rootCommand = new RootCommand
            {
                bdUrl,
                token,
                notSecure,
                filePath,
            };

            rootCommand.Handler = CommandHandler.Create<string, string, bool, string>((bdUrl, token, notSecure, filePath) =>
            {
                var start = DateTime.Now;
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;

                var offset = 0;
                var limit = 1000;

                var additionalSearchParams = $"?offset={offset}&limit={limit}";

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


                //Checking valid FilePath input
                var columnString = "CodeLocation_name|Mapped_project_version|Scan_size";
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
                    var codeLocations = bdapi.GetAllCodeLocations(additionalSearchParams);
                    var mappedScansCounter = 0;
                    var unmappedScansCounter = 0;


                    while (codeLocations.Count > 0)
                    {
                        
                        foreach (var codelocation in codeLocations)
                        {
                            var mappedVersion = "";
                            if (codelocation.mappedProjectVersion == null)
                            {
                                mappedVersion = "UNMAPPED";
                                unmappedScansCounter++;
                            }

                            else
                            {
                                mappedVersion = codelocation.mappedProjectVersion;
                                mappedScansCounter++;

                            }

                            var logString = $"{codelocation.name} | {mappedVersion} | {codelocation.scanSize}";
                            if (filePath != "")
                            {
                                Logger.Log(filePath, logString);
                            }
                            else
                            {
                                Console.WriteLine(logString);
                            }
                        }

                        offset = offset + limit;
                        additionalSearchParams = $"?offset={offset}&limit={limit}";
                        codeLocations = bdapi.GetAllCodeLocations(additionalSearchParams);
                    }

                    Console.WriteLine();
                    Console.WriteLine($"There are {mappedScansCounter} mapped scans");
                    Console.WriteLine($"There are {unmappedScansCounter} UNMAPPED scans");
                    Console.WriteLine();
                }

                catch (Exception ex)
                {
                    // Catching Serialization errors
                    Console.WriteLine("\nError:" + ex.Message + " Please verify that you have correct BDurl and token ");
                    return;
                }

                if (filePath != "")
                {
                    Console.WriteLine($"\nFinished logging to file {filePath}");
                }

                var end = DateTime.Now;
                //Console.WriteLine($"Started {start}, finished {end}");
            });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;

        }
    }
}
