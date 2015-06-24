using AutenthicationAuthorization.VO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Linq;
using NHibernateManager.Dao;
using NHibernateManager.Helper;
using AutenthicationAuthorization.NHibernateManager;

namespace AutenthicationAuthorization.DAO
{
    public class CustomRoleDAO : BaseDao<CustomRole, Int64>
    {
        public CustomRole FindByName(String roleName, String applicationName)
        {
            CustomRole roleCustom = null;

            var queryResult = (from roles in CurrentSession.Query<CustomRole>()
                               where roles.MembershipApplication.Name == applicationName &&
                                     roles.Name == roleName
                               select roles);

            if (queryResult != null && queryResult.Count<CustomRole>() > 0)
            {
                roleCustom = queryResult.First<CustomRole>();
            }
            return roleCustom;
        }

        public IList<CustomRole> FindByUserNameMatch(String userName, String applicationName)
        {
            IList<CustomRole> customRoles = null;

            var queryResult = (from roles in CurrentSession.Query<CustomRole>()
                                     .Where(r => r.MembershipApplication.Name == applicationName)
                                     .Where(r => r.Users.Any(u => u.Name.Contains(userName)))
                               select roles);

            if (queryResult != null && queryResult.Count<CustomRole>() > 0)
            {
                customRoles = queryResult.ToList();
            }
            return customRoles;
        }

        public IList<CustomRole> FindByApplicationName(String applicationName)
        {
            IList<CustomRole> customRoles = null;

            var queryResult = (from roles in CurrentSession.Query<CustomRole>()
                               where roles.MembershipApplication.Name == applicationName
                               select roles);

            if (queryResult != null && queryResult.Count<CustomRole>() > 0)
            {
                customRoles = queryResult.ToList();
            }
            return customRoles;
        }

        public Int32 DeleteByName(String roleName, String applicationName)
        {
            Int32 count = 0;

            var queryResult = (from roles in CurrentSession.Query<CustomRole>()
                               where roles.MembershipApplication.Name == applicationName &&
                                     roles.Name == roleName
                               select roles);

            if (queryResult != null && queryResult.Count<CustomRole>() > 0)
            {
                count = queryResult.Count();
                queryResult.ToList().ForEach(roleToBeDeleted => CurrentSession.Delete(roleToBeDeleted));
                CurrentSession.Flush();
            }

            return count;
        }

        public override INHibernateMapper GetINHibernateMapper()
        {
            return new NHibernateMembershipMapper();
        }
    }
}
