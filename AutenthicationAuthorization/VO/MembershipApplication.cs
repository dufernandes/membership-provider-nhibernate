using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernateManager.VO;

namespace AutenthicationAuthorization.VO
{
    [Serializable]
    public class MembershipApplication : BaseVO<Int64>
    {
        public virtual String Name { get; set; }
        public virtual ICollection<CustomUser> Users { get; set; }
        public virtual ICollection<CustomRole> Roles { get; set; }
    }
}
