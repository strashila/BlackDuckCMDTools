using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{
    // This is a class for deserialization only
    public class BlackDuckAPIProjectVersionsListing
    {
        public int totalCount;
        public List<BlackDuckProjectVersion> items;
        public BlackDuckAPIMeta _meta;


    }
}
