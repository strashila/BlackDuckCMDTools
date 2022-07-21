using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{

    /// This is the BOM component, coming from /api/projects/{id}/versions/{id}/components. it is NOT the same as BlackDuckComponent, coming from /api/components

    public class BlackDuckBOMComponent
    {
        public string componentName; //"google.golang.org/protobuf"
        public string componentVersionName; //"v1.27.1"
        public string component; //component URL, like "https://{BDURL}/api/components/dfb9e6a1-7f47-4b73-a10c-136c7cd89e2e" This is a link without project and version.
        public string componentVersion; // component Version URL, like "https://{BDURL}/api/components/{iD}/versions/{versionId}" 
        public List<BlackDuckBOMComponentOrigin> origins;
        public int totalFileMatchCount;
        public BlackDuckAPI_meta _meta;
    }
}
