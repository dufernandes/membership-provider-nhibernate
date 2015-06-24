using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Configuration;

namespace NHibernateManager.WebApp
{
    /// <summary>
    /// Utilitary class for web applications.
    /// </summary>
    class WebAppUtils
    {
        /// <summary>
        /// Get Web Applicaton Server Path.
        /// </summary>
        /// <returns>Web Applicatoin Server Path.</returns>
        public static String GetWebAppPath()
        {
            return HttpRuntime.AppDomainAppPath;
        }

        /// <summary>
        /// Get Web.Config object.
        /// </summary>
        /// <returns>Web.Config file object.</returns>
        public static Configuration GetWebConfig()
        {
            return WebConfigurationManager.OpenWebConfiguration("~");
        }

        /// <summary>
        /// Get Membership Section within Web.Config.
        /// </summary>
        /// <returns>Membership Section within Web.Config.</returns>
        public static MembershipSection GetMembershipSection()
        {
            Configuration config = GetWebConfig();
            return (MembershipSection)config.GetSection("system.web/membership");
        }

        /// <summary>
        /// Retrieve a Membership Value for a Membership Property within 
        /// the Membership Section in the Web.Config file.
        /// </summary>
        /// <param name="memebershipProperty">Mem</param>
        /// <returns>Membership Value of a certain property.</returns>
        private static String GetMembershipProperty(String memebershipProperty)
        {
            MembershipSection section = WebAppUtils.GetMembershipSection();
            String defaultProvider = section.DefaultProvider;
            ProviderSettings providerSettings = section.Providers[defaultProvider];
            return providerSettings.Parameters.Get(memebershipProperty);
        }

        /// <summary>
        /// Check whether a certain property of a Membership is set in the
        /// Membership Section of the Web.Config.
        /// </summary>
        /// <param name="memebershipProperty">Membership Property te be verified
        /// whether it was assigned in the Memberhsip Section.</param>
        /// <returns>True in case the Property has a value, false otherwise.</returns>
        private static bool ContainsMembershipProperty(String memebershipProperty)
        {
            return GetMembershipProperty(memebershipProperty) != null;
        }

        /// <summary>
        /// Set a Membership Property Value within the Membership Section in the
        /// Web.Config file.
        /// </summary>
        /// <param name="membershipProperty">Membership Property which a certain Value
        /// will be assigned.</param>
        /// <param name="value">Value to be assigned.</param>
        public static void SetMembershipProperty(String membershipProperty, String value)
        {
            Configuration config = GetWebConfig();
            MembershipSection section = (MembershipSection)config.GetSection("system.web/membership");
            String defaultProvider = section.DefaultProvider;
            ProviderSettings providerSettings = section.Providers[defaultProvider];
            providerSettings.Parameters.Set(membershipProperty, value);
            config.Save();
        }
    }
}
