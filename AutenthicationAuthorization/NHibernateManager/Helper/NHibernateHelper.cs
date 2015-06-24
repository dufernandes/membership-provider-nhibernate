using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using System.IO;
using NHibernateManager.WebApp;
using System.Reflection;
using System.Web;

namespace NHibernateManager.Helper
{
    /// <summary>
    /// Here basic NHibernate manipulation methods are implemented.
    /// </summary>
    class NHibernateHelper
    {
        private ISessionFactory _sessionFactory = null;
        private INHibernateMapper _mapper = null;

        /// <summary>
        /// Create an instance of this helper based on a implementation of
        /// the INHibernateMapper interface. This interface provides info regarding
        /// a certain database.
        /// </summary>
        /// <param name="mapper">INHibernateMapper interface implementation.</param>
        /// <exception cref="ArgumentNullException">Exception thrown in the the
        /// parameter mapper is null.</exception>
        public NHibernateHelper(INHibernateMapper mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException("The INHibernateMapper argument cannot be null");
            }

            _mapper = mapper;
        }

        /// <summary>
        /// In case there is an already instantiated NHibernate ISessionFactory,
        /// retrieve it. Otherwise, instantiate on based on the information provided
        /// by the INHibernateMapper associated with this helper.
        /// </summary>
        public ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    Configuration configuration = new Configuration();
                    // for a NHibernate target file, retrieve it and use it the configurate NHibernate
                    if (_mapper.GetConfigType() == NHibernateConfigType.TargetFile)
                    {

                        String webPath = WebAppUtils.GetWebAppPath();
                        configuration.Configure(Path.Combine(webPath, _mapper.GetFileName()));
                    }
                    // for the NHibernate configured in the Web.Config file, configurate the NHibernate
                    else if (_mapper.GetConfigType() == NHibernateConfigType.TargetWebConfig)
                    {
                        configuration.Configure();
                    }

                    /*
                     * If there is no Http Session, the context management class must be of type "call",
                     * which represents the "NHibernate.Context.CallSessionContext' implementation. This
                     * sort of approach supports NHibernate Sessions without Http Context.
                     */
                    if (HttpContext.Current == null)
                    {
                        configuration.SetProperty("current_session_context_class", "call");
                    }

                    // build a Session Factory
                    _sessionFactory = configuration.BuildSessionFactory();
                }
                return _sessionFactory;
            }
        }

        /// <summary>
        /// Open an ISession based on the built SessionFactory.
        /// </summary>
        /// <returns>Opened ISession.</returns>
        public ISession OpenSession()
        {
            return SessionFactory.OpenSession();

        }
        /// <summary>
        /// Create an ISession and bind it to the current tNHibernate Context.
        /// </summary>
        public void CreateSession()
        {
            CurrentSessionContext.Bind(OpenSession());
        }

        /// <summary>
        /// Close an ISession and unbind it from the current
        /// NHibernate Context.
        /// </summary>
        public void CloseSession()
        {
            if (CurrentSessionContext.HasBind(SessionFactory))
            {
                CurrentSessionContext.Unbind(SessionFactory).Dispose();
            }
        }

        /// <summary>
        /// Retrieve the current binded NHibernate ISession, in case there
        /// is any. Otherwise, open a new ISession.
        /// </summary>
        /// <returns>The current binded NHibernate ISession.</returns>
        public ISession GetCurrentSession()
        {
            if (!CurrentSessionContext.HasBind(SessionFactory))
            {
                CurrentSessionContext.Bind(SessionFactory.OpenSession());
            }
            return SessionFactory.GetCurrentSession();
        }
    }
}
