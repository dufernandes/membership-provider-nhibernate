using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernateManager.Helper;

namespace AutenthicationAuthorization.NHibernateManager
{
    public class NHibernateMembershipMapper : INHibernateMapper
    {
        public string GetUniqueIdentifier()
        {
            return "NHibernateMembershipMapper";
        }

        public NHibernateConfigType GetConfigType()
        {
            return NHibernateConfigType.TargetFile;
        }

        public string GetFileName()
        {
            return "NHibernateAuthentication.config";
        }
    }
}
