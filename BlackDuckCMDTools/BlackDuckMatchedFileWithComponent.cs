using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{
    public class BlackDuckMatchedFileWithComponent
    {
        public string uri;
        public string declaredComponentPath;
        public List<BlackDuckMatchedFileWithComponentMatch> matches;
    }
}
