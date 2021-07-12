using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net.Http;
using BlackDuckCMDTools;
using Newtonsoft.Json;

namespace BlackDuckGetBomMatchedFiles
{
    class Program
    {
        static int Main(string[] args)
        {

            /// Uses the beta System.CommandLine library to parse command line arguments 
            /// https://github.com/dotnet/command-line-api/blob/main/docs/Your-first-app-with-System-CommandLine.md


            var token = new Option<string>("--token");
            token.Description = "REQUIRED: BD Token";

            var bdUrl = new Option<string>("--bdurl");
            bdUrl.Description = "REQUIRED: BlackDuck URL";

            var projectName = new Option<string>("--projectname");
            projectName.Description = "REQUIRED: Project name";

            var versioNname = new Option<string>("--versionname");
            versioNname.Description = "REQUIRED: Version name";

            var notSecure = new Option<bool>("--not-secure");
            //secureConnection.SetDefaultValue(false);
            notSecure.Description = "Disable secure connection to BlackDuck server";


            var filePath = new Option<string>("--filepath");
            filePath.AddAlias("-f");
            filePath.Description = "Output filepath. If not present in options, the tool will print the output to console";


            var filter = new Option<string>("--filter");
            filter.Description = "Supported Filters: see list here: /api-doc/public.html#_listing_bom_components";

            var rootCommand = new RootCommand
            {
                token,
                bdUrl,
                projectName,
                versioNname,
                notSecure,
                filePath,
                filter
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string, string, bool, string, string>((token, bdUrl, projectName, versionName, notSecure, filePath, filter) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;
                List<BlackDuckBOMComponent> components;

                var additionalSearchParams = "?offset=0&limit=5000";

                if (token == "" || bdUrl == "" || projectName == "")
                {
                    Console.WriteLine("Parameters missing, use --help");
                    return;
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
                            Console.WriteLine($"Error: {ex.Message}");
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
                            Console.WriteLine($"Error: {ex.Message}");
                        }
                        return;
                    }
                }

                if (filter != "")
                {
                    additionalSearchParams += "&" + filter;
                }
                

                try
                {
                    // Getting the components
                    components = bdapi.GetBOMComponentsFromProjectVersion(projectName, versionName, additionalSearchParams);
                }
                catch (Newtonsoft.Json.JsonReaderException ex)
                {
                    // Catching Serialization errors
                    Console.WriteLine("Error: Please check that you have correct Project Name, Version Name and Bearer Token with appropriate permissions");
                    return;
                }


                var columnString = "ComponentName;UUID";

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
                            Console.WriteLine($"{ex.Message} or filename not specified");
                            return;
                        }
                    }
                    

                }


                foreach (BlackDuckBOMComponent component in components)
                {
                    string uuid = bdapi.ParseComponentId(component.component);              

                    string logString = component.componentName+ ";" + uuid;

                    if (filePath != "")
                    {
                        Logger.Log(filePath, logString);

                    }
                    else
                    {
                        Console.WriteLine(logString);
                    }
                }
                Console.WriteLine($"Finished logging to file {filePath}");
            });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;

        }
    }
}
