using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace VirtualCurrencyWebSvc.Data
{
    [DataContract]
    public class SiteResponse
    {
        public enum Status
        {
            Success,
            Exception
        }

        [DataMember(IsRequired = true)]
        public string syncKey;

        [DataMember(IsRequired = true)]
        public Status status = Status.Success;

        [DataMember(IsRequired = true)]
        public object response;
    }
}
