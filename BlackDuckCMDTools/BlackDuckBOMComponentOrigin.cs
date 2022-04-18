using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{

    // This class is different than Component Origin Representation /api-doc/public.html#component-origin-representation
    // This is the origin representation that is being returned with BOM component, from /api-doc/public.html#bom-component-representation
    //
    public class BlackDuckBOMComponentOrigin
    {
        public string name; //"v1.14.0"
        public string origin; // complete origin href "https://{BDURL}/api/components/{compId}/versions/{versionId}/origins/{id}"
        public string externalNamespace; // "github"
        public string externalId; //  "aws/aws-sdk-go-v2:v1.14.0"
        public BlackDuckAPI_meta meta;

    }
}
