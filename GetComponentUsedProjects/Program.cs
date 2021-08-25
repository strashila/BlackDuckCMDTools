using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net.Http;
using BlackDuckCMDTools;
using Newtonsoft.Json;

namespace GetComponentsUUID
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


            var component = new Option<string>("--component");
            component.Description = "REQUIRED: Component name";


            var filePath = new Option<string>("--filepath");
            filePath.AddAlias("-f");
            filePath.Description = "Output filepath. If not present in options, the tool will print the output to console";

            var notSecure = new Option<bool>("--not-secure");
            
            notSecure.Description = "Disable secure connection to BlackDuck server";



            var rootCommand = new RootCommand
            {
                bdUrl,
                token,
                component,                
                filePath,
                notSecure
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string, string, bool>((bdUrl, token, component, filePath, notSecure) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;




                var additionalSearchParams = "?offset=0&limit=500";

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




                var columnString = "ComponentName|ProjectName|Version";

                if (filePath != "")
                {
                    try
                    {
                        Logger.Log(filePath, columnString);
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

                try
                {
                    List<BlackDuckProject> allProjects = bdapi.GetAllProjects(additionalSearchParams);

                    foreach (BlackDuckProject project in allProjects)
                    {
                        string projectId = project._meta.href.Split('/').Last();
                        List<BlackDuckProjectVersion> versions;
                        try
                        {
                            versions = bdapi.GetProjectVersionsFromProjectId(projectId, additionalSearchParams);
                        }
                        catch (Exception ex)
                        {
                            versions = null;
                        }

                        if (versions != null)
                        {
                            foreach (var version in versions)
                            {
                                List<BlackDuckBOMComponent> components;
                                var versionId = version._meta.href.Split('/').Last();
                                try
                                {
                                    components = bdapi.GetComponentsFromProjectIdVersionId(projectId, versionId, additionalSearchParams);

                                }
                                catch (Exception ex)
                                {
                                    components = null;
                                }

                                if (components != null)
                                {
                                    foreach (var singleComponent in components)
                                    {
                                        var name = singleComponent.componentName.ToLower();
                                        if (name.Contains(component.ToLower()))
                                        {
                                            string logString = singleComponent.componentName + "|" + project.name + "|" + version.versionName;

                                            if (filePath != "")
                                            {
                                                Logger.Log(filePath, logString);

                                            }
                                            else
                                            {
                                                Console.WriteLine(logString);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message + " Please verify that you have correct BDurl and token " );
                }

                Console.WriteLine($"\nFinished logging to file {filePath}");
            });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;

        }
    }
}
