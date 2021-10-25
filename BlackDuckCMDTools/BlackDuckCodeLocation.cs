using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{
    /// This class is for Hub CodeLocation Deserialization, for simplicity the members names follow API fields and don't don't follow c# naming conventions
    /// /api-doc/public.html#codelocation-representation

    public class BlackDuckCodeLocation
    {
        public string name;
        public string url;
        public long scanSize;
        public DateTime createdAt;
        public DateTime updatedAt;
        public string mappedProjectVersion;
        public BlackDuckAPI_meta _meta;

    }
}
