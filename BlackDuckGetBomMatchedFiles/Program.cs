using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
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
            filter.Description = "Supported Filters: [bomMatchType]";

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
                List<BlackDuckMatchedFileWithComponent> matchedFiles;

                var additionalSearchParams = "?offset=0&limit=5000";

                if (token == "" || bdUrl == "" || projectName == "")
                {
                    Console.WriteLine("Parameters missing, use --help");
                    return;
                }

                if (notSecure)
                {
                    bdapi = new BlackDuckCMDTools.BlackDuckRestAPI(bdUrl, token, false);
                }

                else { bdapi = new BlackDuckCMDTools.BlackDuckRestAPI(bdUrl, token, true); }

                if (filter != "")
                {
                    additionalSearchParams += "&" + filter;
                }


                matchedFiles = bdapi.GetBOMMatchedFilesWithComponent(projectName, versionName, additionalSearchParams);

                var columnString = "uri_or_declared_component_path;matchType;componentId";

                if (filePath != "")
                {
                    Logger.Log(filePath, columnString);
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

            });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;
            
        }
    }
}
