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
    public class MembershipApplicationDAO : BaseDao<MembershipApplication, Int64>
    {
        public MembershipApplication FindByName(String name)
        {
            MembershipApplication membership = null;

            var queryResult = (from memberships in CurrentSession.Query<MembershipApplication>()
                               where memberships.Name == name
                               select memberships);

            if (queryResult != null && queryResult.Count<MembershipApplication>() > 0)
            {
                membership = queryResult.First<MembershipApplication>();
            }
            return membership;
        }

        public override INHibernateMapper GetINHibernateMapper()
        {
            return new NHibernateMembershipMapper();
        }
    }
}
