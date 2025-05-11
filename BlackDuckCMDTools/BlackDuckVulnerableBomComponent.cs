using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackDuckCMDTools
{
    /// <summary>
    ///   /api-doc/public.html#_listing_a_boms_vulnerable_components
    /// 
    ///   GET /api/projects/{projectId}/versions/{projectVersionId}/vulnerable-bom-components
    ///   Accept: application/vnd.blackducksoftware.bill-of-materials-8+json
    /// </summary>
    public class BlackDuckVulnerableBomComponent
    {
        public string componentVersion;
        public string componentName;
        public string componentVersionName;
        public string externalNamespace;
        public string externalId;
        public string ignored;
        public BlackDuckVulnerabilityWithRemediation vulnerability;
        public string packageUrl;
        public BlackDuckAPI_meta _meta;
    }
}
