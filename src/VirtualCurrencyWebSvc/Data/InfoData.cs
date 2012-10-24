using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace VirtualCurrencyWebSvc.Data
{
    // TODO change the name of this class, so it doesn't collide with the one in InfoMotor namespace
    [DataContract]
    public class InfoData
    {
        [DataMember(IsRequired=true)]
        public int anInt;
        [DataMember(IsRequired = true)]
        public string aString;
        [DataMember(IsRequired = false)]
        public string Department;
        [DataMember(IsRequired = true)]
        public string Title;
        [DataMember(IsRequired = true)]
        public string ViewType;
    }
}
