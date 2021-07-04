using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{

    // This is a class for deserialization only
    public class BlackDuckProject 
    {
        public string name;
        public string description;
        
        public bool projectLevelAdjustments;
        public DateTime createdAt;
        public string createdBy;
        public string createdByUser;
        public DateTime updatedAt;
        public string updatedBy;
        public string updatedByUser;
        public BlackDuckAPI_meta _meta;

    }
}
