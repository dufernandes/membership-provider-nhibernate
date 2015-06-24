using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernateManager.VO
{
    /// <summary>
    /// This class comparmentalizes the Value Object for this framework.
    /// Value Objects, or VOs, store data which is persisted in the
    /// Database.
    /// </summary>
    /// <typeparam name="TIdentifier">Identifier Type.</typeparam>
    public class BaseVO<TIdentifier> 
    {
        public BaseVO()
        {
            DateTime now = DateTime.Now;
            CreationDate = now;
            LastUpdateDate = now;
        }

        /// <summary>
        /// Gets or sets the Identifier.
        /// </summary>
        public virtual TIdentifier Id
        { get; set; }

        public virtual DateTime CreationDate
        { get; set; }

        public virtual DateTime LastUpdateDate
        { get; set; }

        /// <summary>
        /// Set the Creation and Last Update Date to now.
        /// </summary>
        public virtual void InitializeDates()
        {
            // call overloated method with the now date
            InitializeDates(DateTime.Now);
        }

        /// <summary>
        /// Set Creation and Last Update Date to the date
        /// parameter.
        /// </summary>
        /// <param name="date">Date to set the Creation and Last
        /// Update Dates.</param>
        public virtual void InitializeDates(DateTime date)
        {
            CreationDate = date;
            LastUpdateDate = date;
        }
    }
}
