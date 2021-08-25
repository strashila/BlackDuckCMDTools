using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net.Http;
using BlackDuckCMDTools;
using Newtonsoft.Json;

namespace GetBomMatchedFiles
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
            filter.Description = "Supported Filters: [bomMatchType]";

            var rootCommand = new RootCommand
            {
                bdUrl,
                token,
                projectName,
                versioNname,
                notSecure,
                filePath,
                filter
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string, string, bool, string, string>((bdUrl, token, projectName, versionName, notSecure, filePath, filter) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;
                List<BlackDuckMatchedFileWithComponent> matchedFiles;


                var additionalSearchParams = "?offset=0&limit=500";

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

                if (filter != "")
                {
                    additionalSearchParams += "&" + filter;
                }

                try
                {
                    matchedFiles = bdapi.GetBOMMatchedFilesWithComponent(projectName, versionName, additionalSearchParams);
                }
                catch (Newtonsoft.Json.JsonReaderException ex)
                {
                    // Catching Serialization errors
                    Console.WriteLine("\nError: Please check that you have correct ProjectName, VersionName and Token with appropriate permissions");
                    return;
                }


                var columnString = "uri_or_declared_component_path;matchType;componentId";

                if (filePath != "")
                {
                    try
                    {
                        Logger.Log(filePath, columnString);
                    }

                    catch (Exception ex)
                    //Catching exception for invalid FilePath
                    {
                        if (ex is DirectoryNotFoundException || ex is UnauthorizedAccessException)
                        {
                            Console.WriteLine($"\n{ex.Message} or filename not specified");
                            return;
                        }
                    }
                }


                foreach (BlackDuckMatchedFileWithComponent matchedfile in matchedFiles)
                {
                    var matchesString = "";
                    string matchedFileOrComponentPath = matchedfile.uri;

                    if (matchedFileOrComponentPath == "" || matchedfile.uri == null)
                    {
                        matchedFileOrComponentPath = matchedfile.declaredComponentPath;
                    }

                    foreach (BlackDuckMatchedFileWithComponentMatch match in matchedfile.matches)
                    {
                        string compId = bdapi.ParseComponentId(match.component);
                        matchesString += match.matchType + ";" + compId;
                    }

                    string logString = matchedFileOrComponentPath + ";" + matchesString;

                    if (filePath != "")
                    {
                        Logger.Log(filePath, logString);
                    }
                    else
                    {
                        Console.WriteLine(logString);
                    }
                }
                Console.WriteLine($"\nFinished logging to file {filePath}");
            });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;

        }
    }
}
