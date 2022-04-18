using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{

    /// This is the KB component, coming from /api/components. This is /api-doc/public.html#component-representation
    /// it is NOT the same as BOM component

    public class BlackDuckComponent
    {
        public string name; //"google.golang.org/protobuf"
        public string description; 

        public string url; // source url "http://github.com/aws/aws-sdk-go-v2/
        public string approvalStatus; // "UNREVIEWED"
        public string primaryLanguage; // Java"
        public string source; //"KB"
        public string type; //  "COMPONENT"        
        public BlackDuckAPI_meta _meta;
    }



}
