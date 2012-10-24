using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace VirtualCurrencyWebSvc.Data
{
    [DataContract]
    public class InfoDataListEntry
    {
        [DataMember]
        public string id;
        [DataMember]
        public string viewType;
        [DataMember]
        public string title;
        [DataMember]
        public string dateTime;
    }
}
