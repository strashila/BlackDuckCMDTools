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

            var notSecure = new Option<bool>("--not-secure");
            //secureConnection.SetDefaultValue(false);
            notSecure.Description = "Disable secure connection to BlackDuck server";


            var rootCommand = new RootCommand
            {
                bdUrl,
                token,
                notSecure
            };

            rootCommand.Handler = CommandHandler.Create<string, string, bool>((bdUrl, token, notSecure) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;

                var additionalSearchParams = "?offset=0&limit=1000";

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
                    Console.WriteLine();
                    Console.WriteLine("Warning - you are about to delete all versions with no scans associated with them from your instance. To continue type \"Yes\"");
                    string concent = Console.ReadLine();
                    if (concent.ToLower() != "yes")
                    {
                        return;
                    }

                    Console.WriteLine("Getting projects...");
                    var projects = bdapi.GetAllProjects(additionalSearchParams);
                    var allVersionsList = new List<(string, string)>();

                    foreach (var project in projects)
                    {
                        var projectId = project._meta.href.Split('/').Last();
                        var versions = bdapi.GetProjectVersionsFromProjectId(projectId, additionalSearchParams);
                        foreach (var version in versions)
                        {
                            var versionId = version._meta.href.Split('/').Last();
                            (string, string) projectVersionComboTuple = (projectId, versionId);
                            allVersionsList.Add(projectVersionComboTuple);
                        }
                    }

                    Console.WriteLine("Getting scans...");
                    var codelocations = bdapi.GetAllCodeLocations(additionalSearchParams);
                    foreach (var codelocation in codelocations)
                    {
                        if (codelocation.mappedProjectVersion != null)
                        {

                            var str = codelocation.mappedProjectVersion.Split('/');
                            var projectId = str[str.Length - 3];
                            var versionId = str.Last();
                            var mappedVersionTuple = (projectId, versionId);
                            allVersionsList.Remove(mappedVersionTuple);
                        }
                    }

                    Console.WriteLine("Deleting versions with 0 scans...");

                    foreach (var emptyVersionCombo in allVersionsList)
                    {
                        var projectId = emptyVersionCombo.Item1;
                        var versionId = emptyVersionCombo.Item2;

                        Console.WriteLine($"Project {bdapi.GetProjectNameByID(projectId)} version {bdapi.GetProjectVersionNameByID(projectId, versionId)} deleted", bdapi.DeleteProjectVersionByProjectIdVersionId(projectId, versionId));

                        var versions = bdapi.GetProjectVersionsFromProjectId(projectId, additionalSearchParams);
                        if (versions.Count == 1 && versions[0].versionName == "unnamed")
                        {

                            Console.WriteLine($"Project {bdapi.GetProjectNameByID(projectId)} was deleted because it had 0 versions", bdapi.DeleteProjectByProjectId(projectId));
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
