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

            var additionalSearchParams = "?offset=0&limit=5000"; // might as well, never saw a need to paginate

            var token = new Option<string>("--token");
            token.Description = "REQUIRED: BD Token";

            var bdurl = new Option<string>("--bdurl");
            bdurl.Description = "REQUIRED: BlackDuck URL";

            var projectname = new Option<string>("--projectname");
            projectname.Description = "REQUIRED: Project name";

            var versionname = new Option<string>("--versionname");
            versionname.Description = "REQUIRED: Version name";

            var secureConnection = new Option<bool>("--not-secure");
            //secureConnection.SetDefaultValue(false);
            secureConnection.Description = "disable secure connection";


            var filePath = new Option<string>("--filepath");
            filePath.AddAlias("-f");
            filePath.Description = "Output filepath. If not present in option the tool prints to console";


            var rootCommand = new RootCommand
            {
                token,
                bdurl,
                projectname,
                versionname,
                secureConnection,
                filePath
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string, string, bool, string>((token, bdUrl, projectname, projectVersionName, notSecure, filepath) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI bdapi;
                List<BlackDuckMatchedFileWithComponent> matchedFiles;

                

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

                matchedFiles = bdapi.getBOMMatchedFilesWithComponent(projectname, projectVersionName, additionalSearchParams);

                var columnString = "fileURI;matchType;componentId";

                if (filepath != "")
                {
                    Logger.Log(filepath, columnString);
                }

                

                foreach (var matchedfile in matchedFiles)
                {
                    var matchesString = "";
                    var matchedFileUri = matchedfile.uri;
                    if (matchedFileUri == "" || matchedfile.uri == null)
                    {
                        matchedFileUri = "no_uri_detected";
                    }
                    foreach (var match in matchedfile.matches)
                    {
                        var compId = bdapi.parseComponentId(match.component);
                        matchesString += match.matchType + ";" + compId;
                    }
                    var logString = matchedFileUri + ";" + matchesString;
                    if (filepath != "")
                    {
                        Logger.Log(filepath, logString);
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
