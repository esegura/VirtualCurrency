using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Security;
using RoleManagement;
using UserManagement;
using UserManagement.MembershipProviders;
using VirtualCurrencyWebSvc.Data;
using VirtualCurrencyWebSvc.Properties;

namespace VirtualCurrencyWebSvc.Util
{
    public class SessionUtil
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static User GetUser(string username)
        {
            if (username == Settings.Default.AdministratorUsername)
                return FindAdministratorUser();

            return FindUser(username);
        }

        public static User FindUser(string username)
        {
            User u = null;

            // InfomotorAPIMembershipUser membershipUser = (CpadPageMembershipUser)provider.GetUser(username, true);
            MembershipUser membershipUser = Membership.GetUser(username, true);

            // Try to find our record for this user. If we don't have it, create one.
            // The user could have authenticated against LDAP, for ex. 
            // If this is the first time s/he logs in, we won't have a record for him. Create it.
            try
            {
                u = UserManagementService.FindUserWithAlias(username);
            }
            catch (UserManagementServiceException umse)
            {
                if (umse.Code == UserManagementServiceException.ErrorCode.ObjectNotFound)
                {
                    if (log.IsInfoEnabled) log.Info("Validation succeeded, but username not found: " + username);
                }
                else
                    throw;
            }

            bool isUsingExternalMembership = membershipUser.ProviderName != typeof(UserManagementMembershipProvider).Name;

            // If we didn't find the user with his alias 
            // and the user was validated using our UserManagementMembershipProvider, that's a bug
            // if the user is using other membership provider, we might pull the data to build our record for this user in the next step below
            Debug.Assert((u == null) ? isUsingExternalMembership : true);

            // At this point we want to keep the our data for this user in sync with that in the 
            // MembershipProvider. For example to keep our data in sync with LDAP
            if (isUsingExternalMembership)
                SessionUtil.SyncUserWithExternalDirectory((UserManagementMembershipUser)membershipUser, ref u);

            return u;
        }

        private static void SyncUserWithExternalDirectory(UserManagementMembershipUser membershipUser, ref User u)
        {
            // if the user is null, create it
            u = u ?? UserManagementService.CreateUser(membershipUser.UserName, membershipUser.Email, null);

            // if anything has changed since the last time this user logged in, update our copy
            if (u.Email != membershipUser.Email)
            {
                u.Email = membershipUser.Email;
                UserManagementService.UpdateUser(u, u);
            }

            if (u.Name != membershipUser.Name)
            {
                u.Name = membershipUser.Name;
                UserManagementService.UpdateUser(u, u);
            }
        }

        private static bool IsAdministrator(string username)
        {
            if (username == Settings.Default.AdministratorUsername)
                return true;

            User u = FindUser(username);
            return IsAdministrator(u);
        }

        public static bool IsAdministrator(User u)
        {
            RoleManagementService roleManagementService = new RoleManagementService(RoleManagementService.Application);
            IEnumerable<Role> userRoles = roleManagementService.FindRoles(u.Id);
            Role adminRole = userRoles.FirstOrDefault(r => r.Name == Settings.Default.AdministratorRoleName);
            return adminRole != null;
        }

        private static User FindAdministratorUser()
        {
            return UserManagementService.FindUserWithAlias(Settings.Default.AdministratorUsername);
        }

        public static bool ValidateLogin(string username, string password)
        {
            MembershipProvider provider = new UserManagementMembershipProvider();

            if (username == Settings.Default.AdministratorUsername)
            {
                if (password == Settings.Default.AdministratorPassword)
                    return true;
                return false;
            }

            return provider.ValidateUser(username, password);
        }

        public static LoginToken Login(string username, string password)
        {
            bool validated = ValidateLogin(username, password);
            if (!validated)
                throw new ApplicationException("Invalid username or password");

            return CreateLoginToken(username);
        }

        public static LoginToken LoginWithOam(HttpContext context)
        {
            if (!Settings.Default.EnableOamIntegration)
                throw new InfomotorAPIException(InfomotorAPIException.ErrorCode.AccessDenied, "OamIntegration is not enabled");

            string username = context.Header("cpadid");
            string obSSOCookie = GetObSSOCookie(context);

            if (log.IsInfoEnabled) log.Info(string.Format("User '{0}' logged in with sso='{1}'", username, obSSOCookie));

            return CreateLoginToken(username);
        }

        private static LoginToken CreateLoginToken(string username)
        {
            User u = GetUser(username);
            LoginToken lt = UserManagementService.CreateLoginToken(u);
            return lt;
        }

        // looks like they are encoding the sso token into a 'Cookie' field in the headers now. We need to take it out of there.
        private static string GetObSSOCookie(HttpContext context)
        {
            //AR: Not all user has the cookie?! WTF?
            /*
           string cookie = context.Header("Cookie");

           if (cookie == null)
           {
               return (System.DateTime.Now.Ticks.ToString());
               //throw new ArgumentException("Could not find SSO cookie. Check OAM configuration.");
           }

           string[] nameValuePairs = cookie.Split(';');
           for (int i = 0; i < nameValuePairs.Length; i++)
           {
               string[] nameValuePair = nameValuePairs[i].Split('=');
               if ((nameValuePair.Length == 2) && (nameValuePair[0] == "ObSSOCookie"))
                   return nameValuePair[1];
           }

           //throw new ArgumentException("Could not find SSO cookie. Check OAM configuration.");
           //AR: Not all user has the cookie?! WTF?
            */
            return (System.DateTime.Now.Ticks.ToString());
        }

        public static void Logout(LoginToken lt)
        {
            UserManagementService.InvalidateLoginToken(lt);
        }

        public static void ResetPassword(string username, string email)
        {
            if (!string.IsNullOrEmpty((email ?? "").Trim()))
            {
                UserManagementService.ResetUserPassword(email);
                return;
            }

            User match = FindUser(username);
                
            if (match == null)
                throw new ApplicationException(
                    string.Format("Could not find user with username '{0}'.", username));

            UserManagementService.ResetUserPassword(match.Email);
        }

        public static bool IsUsernameTaken(string username)
        {
            try
            {
                // if the user is different than null, then it means that this username is taken by another
                // user.            
                return GetUser(username) != null;
            }
            catch (Exception ignored)
            {
                if (log.IsInfoEnabled)
                {
                    log.Info("The username has not been take. This exception indicates not user"
                        + " has taken the username: "
                        + ignored.Message);
                }
                return false;
            }
        }

        public static bool IsEmailTaken(string newEmail)
        {
            try
            {
                // the reason I have it like is because It is not clear, after check
                return UserManagementService.FindUserWithEmail(newEmail) != null;
                //if (match == null) return false;
                //if (match.Email == null) return false;
                //if (match.Email != newEmail) return false;
                //throw new ApplicationException("this is weird");
            }
            catch (Exception ignored)
            {
                if (log.IsInfoEnabled)
                {
                    log.Info("The username has not been take. This exception indicates not user"
                        + " has taken the username: "
                        + ignored.Message);
                }
                return false;
            }

        }
    }
}
