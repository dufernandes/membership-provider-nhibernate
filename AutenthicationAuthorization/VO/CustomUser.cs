using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using NHibernateManager.VO;

namespace AutenthicationAuthorization.VO
{
    [Serializable]
    public class CustomUser : BaseVO<Guid>
    {
        public virtual String Name { get; set; }
        public virtual String Password { get; set; }
        public virtual String Email { get; set; }
        public virtual String Comment { get; set; }
        public virtual String PasswordQuestion { get; set; }
        public virtual String PasswordAnswer { get; set; }
        public virtual Boolean? IsApproved { get; set; }
        public virtual DateTime? LastActivityDate { get; set; }
        public virtual DateTime? LastLoginDate { get; set; }
        public virtual DateTime? LastPasswordChangedDate { get; set; }
        public virtual Boolean? IsOnline { get; set; }
        public virtual Boolean? IsLockedOut { get; set; }
        public virtual DateTime? LastLockedOutDate { get; set; }
        public virtual Int32? FailedPasswordAttemptCount { get; set; }
        public virtual DateTime? FailedPasswordAttemptWindowStart { get; set; }
        public virtual Int32? FailedPasswordAnswerAttemptCount { get; set; }
        public virtual DateTime? FailedPasswordAnswerAttemptWindowStart { get; set; }
        public virtual MembershipApplication MembershipApplication { get; set; }
        public virtual ICollection<CustomRole> Roles { get; set; }

        public CustomUser()
        {
        }

        public CustomUser(MembershipApplication membershipApplication, Guid id, string userName, string password, string email, string passwordQuestion, string passwordAnswer, bool? isApproved, bool? isLockedOut)
        {
            this.Id = id;
            this.MembershipApplication = membershipApplication;
            this.Name = userName;
            this.Password = password;
            this.Email = email;
            this.PasswordQuestion = passwordQuestion;
            this.PasswordAnswer = passwordAnswer;
            this.IsApproved = isApproved;
            this.IsLockedOut = isLockedOut;
        }

        public CustomUser(MembershipApplication membershipApplication, Guid id, string userName, string password, string email, string passwordQuestion, string passwordAnswer, bool? isApproved, bool? isLockedOut, DateTime creationDate, DateTime? lastActivityDate, DateTime? lastLoginDate, DateTime? lastPasswordChangedDate)
            : this(membershipApplication, id, userName, password, email, passwordQuestion, passwordAnswer, isApproved, isLockedOut)
        {
            this.CreationDate = creationDate;
            this.LastActivityDate = lastActivityDate;
            this.LastLoginDate = lastLoginDate;
            this.LastPasswordChangedDate = lastPasswordChangedDate;
        }

        public CustomUser(MembershipApplication membershipApplication, Guid id, String name, String password, String email, 
            String comment, String passwordQuestion, String passwordAnswer, Boolean? isApproved, DateTime? lastActivityDate, 
            DateTime? lastLoginDate, DateTime? lastPasswordChangedDate, DateTime creationDate, Boolean? isOnline, 
            Boolean? isLockedOut, DateTime? lastLockedOutDate, Int32? failedPasswordAttemptCount, DateTime? failedPasswordAttemptWindowStart, 
            Int32? failedPasswordAnswerAttemptCount, DateTime? failedPasswordAnswerAttemptWindowStart)
            : this(membershipApplication, id, name, password, email, passwordQuestion, passwordAnswer, isApproved, isLockedOut, creationDate, lastActivityDate, lastLoginDate, lastPasswordChangedDate)
        {
            this.Comment = comment;
            this.IsOnline = isOnline;
            this.LastLockedOutDate = lastLockedOutDate;
            this.FailedPasswordAttemptCount = failedPasswordAnswerAttemptCount;
            this.FailedPasswordAttemptWindowStart = failedPasswordAttemptWindowStart;
            this.FailedPasswordAnswerAttemptCount = failedPasswordAnswerAttemptCount;
            this.FailedPasswordAnswerAttemptWindowStart = failedPasswordAnswerAttemptWindowStart;
        }

        public override bool Equals(object obj)
        {
            return obj != null && 
                obj is CustomUser && 
                ((CustomUser)obj).Id.Equals(this.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.ToByteArray().Count() * 2;
        }
    }
}
