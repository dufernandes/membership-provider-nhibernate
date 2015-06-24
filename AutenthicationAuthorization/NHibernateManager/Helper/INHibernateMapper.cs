using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernateManager.Helper
{
    /// <summary>
    /// Indicates the configuration type of the NHibernate.
    /// </summary>
    public enum NHibernateConfigType
    {
        /// <summary>
        /// The configuration is in the target application's NHibernate specific
        /// Config file.
        /// </summary>
        TargetFile,
        /// <summary>
        /// The configuration is in the Web.Config file of an Web App or Web Site.
        /// </summary>
        TargetWebConfig
    }

    /// <summary>
    /// Interface which its implementation defines a Database configuration
    /// for NHibernate.
    /// </summary>
    public interface INHibernateMapper
    {
        /// <summary>
        /// Retrieve the unique identifier of this configuration.
        /// </summary>
        /// <returns>Unique identifier.</returns>
        String GetUniqueIdentifier();

        /// <summary>
        /// Retrieve the configuration type (Web.Config, NHibernate.Config, etc). For
        /// more details check NHibernateConfigType.
        /// </summary>
        /// <returns>The Configuration type.</returns>
        NHibernateConfigType GetConfigType();

        /// <summary>
        /// In case the configuration type (retrieved by the method 
        /// NHibernateManager.HelperNHibernateConfigType.GetConfigType() is of type
        /// NHibernateConfigType.TargetFile, a name of the target file must be provided, along with its 
        /// path (in case it is not in the target's root directory).
        /// </summary>
        /// <returns>Database Configuration File.</returns>
        String GetFileName();
    }
}
