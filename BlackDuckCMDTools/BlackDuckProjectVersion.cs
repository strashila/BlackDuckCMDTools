using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{
    /// This class is for Deserialization of API Json responses, for simplicity the members names follow API fields and don't don't follow c# naming conventions
    public class BlackDuckProjectVersion
    {
        public string versionName;
        public string nickname;
        public string phase;
        public string source;
        public DateTime createdAt;
        public string createdBy;  //user name
        public string createdByUser; // user string with ID. i.e. /api/users/2b4c311f-2a26-4627-bde3-d776aee8d236
        public BlackDuckAPI_meta _meta;
    }
}
