using System;
using BlackDuckCMDTools;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BlackDuckCMDTESTProject
{
    class Program
    {
        static void Main(string[] args)
        {
           

            var bdtoken = "NDYwMWNhODktMWEzNS00NDc5LWFlNjQtYmZmMDUxY2Q4YzljOmMzOTAyNmIxLTllOTEtNDVkMS05YjIxLTZhOGFlYzQ4NmM5OQ==";            

            var bdurl = "https://sup-hub-knurenko01.dc1.lan";

            var bdhash = "d38ca33891087edc69e76bc17fd2b5f813dd7466"; // my bd hash
           

            BlackDuckRestAPI bd = new BlackDuckRestAPI(bdurl, bdtoken, bdhash);

                      
            

            var projectName = "strashila_python_cicd_tests";

            //Console.WriteLine(bd.ReturnPolicyRules());

            Console.WriteLine(bd.getProjectIDFromName(projectName));





        }
    }
}
