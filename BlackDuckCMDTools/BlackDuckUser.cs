using System;
using System.Collections.Generic;
using System.Text;

namespace BlackDuckCMDTools
{
    /// This class is for Deserialization of API Json responses, the members names follow API fields names and don't follow c# naming conventions

    public class BlackDuckUser
    {
        public string userName;
        public string externalUserName;
        public string firstName;
        public string lastName;
        public string email;
        public string type;
        public bool active;
        public BlackDuckAPI_meta _meta;
    }
}
