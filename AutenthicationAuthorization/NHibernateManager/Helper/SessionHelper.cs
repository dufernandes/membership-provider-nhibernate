using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace NHibernateManager.Helper
{
    /// <summary>
    /// Helper methods for dealing with NHibernate ISession.
    /// </summary>
    public class SessionHelper
    {
        /// <summary>
        /// NHibernate Helper
        /// </summary>
        private NHibernateHelper _nHibernateHelper = null;

        /// <summary>
        /// Instantiate an SessionHelper using an INHibernateMapper implementation.
        /// The INHibernateMapper provides basic database information.
        /// </summary>
        /// <param name="mapper">INHibernateMapper implementation.</param>
        /// <exception cref="ArgumentNullException">Exception thrown in the the
        /// parameter mapper is null.</exception>
        public SessionHelper(INHibernateMapper mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException("The INHibernateMapper argument cannot be null");
            }

            _nHibernateHelper = new NHibernateHelper(mapper);
        }

        /// <summary>
        /// Retrive the current ISession.
        /// </summary>
        public ISession Current
        {
            get
            {
                return _nHibernateHelper.GetCurrentSession();
            }
        }

        /// <summary>
        /// Create an ISession.
        /// </summary>
        public void CreateSession()
        {
            _nHibernateHelper.CreateSession();
        }

        /// <summary>
        /// Clear an ISession.
        /// </summary>
        public void ClearSession()
        {
            Current.Clear();
        }

        /// <summary>
        /// Open an ISession.
        /// </summary>
        public void OpenSession()
        {
            _nHibernateHelper.OpenSession();
        }

        /// <summary>
        /// Close an ISession.
        /// </summary>
        public void CloseSession()
        {
            _nHibernateHelper.CloseSession();
        }
    }
}
