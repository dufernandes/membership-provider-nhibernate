using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernateManager.Helper
{
    /// <summary>
    /// Session manager which stores and retrieves various NHibernate Sessionhelpers.
    /// Each SessionHelper holds information regarding a specific database. This
    /// Session Manager uses the Factory Design Pattern.
    /// </summary>
    public class SessionManagerFactory
    {
        /// <summary>
        /// Static HashMap which stores all Session Helpers.
        /// </summary>
        private static IDictionary<String, SessionHelper> _sessionMap = new Dictionary<String, SessionHelper>();

        /// <summary>
        /// Make the basic constructor private so this class cannot
        /// be instantiated.
        /// </summary>
        private SessionManagerFactory()
        {
            // do nothing
        }

        /// <summary>
        /// Based on the INHibernateMapper implementation identifier (given by the method 
        /// INHibernateMapper.GetUniqueIdentifier()), a stored instance of
        /// the SessionHelper is retrieved. In case this instance
        /// is not yet stored, a new SessionHelper is created based on the data provided
        /// by the INHibernateMapper implementation, and this newly-created-SessionHelper
        /// is then stored in this factory.
        /// </summary>
        /// <param name="mapper">INHibernateMapper implementation.</param>
        /// <returns>An instance of the SessionHelper, based on a INHibernateMapper implementation.</returns>
        public static SessionHelper GetSessionHelper(INHibernateMapper mapper)
        {
            SessionHelper sessionHelper = null;

            // in case the SessionHelper is already stored, retrieve it.
            if (_sessionMap.ContainsKey(mapper.GetUniqueIdentifier()))
            {
                sessionHelper = _sessionMap[mapper.GetUniqueIdentifier()];
            }
            // the session helper is not stored
            else
            {
                // create a new SessionHelper and store it
                sessionHelper = new SessionHelper(mapper);
                _sessionMap.Add(mapper.GetUniqueIdentifier(), sessionHelper);
            }

            return sessionHelper;
        }
    }
}
