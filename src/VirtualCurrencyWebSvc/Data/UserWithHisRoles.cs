using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualCurrencyWebSvc.Data
{
    public class UserWithHisRoles
    {
        public int userId;
        public string userName;
        public string userFullName;
        public List<RoleManagement.Role> roles;
    }
}
