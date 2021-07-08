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

            var bdurl = new Option<string>("--bdurl");
            bdurl.Description = "REQUIRED: BlackDuck URL";

            var projectname = new Option<string>("--projectname");
            projectname.Description = "REQUIRED: Project name";

            var versionname = new Option<string>("--versionname");
            versionname.Description = "REQUIRED: Version name";

            var notSecure = new Option<bool>("--not-secure");
            //secureConnection.SetDefaultValue(false);
            notSecure.Description = "disable secure connection";


            var filePath = new Option<string>("--filepath");
            filePath.AddAlias("-f");
            filePath.Description = "Output filepath. If not present in option the tool prints to console";


            var filter = new Option<string>("--filter");
            filter.Description = "Supported Filters: [bomMatchType]";

            var rootCommand = new RootCommand
            {
                token,
                bdurl,
                projectname,
                versionname,
                notSecure,
                filePath,
                filter
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string, string, bool, string, string>((token, bdUrl, projectname, versionname, notSecure, filePath, filter) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;
                List<BlackDuckMatchedFileWithComponent> matchedFiles;

                var additionalSearchParams = "?offset=0&limit=5000";

                if (token == "" || bdUrl == "" || projectname == "")
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


                matchedFiles = bdapi.getBOMMatchedFilesWithComponent(projectname, versionname, additionalSearchParams);

                var columnString = "uri_or_declared_component_path;matchType;componentId";

                if (filePath != "")
                {
                    Logger.Log(filePath, columnString);
                }

                

                foreach (var matchedfile in matchedFiles)
                {
                    var matchesString = "";
                    var matchedFileOrComponentPath = matchedfile.uri;
                    if (matchedFileOrComponentPath == "" || matchedfile.uri == null)
                    {
                        matchedFileOrComponentPath = matchedfile.declaredComponentPath;
                    }
                    foreach (var match in matchedfile.matches)
                    {
                        var compId = bdapi.parseComponentId(match.component);
                        matchesString += match.matchType + ";" + compId;
                    }
                    var logString = matchedFileOrComponentPath + ";" + matchesString;
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
