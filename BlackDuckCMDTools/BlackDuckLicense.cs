﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{
    /// This class is for Deserialization of API Json responses, the members names follow API fields names and don't follow c# naming conventions

    public class BlackDuckLicense
    {
        public string name;
        public List<string> licenseFamily;
        public string ownership;
        public string licenseStatus;
        public DateTime expirationDate;
        public BlackDuckAPI_meta _meta;

    }
}
