using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using NHibernateManager.WebApp;
using AutenthicationAuthorization.DAO;
using AutenthicationAuthorization.VO;
using System.Transactions;
using System.Collections.Specialized;
using NHibernateManager.Helper;

namespace AutenthicationAuthorization.CustomMembership
{
    [Serializable]
    class CustomRoleProvider : RoleProvider
    {
        private String appName = null;
        private String providerName = null;
        private MembershipApplication membershipApplication = null;
        private NameValueCollection providerConfig = null;

        public override void Initialize(string name, NameValueCollection config)
        {
            if (name == null || name.Trim().Length == 0)
            {
                throw new ArgumentException("The name cannot be null nor empty");
            }

            if (config == null)
            {
                throw new ArgumentException("The config cannot be null nor empty");
            }

            base.Initialize(name, config);
            providerName = name;
            this.providerConfig = config;

            // get app name. in case it does not exist, create one
            appName = providerConfig.Get("applicationName");
            if (appName == null)
            {
                appName = "CustomMembershipApplicationName";
            }
            MembershipApplicationDAO memebershipApplicationDAO = new MembershipApplicationDAO();
            membershipApplication = memebershipApplicationDAO.FindByName(appName);
            if (membershipApplication == null)
            {
                membershipApplication = new MembershipApplication();
                membershipApplication.Name = appName;
                memebershipApplicationDAO.SaveOrUpdate(membershipApplication);
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            if (usernames != null && usernames.Length > 0 && roleNames != null && roleNames.Length > 0)
            {
                using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required))
                {
                    CustomUser user = null;
                    CustomRole role = null;
                    foreach (String roleName in roleNames)
                    {
                        CustomUserDAO userDAO = new CustomUserDAO();
                        CustomRoleDAO roleDAO = new CustomRoleDAO();
                        role = roleDAO.FindByName(roleName, ApplicationName);
                        if (role != null)
                        {
                            foreach (String userName in usernames)
                            {
                                user = userDAO.FindByName(userName, ApplicationName);
                                if (role.Users == null)
                                {
                                    role.Users = new HashSet<CustomUser>();
                                }
                                if (!role.Users.Contains(user))
                                {
                                    role.Users.Add(user);
                                }
                            }
                            roleDAO.SaveOrUpdate(role);
                        }
                    }
                    transaction.Complete();
                }
            }
        }

        public override string ApplicationName
        {
            get
            {
                if (appName == null)
                {
                    appName = providerConfig.Get("applicationName");
                }

                MembershipApplicationDAO memebershipApplicationDAO = new MembershipApplicationDAO();
                membershipApplication = memebershipApplicationDAO.FindByName(appName);
                if (membershipApplication == null)
                {
                    membershipApplication = new MembershipApplication();
                    membershipApplication.Name = appName;
                    memebershipApplicationDAO.SaveOrUpdate(membershipApplication);
                }

                return appName;
            }
            set
            {
                // in case the entered app is not created, do it
                MembershipApplicationDAO membershipDAO = new MembershipApplicationDAO();
                MembershipApplication membership = membershipDAO.FindByName(value);
                if (membership == null)
                {
                    membership = new MembershipApplication()
                    {
                        Name = value
                    };
                    membershipDAO.SaveOrUpdate(membership);
                }
                providerConfig.Set("applicationName", value);
                appName = value;
            }
        }

        public override void CreateRole(string roleName)
        {
            if (roleName == null || roleName.Trim().Length == 0)
            {
                throw new ArgumentException("The rolename cannot be null nor empty");
            }

            CustomRoleDAO roleDAO = new CustomRoleDAO();
            roleDAO.SaveOrUpdate(new CustomRole(roleName, membershipApplication));
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            bool isDeleted = false;
            if (roleName != null)
            {
                CustomRoleDAO roleDAO = new CustomRoleDAO();
                Int32 count = roleDAO.DeleteByName(roleName, ApplicationName);
                isDeleted = count > 0;
            }
            return isDeleted;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            IList<String> userNames = new List<String>();
            if (roleName != null && usernameToMatch != null)
            {
                CustomRoleDAO roleDAO = new CustomRoleDAO();
                CustomRole role = roleDAO.FindByName(roleName, ApplicationName);

                if (role != null)
                {
                    ICollection<CustomUser> users = role.Users;
                    if (users != null && users.Count > 0)
                    {
                        foreach (CustomUser user in users)
                        {
                            if (user.Name.Contains(usernameToMatch))
                            {
                                userNames.Add(user.Name);
                            }
                        }
                    }
                }
            }
            return userNames.ToArray<String>();
        }

