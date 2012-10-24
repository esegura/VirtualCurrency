using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using UserManagement;
using RoleManagement;

namespace VirtualCurrencyWebSvc.Data
{
    [DataContract]
    public class ListOfUserRoleResource
    {
        [DataMember]
        public User[] listOfUsers;
        [DataMember]
        public Role[] listOfRoles;
        [DataMember]
        public Resource[] listOfResources;
    }
}