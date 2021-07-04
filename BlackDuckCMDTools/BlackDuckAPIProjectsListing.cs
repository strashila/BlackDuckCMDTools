using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{

    // This is a class for deserialization only
    public class BlackDuckAPIProjectsListing
    {
        public int totalCount;
        public List<BlackDuckProject> items;
        public BlackDuckAPI_meta _meta;
    }
}
