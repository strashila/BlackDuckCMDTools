using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{

    /// This class is for Deserialization of API Json responses, for simplicity the members names follow API fields names and don't don't follow c# naming conventions
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
