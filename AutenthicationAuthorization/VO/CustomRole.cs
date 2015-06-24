using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernateManager.VO;

namespace AutenthicationAuthorization.VO
{
    [Serializable]
    public class CustomRole : BaseVO<Int64>
    {
        public virtual String Name { get; set; }
        public virtual MembershipApplication MembershipApplication { get; set; }
        public virtual ICollection<CustomUser> Users { get; set; }

        public CustomRole()
        {
        }

        public CustomRole(String name, MembershipApplication membershipApplication)
        {
            this.Name = name;
            this.MembershipApplication = membershipApplication;
        }
    }
}
