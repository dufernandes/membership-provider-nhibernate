using AutenthicationAuthorization.VO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Linq;
using System.Transactions;
using NHibernateManager.Dao;
using NHibernateManager.Helper;
using AutenthicationAuthorization.NHibernateManager;
using NHibernate.Criterion;

namespace AutenthicationAuthorization.DAO
{
    public class CustomUserDAO : BaseDao<CustomUser, Guid>
    {
        public override Guid Save(CustomUser entity)
        {
            Guid identifier = Guid.Empty;
            if (entity != null)
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    if (entity.Id != null)
                    {
                        CustomUser sameUser = LoadById(entity.Id);
                        if (sameUser == null)
                        {
                           identifier = (Guid) CurrentSession.Save(entity);
                        }
                        else
                        {
                            CurrentSession.Update(entity);
                            identifier = entity.Id;
                        }
                        CurrentSession.Flush();
                    }
                    else
                    {
                        throw new ArgumentNullException("It is not allowed to save a User without providing its Id");
                    }
                    transaction.Complete();
                }
            }
            return identifier;
        }

        public override Guid SaveOrUpdate(CustomUser entity)
        {
            Guid identifier = Guid.Empty;
            if (entity != null)
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    if (entity.Id != null)
                    {
                        CustomUser sameUser = LoadById(entity.Id);
                        if (sameUser == null)
                        {
                            identifier = (Guid)CurrentSession.Save(entity);
                        }
                        else
                        {
                            CurrentSession.Update(entity);
                            identifier = entity.Id;
                        }
                        CurrentSession.Flush();
                    }
                    else
                    {
                        throw new ArgumentNullException("It is not allowed to save a User without providing its Id");
                    }
                    transaction.Complete();
                }
            }
            return identifier;
        }

        public CustomUser FindByName(String userName, String applicationName)
        {
            CustomUser userCustom = null;

            var queryResult = CurrentSession.QueryOver<CustomUser>()
                               .And(users => users.Name == userName)
                               .JoinQueryOver(users => users.MembershipApplication)
                                    .Where(mp => mp.Name == applicationName);

            if (queryResult != null && queryResult.RowCount() > 0)
            {
                userCustom = queryResult.SingleOrDefault();
            }
            return userCustom;
        }

        public CustomUser FindByEmail(String email, String applicationName)
        {
            CustomUser userCustom = null;

            var queryResult = (from users in CurrentSession.Query<CustomUser>()
                               where users.MembershipApplication.Name == applicationName &&
                                     users.Email == email
                               select users);

            if (queryResult != null && queryResult.Count<CustomUser>() > 0)
            {
                userCustom = queryResult.First<CustomUser>();
            }
            return userCustom;
        }

        public Int32 DeleteByName(String userName, String applicationName)
        {
            Int32 count = 0;
            var queryResult = CurrentSession.QueryOver<CustomUser>()
                               .And(users => users.Name == userName)
                               .JoinQueryOver(users => users.MembershipApplication)
                                    .Where(mp => mp.Name == applicationName);

            if (queryResult != null && queryResult.RowCount() > 0)
            {
                count = queryResult.RowCount();
                IList<CustomUser> usersToBeDeleted = queryResult.List();
                foreach(CustomUser user in usersToBeDeleted) {
                    CurrentSession.Delete(user);
                }
                CurrentSession.Flush();
            }
            return count;
        }

        public IList<CustomUser> FindByEmailMatch(String emailMatch, String applicationName, int pageIndex, int pageSize)
        {
            var queryResult = CurrentSession.QueryOver<CustomUser>()
                               .WhereRestrictionOn(users => users.Email).IsLike(emailMatch, MatchMode.Anywhere)
                               .JoinQueryOver(users => users.MembershipApplication)
                                    .Where(mp => mp.Name == applicationName)
                                .Skip(pageIndex)
                                .Take(pageSize);

            return queryResult != null && queryResult.RowCount() > 0 ? queryResult.List() : new List<CustomUser>();
        }

        public IList<CustomUser> FindByNameMatch(String nameMatch, String applicationName, int pageIndex, int pageSize)
        {
            var queryResult = CurrentSession.QueryOver<CustomUser>()
                               .WhereRestrictionOn(users => users.Name).IsLike(nameMatch, MatchMode.Anywhere)
                               .JoinQueryOver(users => users.MembershipApplication)
                                    .Where(mp => mp.Name == applicationName)
                                .Skip(pageIndex)
                                .Take(pageSize);

            return queryResult != null && queryResult.RowCount() > 0 ? queryResult.List() : new List<CustomUser>();
        }

        public IList<CustomUser> FindAll(String applicationName, int pageIndex, int pageSize)
        {
            var queryResult = CurrentSession.QueryOver<CustomUser>()
                               .JoinQueryOver(users => users.MembershipApplication)
                                    .Where(mp => mp.Name == applicationName)
                                .Skip(pageIndex)
                                .Take(pageSize);

            return queryResult != null && queryResult.RowCount() > 0 ? queryResult.List() : new List<CustomUser>();
        }

        public int FindActiveUsersCount(DateTime activeDate, String applicationName)
        {

            var queryResult = CurrentSession.QueryOver<CustomUser>()
                               .Where(users => users.LastActivityDate >= activeDate)
                               .JoinQueryOver(users => users.MembershipApplication)
                                    .Where(mp => mp.Name == applicationName);

            return queryResult != null ? queryResult.RowCount() : 0;
        }

        public int Count()
        {
            return CurrentSession.Query<CustomUser>().Count();
        }

        public override INHibernateMapper GetINHibernateMapper()
        {
            return new NHibernateMembershipMapper();
        }
    }
}
