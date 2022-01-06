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


            var email = new Option<string>("--email");
            token.Description = "REQUIRED: User Email";

            var notSecure = new Option<bool>("--not-secure");            
            notSecure.Description = "Disable secure connection to BlackDuck server";

            var rootCommand = new RootCommand
            {
                bdUrl,
                token,
                email,
                notSecure
            };


            // The new command handler API
            // https://github.com/dotnet/command-line-api/issues/1537


            rootCommand.SetHandler(
            (string bdUrl, string token, string email, bool notSecure) => 
                {
                    BlackDuckCMDTools.BlackDuckRestAPI bdapi;

                    var offset = 0;
                    var limit = 1000;
                    var additionalSearchParams = $"?offset={offset}&limit={limit}";


                    if (token == "" || bdUrl == "" || email == "")
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
                        Console.WriteLine("Getting users...");
                        var users = bdapi.GetUsers(additionalSearchParams);

                        while (users.Count > 0)
                        {
                            foreach (var user in users)
                            {
                                if (user.email != null && user.email == email && user.type == "EXTERNAL")
                                {
                                    Console.WriteLine(bdapi.DeactivateUser(user));
                                }
                            }

                            offset += limit;
                            additionalSearchParams = $"?offset={offset}&limit={limit}";
                            users = bdapi.GetUsers(additionalSearchParams);
                        }
                    }

                    catch (Exception ex)
                    {
                        // Catching Serialization errors
                        Console.WriteLine("\nError:" + ex.Message + " Please verify that you have correct BDurl and token ");
                        return;
                    }
                },

            bdUrl, token, email, notSecure);

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
