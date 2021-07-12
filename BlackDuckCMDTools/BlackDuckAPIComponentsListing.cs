using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{
    /// This class is for Deserialization of API Json responses, for simplicity the members names follow API fields and don't don't follow c# naming conventions

    public class BlackDuckAPIComponentsListing
    {
        public int totalCount;
        public List<BlackDuckBOMComponent> items;
        public BlackDuckAPI_meta _meta;
    }
}
