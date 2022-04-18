using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackDuckCMDTools
{

    /// This is the "KB" component version, coming from /api/components

    public class BlackDuckComponentVersion
    {
        public string versionName;
        public DateTime releasedOn;
        public string approvalStatus;
        public string source;
        public string type;
        public BlackDuckAPI_meta _meta;

        public string getVersionID()
        {
            return this._meta.href.Split('/').Last();
        }
    }
}