        public override string[] GetAllRoles()
        {
            String[] roleNames = new String[0];
            CustomRoleDAO roleDAO = new CustomRoleDAO();
            IList<CustomRole> roles = roleDAO.FindByApplicationName(ApplicationName);

            if (roles != null && roles.Count > 0)
            {
                roleNames = new String[roles.Count];
                for (int index = 0; index < roles.Count; index++)
                {
                    roleNames[index] = roles[index].Name;
                }
            }

            return roleNames;
        }

        public override string[] GetRolesForUser(string username)
        {

            String[] roleNames = new String[0];
            if (username != null)
            {
                CustomUserDAO userDAO = new CustomUserDAO();
                CustomUser user = userDAO.FindByName(username, ApplicationName);
                if (user != null)
                {
                    ICollection<CustomRole> roles = user.Roles;
                    if (roles != null && roles.Count > 0)
                    {
                        roleNames = new String[roles.Count];
                        for (int index = 0; index < roles.Count; index++)
                        {
                            roleNames[index] = roles.ElementAt(index).Name;
                        }
                    }
                }
            }
            return roleNames;
        }

        public override string[] GetUsersInRole(string roleName)
        {
            String[] userNames = new String[0];
            if (roleName != null)
            {
                CustomRoleDAO roleDAO = new CustomRoleDAO();
                CustomRole role = roleDAO.FindByName(roleName, ApplicationName);
                if (role != null)
                {
                    ICollection<CustomUser> users = role.Users;
                    if (users != null && users.Count > 0)
                    {
                        userNames = new String[users.Count];
                        for (int index = 0; index < users.Count; index++)
                        {
                            userNames[index] = users.ElementAt(index).Name;
                        }
                    }
                }
            }
            return userNames;
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            bool isUserInRole = false;
            if (username != null && roleName != null)
            {
                CustomRoleDAO roleDAO = new CustomRoleDAO();
                CustomRole role = roleDAO.FindByName(roleName, ApplicationName);
                if (role != null)
                {
                    ICollection<CustomUser> users = role.Users;
                    if (users != null && users.Count > 0)
                    {
                        var query = (from u in users
                                     where u.Name == username
                                     select u.Name);
                        isUserInRole = query != null && query.Count() > 0;
                    }
                }
            }
            return isUserInRole;
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (usernames != null && usernames.Length > 0 && roleNames != null && roleNames.Length > 0)
            {
                using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required))
                {   
                    CustomRole role = null;
                    IList<CustomUser> usersToBeRemoved = null;
                    CustomRoleDAO roleDAO = new CustomRoleDAO();
                    // remove users for each role
                    foreach (String roleName in roleNames)
                    {
                        role = roleDAO.FindByName(roleName, ApplicationName);
                        if (role != null)
                        {
                            if (role.Users != null && role.Users.Count > 0)
                            {
                                // get all matched users (users to be removed) from the current role
                                var query = (from u in role.Users
                                             where usernames.Any(name => name == u.Name)
                                                 select u
                                            );
                                if (query != null && query.Count() > 0)
                                {
                                    usersToBeRemoved = query.ToList();

                                    // remove each matched user from the role
                                    foreach (CustomUser userToBeRemoved in usersToBeRemoved)
                                    {
                                        role.Users.Remove(userToBeRemoved);
                                    }
                                }
                                // persist role
                                roleDAO.SaveOrUpdate(role);
                            }
                        }
                    }

                    transaction.Complete();
                }
            }
        }

        public override bool RoleExists(string roleName)
        {
            CustomRoleDAO roleDAO = new CustomRoleDAO();
            return roleName != null && roleDAO.FindByName(roleName, ApplicationName) != null;
        }
    }
}
