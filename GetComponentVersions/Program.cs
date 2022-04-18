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


            var _bdurl = new Option<string>(
                "--bdurl",
                description: "REQUIRED: BlackDuck URL"
                );

            var _token = new Option<string>(
                "--token",
                description: "REQUIRED: BD Token"
                );


            var _component = new Option<string>(
                "--component",
                description: "REQUIRED: Component UUID"
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
                _component,
                _notsecure,
                _filename
            };


            // The new command handler API
            // https://github.com/dotnet/command-line-api/issues/1537


            rootCommand.SetHandler(
            (string bdUrl, string token, string component, bool notSecure, string filename) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;

                var offset = 0;
                var limit = 1000;
                var additionalSearchParams = $"?offset={offset}&limit={limit}";


                if (token == null || bdUrl == null || component == null)
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

                Console.WriteLine($"Getting versions for component UUID: {component}");
                Console.WriteLine();

                var columnString = "ComponenUUID;VersionName;VersionUUID";

                if (filename != "")
                {
                    try
                    {
                        Logger.Log(filename, columnString);
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
                    var versions = bdapi.GetComponentVersions(component, additionalSearchParams);

                    while (versions.Count > 0)
                    {
                        foreach (var version in versions)
                        {
                            if (version._meta.href != null)
                            {
                                string versionUUID = version.getVersionID();
                                var logstring = component + ";" + version.versionName + ";" + versionUUID;
                                if (filename != "")
                                {
                                    Logger.Log(filename, logstring);
                                }
                                else Console.WriteLine(logstring);
                            }                            
                        }

                        if (versions.Count < limit)
                        {
                            break;
                        }
                        else
                        {
                            offset = offset + limit;
                            additionalSearchParams = $"?offset={offset}&limit={limit}";
                            versions = bdapi.GetComponentVersions(component, additionalSearchParams);
                        }
                    }
                }

                catch (Exception ex)
                {
                    // Catching Serialization errors
                    Console.WriteLine("\nError:" + ex.Message + " Please verify that you have correct BDurl, token and ComponentUUID ");
                    return;
                }
            },

            _bdurl, _token, _component, _notsecure, _filename);

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
