using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{

    /// This class is for BOM Component Representation Deserialization, for simplicity the members names follow API fields and don't don't follow c# naming conventions

    public class BlackDuckBOMComponent
    {
        public string componentName; //"google.golang.org/protobuf"
        public string componentVersionName; //"v1.27.1"

        public string component; //component URL, like "https://{BDURL}/api/components/dfb9e6a1-7f47-4b73-a10c-136c7cd89e2e" This is a link without project and version.
        public string componentVersion; // component Version URL, like "https://{BDURL}/api/components/{iD}/versions/{versionId}" 
        public BlackDuckBOMComponentOrigin[] origins;

        public int totalFileMatchCount;
        public BlackDuckAPI_meta _meta;
    }



}
