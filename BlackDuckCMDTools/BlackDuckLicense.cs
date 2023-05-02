using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackDuckCMDTools
{
    /// This class is for Deserialization of API Json responses, the members names follow API fields names and don't follow c# naming conventions

    public class BlackDuckLicense
    {
        public string name;       
        public string ownership;
        public string licenseStatus;
        public string licenseSource;
        public BlackDuckLicenseFamily licenseFamily;
        public BlackDuckAPI_meta _meta;
    }
}
