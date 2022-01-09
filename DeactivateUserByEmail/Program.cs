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


            var _email = new Option<string>(
                "--email",
                description: "REQUIRED: User Email"
                );


            var _trustcert = new Option<bool>(
                "--trust-cert",
                description: "Trust the certificate of the BD server",
                getDefaultValue: () => false
                );


            var rootCommand = new RootCommand
            {
                _bdurl,
                _token,
                _email,
                _trustcert
            };


            // The new command handler API
            // https://github.com/dotnet/command-line-api/issues/1537


            rootCommand.SetHandler(
            (string bdUrl, string token, string email, bool trustCert) => 
                {
                    BlackDuckCMDTools.BlackDuckRestAPI bdapi;

                    var offset = 0;
                    var limit = 1000;
                    var additionalSearchParams = $"?offset={offset}&limit={limit}";


                    if (token == null || bdUrl == null || email == null)
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

                    if (trustCert)
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
                        Console.WriteLine();
                        var users = bdapi.GetUsers(additionalSearchParams);

                        while (users.Count > 0)
                        {
                            foreach (var user in users)
                            {
                                if (user.email != null && user.email == email && user.active == true && user.type == "EXTERNAL")
                                {
                                    var deactivatedUser = (bdapi.DeactivateUser(user));
                                    Console.WriteLine("User Deactivated");
                                    Console.WriteLine(deactivatedUser);
                                    Console.WriteLine();
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

            _bdurl, _token, _email, _trustcert);

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
