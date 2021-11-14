using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{
    public class BlackDuckScanSummary
    {
        public string status;
        public DateTime createdAt;
        public int matchCount;
        public int directoryCount;
        public int fileCount;
        public string hostName;
        public string scanType;
        public BlackDuckAPI_meta _meta;
    }
}
