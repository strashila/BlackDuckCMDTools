using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{

    // This class is the Component Origin Representation /api-doc/public.html#component-origin-representation
    // This is the origin representation that is being returned with BOM component, from /api-doc/public.html#bom-component-representation
    //
    public class BlackDuckKBcomponentOrigin
    {
        public string versionName; //"v1.14.0"
        public DateTime releasedOn; //"2022-07-28T12:46:06.507Z"
        public string source; //"KB"
        public string originName; //"redhat"
        public string originId; // "coreutils-single/8.30-8.el8/x86_64"
        public string externalNamespace; // "redhat"
        public string externalId;
        public BlackDuckAPI_meta _meta;
    }
}
