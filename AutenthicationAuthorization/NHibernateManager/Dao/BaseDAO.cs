using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using NHibernate;
using NHibernate.Linq;
using NHibernateManager.Helper;
using NHibernateManager.VO;

namespace NHibernateManager.Dao
{
    /// <summary>
    /// Dao which provided common basic methods for all Daos which
    /// extend this one. It also implements optimistic concurrenty by
    /// implementing TemplateDao.
    /// </summary>
    /// <typeparam name="TEntity">Value Object associated with this Dao. The Value Object
    /// is the entity which will be manipulated in the database via this very Dao.</typeparam>
    /// <typeparam name="TIdentifier">The identifier type. It could be an Int32, Int64, 
    /// String, Gui and so on.</typeparam>
    public abstract class BaseDao<TEntity, TIdentifier> 
        where TIdentifier : new()
        where TEntity : BaseVO<TIdentifier> 
    {
        /// <summary>
        /// NHibernate ISession to be used to manipulate data in the
        /// database.
        /// </summary>
        protected ISession CurrentSession { get; set; }

        /// <summary>
        /// Constructor which retrieves the NHibernate ISession based on the
        /// provided database, given by the implemented method GetINHibernateMapper().
        /// </summary>
        public BaseDao()
        {
            CreateNHibernateSession(GetINHibernateMapper());
        }

        /// <summary>
        /// Constructor which retrieves the NHibernate ISession based on the
        /// entered mapper which will provide the database configurations. Note that
        /// by calling this constructor, the mapper returned by the method BaseDAO.GetINHibernateMapper()
        /// is ignored.
        /// </summary>
        /// <param name="mapper">Mapper holding database information.</param>
        public BaseDao(INHibernateMapper mapper)
        {
            // create nhibernate session using the entered mapper
            CreateNHibernateSession(mapper);
        }

        /// <summary>
        /// Retrieve an Entity based on its identifier.
        /// </summary>
        /// <param name="valueObjectIdentifier">Value Object Identifier</param>
        /// <returns>Entity based on its Identifier.</returns>
        public TEntity LoadById(TIdentifier valueObjectIdentifier)
        {
            TEntity entity = CurrentSession.Get<TEntity>(valueObjectIdentifier);
            return entity;
        }

        /// <summary>
        /// Retrieve all Entities from the database.
        /// </summary>
        /// <returns>List of all entities.</returns>
        public IList<TEntity> LoadAll()
        {
            return CurrentSession.Query<TEntity>().ToList();
        }

        /// <summary>
        /// Create a nhibernate session using the entered mapper.
        /// </summary>
        /// <param name="mapper">INHibernate mapper holding database information.</param>
        /// <exception cref="System.ArgumentNullException">Exception thrown in case the mapper
        /// parameter is null.</exception>
        private void CreateNHibernateSession(INHibernateMapper mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException("The parameter mapper cannot be null.");
            }

            SessionHelper sessionHelper = SessionManagerFactory.GetSessionHelper(mapper);
            CurrentSession = sessionHelper.Current;
        }

        /// <summary>
        /// Save a new entity.
        /// </summary>
        /// <param name="valueObject">Entity to be saved.</param>
        /// <returns>Newly created entity. It has a newly assigned identifier.</returns>
        public virtual TIdentifier Save(TEntity valueObject)
        {
            TIdentifier identifier = new TIdentifier();
            using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required))
            {
                identifier = (TIdentifier)CurrentSession.Save(valueObject);
                transaction.Complete();
            }
            return identifier;
        }

        /// <summary>
        /// Save a new entity or update an existing one..
        /// </summary>
        /// <param name="valueObject">Entity to be saved or updated.</param>
        /// <returns>Entity's identifier.</returns>
        public virtual TIdentifier SaveOrUpdate(TEntity valueObject)
        {
            using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required))
            {
                CurrentSession.SaveOrUpdate(valueObject);
                transaction.Complete();
            }
            return valueObject.Id;
        }

        /// <summary>
        /// Update a certain Value Object.
        /// </summary>
        /// <param name="valueObject">Value Object to be updated.</param>
        /// <returns>Updated Value Object.</returns>
        public int Update(TEntity valueObject)
        {
            using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required))
            {
                CurrentSession.Update(valueObject);
                CurrentSession.Flush();
                transaction.Complete();
            }
            return 0;
        }

        /// <summary>
        /// Delete an Entity based on its Instance.
        /// </summary>
        /// <param name="entity">Entity Instance.</param>
        public void Delete(TEntity entity)
        {
            using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required))
            {
                CurrentSession.Delete(entity);
                transaction.Complete();
            }
        }

        /// <summary>
        /// Delete an Entity based on its Identifier.
        /// </summary>
        /// <param name="entityIdentifier">Entity Identifier.</param>
        public void DeleteById(TIdentifier entityIdentifier)
        {
            using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required))
            {
                TEntity entity = LoadById(entityIdentifier);
                CurrentSession.Delete(entity);
                transaction.Complete();
            }
        }

        /// <summary>
        /// Refresh an entity state from the database.
        /// </summary>
        /// <param name="entity">Entity which status will be refreshed.</param>
        public void Refresh(TEntity entity)
        {
            CurrentSession.Refresh(entity);
        }

        #region Methods to be implemented

        /// <summary>
        /// Returns the Database Mapper to be used in this Dao implementation.
        /// The Database Mapper holds the database information where the entity
        /// manipulated by this Dao is stored. With this Database Mapper, the parameterless
        /// constructor of this Dao can retrieve the correct ISession used by the NHibernate to
        /// do its job in the database.
        /// </summary>
        /// <returns></returns>
        public abstract INHibernateMapper GetINHibernateMapper();

        #endregion

        #region TemplateDao methods

        

        

        #endregion
    }
}
