using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BlackDuckCMDTools;
using System.Threading.Tasks;
using System.Text;

namespace BlackDuckCMDTools
{

    /// This class is for Deserialization of API Json responses, for simplicity the members names follow API fields and don't don't follow c# naming conventions <summary>
    /// 
    /// This is the API response metadata, same for every call response
    public class BlackDuckAPI_meta
    {
        public string[] allow;
        public JObject[] links;
        public string href;
    }
}
