using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{

    /// This class is for Component Version Representation Deserialization, the members names follow API fields and don't follow c# naming conventions

    public class BlackDuckBOMComponentVersion
    {
        public string versionName;
        public DateTime releasedOn;
        public string approvalStatus;
        public string source;
        public string type;
        public BlackDuckAPI_meta _meta;
    }
}
