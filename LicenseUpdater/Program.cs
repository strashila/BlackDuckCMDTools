using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using BlackDuckCMDTools;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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


            var _csvfile = new Option<string>(
                "--csvfile",
                description: "REQUIRED: CSV file containing the licenses"
                );


            var _notsecure = new Option<bool>(
                "--not-secure",
                description: "Trust the certificate of the BD server",
                getDefaultValue: () => false
                );



            var rootCommand = new RootCommand
            {
                _bdurl,
                _token,
                _csvfile,
                _notsecure
            };


            // The new command handler API
            // https://github.com/dotnet/command-line-api/issues/1537


            rootCommand.SetHandler(
            (string bdUrl, string token, string csvfile, bool notSecure) =>
            {
                BlackDuckCMDTools.BlackDuckRestAPI blackDuckClient;
                TextFieldParser parser;

                if (token == null || bdUrl == null || csvfile == null)
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


                blackDuckClient = new BlackDuckCMDTools.BlackDuckRestAPI(bdUrl, token, notSecure);

                try
                {
                    parser = new(csvfile)
                    {
                        TextFieldType = FieldType.Delimited
                    };
                    parser.SetDelimiters(",");

                }

                catch (Exception ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
                    return;
                }
                Console.WriteLine("Parsing the CSV for licenses to update.");


                try
                {
                    var LicenseStatusValuesList = new List<string> { "UNREVIEWED", "IN_REVIEW", "REVIEWED", "APPROVED", "CONDITIONALLY_APPROVED", "REJECTED", "DEPRECATED" };
                    var licenseChangeStatusDict = new Dictionary<string, string>();

                    while (!parser.EndOfData)
                    {
                        //Processing rows

                        var fields = parser.ReadFields();
                        var key = "";
                        string value = "";

                        foreach (var field in fields)
                        {

                            if (field.Contains("api/licenses"))
                            {
                                key = field;
                            }

                            else if (LicenseStatusValuesList.Contains(field.ToUpper()))
                            {
                                value = field;
                            }
                        }

                        if (value != "")
                        {
                            licenseChangeStatusDict[key] = value;
                        }
                    }

                    Console.WriteLine("The only possible values for new license status are: UNREVIEWED, IN_REVIEW, REVIEWED, APPROVED, CONDITIONALLY_APPROVED, REJECTED, DEPRECATED. All other values will be ignored.");

                    if (licenseChangeStatusDict.Count == 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("No licenses to update");
                    }

                    foreach (var kv in licenseChangeStatusDict)
                    {
                        var license = blackDuckClient.GetSingleLicense(kv.Key);
                        var updatedStatus = kv.Value;

                        Console.WriteLine($"trying to update the license \"{license.name}\" to status {updatedStatus.ToUpper()}... Response is {blackDuckClient.UpdateLicenseStatus(license, updatedStatus)}");
                    }
                }


                catch (Exception ex)
                {
                    // Catching Serialization or other errors
                    Console.WriteLine("\nError:" + ex.Message + " Please verify that you have correct BDurl, token, and Project Name");
                    return;
                }
            },

            _bdurl, _token, _csvfile, _notsecure);

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
