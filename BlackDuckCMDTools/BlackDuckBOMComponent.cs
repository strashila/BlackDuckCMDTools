using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{

    /// This class is for BOM Component Representation Deserialization, for simplicity the members names follow API fields and don't don't follow c# naming conventions

    public class BlackDuckBOMComponent
    {
        public string componentName;
        public string component; //component URL, like "/components/dfb9e6a1-7f47-4b73-a10c-136c7cd89e2e"
        public int totalFileMatchCount;

    }
}
