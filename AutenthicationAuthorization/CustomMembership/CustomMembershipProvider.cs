using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using AutenthicationAuthorization.VO;
using AutenthicationAuthorization.DAO;
using NHibernateManager.Helper;
using NHibernateManager.WebApp;
using System.Web.Configuration;
using System.Configuration;
using AutenthicationAuthorization.Security;
using System.Collections.Specialized;
using System.Web.Profile;
using System.Configuration.Provider;
using System.Text.RegularExpressions;
using System.Transactions;

namespace AutenthicationAuthorization.CustomMembership
{
    [Serializable]
    public class CustomMembershipProvider : MembershipProvider
    {
        private String appName = null;
        private String providerName = null;
        private MembershipApplication membershipApplication = null;
        private NameValueCollection providerConfig = null;

        private enum FailureType
        {
            PASSWORD,
            PASSWORD_ANSWER
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (name == null)
            {
                throw new ArgumentNullException("The name cannot be null");
            }

            if (name.Trim().Length == 0)
            {
                throw new ArgumentException("The name cannot be empty");
            }

            // cannot be initialized more than once
            if (providerName != null)
            {
                throw new InvalidOperationException("This provider is being initialized more than once");
            }

            if (config == null)
            {
                config = new NameValueCollection();
            }

            base.Initialize(name, config);
            providerName = name;
            this.providerConfig = config;

            // get app name. in case it does not exist, create one
            appName = providerConfig.Get("applicationName");
            if (appName == null)
            {
                appName = "CustomMembershipApplicationName";
            }
            MembershipApplicationDAO memebershipApplicationDAO = new MembershipApplicationDAO();
            membershipApplication = memebershipApplicationDAO.FindByName(appName);
            if (membershipApplication == null)
            {
                membershipApplication = new MembershipApplication();
                membershipApplication.Name = appName;
                memebershipApplicationDAO.SaveOrUpdate(membershipApplication);
            }
        }

        public override string ApplicationName
        {
            get
            {
                if (appName == null)
                {
                    appName = providerConfig.Get("applicationName");
                }

                MembershipApplicationDAO memebershipApplicationDAO = new MembershipApplicationDAO();
                membershipApplication = memebershipApplicationDAO.FindByName(appName);
                if (membershipApplication == null)
                {
                    membershipApplication = new MembershipApplication();
                    membershipApplication.Name = appName;
                    memebershipApplicationDAO.SaveOrUpdate(membershipApplication);
                }

                return appName;
            }
            set
            {
                // in case the entered app is not created, do it
                MembershipApplicationDAO membershipDAO = new MembershipApplicationDAO();
                MembershipApplication membership = membershipDAO.FindByName(value);
                if (membership == null)
                {
                    membership = new MembershipApplication()
                    {
                        Name = value
                    };
                    membershipDAO.SaveOrUpdate(membership);
                }
                providerConfig.Set("applicationName", value);
                appName = value;
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            bool hasChanged = false;
            bool isValid = true;
            String hashedNewPassword = String.Empty;
            String hashedEnteredOldPassword = String.Empty;

            isValid = ValidateUser(username, oldPassword);

            if (newPassword == null || newPassword.Trim().Length == 0)
            {
                isValid = false;
            }

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);
            OnValidatingPassword(new ValidatePasswordEventArgs(username, newPassword, true));
            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                {
                    throw args.FailureInformation;
                }
                else
                {
                    throw new MembershipPasswordException("The new password does not meet the defined standerds.");
                }
            }

            if (isValid)
            {
                CustomUserDAO userDAO = new CustomUserDAO();
                CustomUser user = userDAO.FindByName(username, ApplicationName);
                if (user != null)
                {
                    // hash entered password
                    hashedEnteredOldPassword = MembershipEncryptionManager.Encrypt(oldPassword, PasswordFormat);
                    // validate hashed entered password (oldpassword) with the user password
                    if (hashedEnteredOldPassword.Equals(user.Password))
                    {
                        // old entered password is valid, thus hash the new one and save the user
                        hashedNewPassword = MembershipEncryptionManager.Encrypt(newPassword, PasswordFormat);
                        user.Password = hashedNewPassword;
                        user.LastPasswordChangedDate = DateTime.Now;
                        userDAO.SaveOrUpdate(user);
                        hasChanged = true;
                    }
                }
            }
            return hasChanged;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            bool changed = false;

            if (username != null && password != null && newPasswordQuestion != null && newPasswordAnswer != null)
            {
                if (ValidateUser(username, password))
                {
                    CustomUserDAO userDAO = new CustomUserDAO();
                    CustomUser user = userDAO.FindByName(username, ApplicationName);
                    if (user != null)
                    {
                        user.PasswordQuestion = newPasswordQuestion;
                        user.PasswordAnswer = newPasswordAnswer;
                        userDAO.SaveOrUpdate(user);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            bool isValid = true;
            MembershipUser newUser = null;
            status = MembershipCreateStatus.Success;

            DateTime currentDate = DateTime.Now;

            if (username == null || username.Trim().Length == 0)
            {
                status = MembershipCreateStatus.InvalidUserName;
                isValid = false;
            }

            if (isValid && (password == null || password.Trim().Length == 0))
            {
                status = MembershipCreateStatus.InvalidPassword;
                isValid = false;
            }

            // only validate the e-mail format in case it is neither null nor empty
            if (isValid && !String.IsNullOrWhiteSpace(email))
            {
                String strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

                Regex regex = new Regex(strRegex);

                if (!regex.IsMatch(email))
                {
                    status = MembershipCreateStatus.InvalidEmail;
                    isValid = false;
                }
            }

            CustomUserDAO userDAO = new CustomUserDAO();

            /*
             * Validate e-mail for the case where it is required to be unique. Here the
             * API states that if so, there MUST be an e-mail provided, as stated in:
             * http://msdn.microsoft.com/en-us/library/d8t4h2es.aspx
             * 
             * You can see that in the following part:
             * 
             * "The SqlMembershipProvider provides an option to require a unique e-mail
             * address for each user. If the RequiresUniqueEmail property is true, you 
             * will need to use one of the CreateUser overloads that allows you to specify 
             * an e-mail address for the user being created. Otherwise, a 
             * MembershipCreateUserException will be thrown."
             * 
             */
            if (isValid && RequiresUniqueEmail)
            {
                // the e-mail cannot be null nor empty
                if (String.IsNullOrWhiteSpace(email))
                {
                    status = MembershipCreateStatus.InvalidEmail;
                    isValid = false;
                }

                // since the validation above passed, check whether the e-mail is unique
                if (isValid)
                {
                    CustomUser sameEmailUser = userDAO.FindByEmail(email, this.ApplicationName);
                    if (sameEmailUser != null)
                    {
                        status = MembershipCreateStatus.DuplicateEmail;
                        isValid = false;
                    }
                }
            }

            // validates password
            if (isValid)
            {
                ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);
                OnValidatingPassword(new ValidatePasswordEventArgs(username, password, true));
                if (args.Cancel)
                {
                    status = MembershipCreateStatus.InvalidPassword;
                    isValid = false;
                }
            }

            if (providerUserKey == null)
            {
                providerUserKey = Guid.NewGuid();
            }
            else if (!(providerUserKey is Guid))
            {
                status = MembershipCreateStatus.InvalidProviderUserKey;
                isValid = false;
            }

            if (isValid)
            {
                CustomUser sameNameUser = userDAO.FindByName(username, this.appName);
                if (sameNameUser != null)
                {
                    status = MembershipCreateStatus.DuplicateUserName;
                    isValid = false;
                }
            }

            if (isValid)
            {
                CustomUser user = new CustomUser(this.membershipApplication, 
                                                 (Guid) providerUserKey,
                                                 username, 
                                                 MembershipEncryptionManager.Encrypt(password, PasswordFormat), 
                                                 email, 
                                                 passwordQuestion, 
                                                 passwordAnswer, 
                                                 isApproved, 
                                                 false, 
                                                 currentDate, 
                                                 currentDate, 
                                                 currentDate, 
                                                 currentDate);
                userDAO.SaveOrUpdate(user);
                newUser = CreateMembershipUser(user);
                status = MembershipCreateStatus.Success;
            }
            return newUser;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            bool isUserDeleted = false;

            if (username == null)
            {
                throw new ArgumentNullException("The entered username parameter is null.");
            }

            if (username.Trim().Length == 0 || username.Contains(","))
            {
                throw new ArgumentException("The entered parameter username is either empty or contains a comma character.");
            }

            if (username != null && username.Trim().Length > 0)
            {
                using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required))
                {
                    CustomUserDAO userDAO = new CustomUserDAO();
                    if (deleteAllRelatedData)
                    {
                        // delete related profile
                        try
                        {
                            ProfileManager.DeleteProfile(username);
                        }
                        catch (Exception)
                        {
                            // an exception occurs, is because the profile
                            // was mal-configured and not at all
                        }

                        /*
                         * Although the API states that role information should be deleted,
                         * it was observed the the Microsoft .NET standard Membership provider
                         * mechanism, does not. Therefore, in this implementation no role will
                         * be deleted.
                         */ 
                    }

                    isUserDeleted = userDAO.DeleteByName(username, ApplicationName) > 0;

                    transaction.Complete();
                }
            }

            return isUserDeleted;
        }

        public override bool EnablePasswordReset
        {
            get { return providerConfig.Get("enablePasswordReset") != null ? Convert.ToBoolean(providerConfig.Get("enablePasswordReset")) : true; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return providerConfig.Get("enablePasswordRetrieval") != null ? Convert.ToBoolean(providerConfig.Get("enablePasswordRetrieval")) : true; }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();
            totalRecords = 0;

            if (emailToMatch != null)
            {

                CustomUserDAO userDAO = new CustomUserDAO();
                IList<CustomUser> users = userDAO.FindByEmailMatch(emailToMatch, ApplicationName, pageIndex, pageSize);

                if (users != null && users.Count > 0)
                {
                    foreach (CustomUser user in users)
                    {
                        membershipUsers.Add(CreateMembershipUser(user));
                    }
                }

                totalRecords = membershipUsers.Count;
            }
            return membershipUsers;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();
            totalRecords = 0;

            if (usernameToMatch != null)
            {
                CustomUserDAO userDAO = new CustomUserDAO();
                IList<CustomUser> users = userDAO.FindByNameMatch(usernameToMatch, ApplicationName, pageIndex, pageSize);

                if (users != null && users.Count > 0)
                {
                    foreach (CustomUser user in users)
                    {
                        membershipUsers.Add(CreateMembershipUser(user));
                    }
                }

                totalRecords = membershipUsers.Count;
            }
            return membershipUsers;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            CustomUserDAO userDAO = new CustomUserDAO();
            IList<CustomUser> users = userDAO.FindAll(ApplicationName, pageIndex, pageSize);

            MembershipUserCollection membershipUsers = new MembershipUserCollection();

            if (users != null && users.Count > 0)
            {
                foreach (CustomUser user in users)
                {
                    membershipUsers.Add(CreateMembershipUser(user));
                }
            }

            totalRecords = userDAO.Count();
            return membershipUsers;
        }

        public override int GetNumberOfUsersOnline()
        {
            int onlineUsers = 0;

            int userIsOnlineTimeWindow =  Membership.UserIsOnlineTimeWindow;
            if (userIsOnlineTimeWindow > 0)
            {
                DateTime onlieDate = DateTime.Now.AddMinutes(userIsOnlineTimeWindow);
                CustomUserDAO userDAO = new CustomUserDAO();
                onlineUsers = userDAO.FindActiveUsersCount(onlieDate, ApplicationName);
            }

            return onlineUsers;
        }

        public override string GetPassword(string username, string answer)
        {
            String passowrd = null;
            if (!EnablePasswordRetrieval)
            {
                throw new NotSupportedException("The property EnablePasswordRetrieval is set to false thus, password retrieval is not allowed.");
            }

            CustomUserDAO userDAO = new CustomUserDAO();
            CustomUser user = userDAO.FindByName(username, ApplicationName);

            if (user == null)
            {
                throw new ArgumentNullException("The entered username does not represent a user");
            }

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new NotSupportedException("The password format is Hashed thus it cannot be retrieved");
            }

            if (RequiresQuestionAndAnswer)
            {
                if (answer == null || user.PasswordAnswer == null || !answer.Equals(user.PasswordAnswer))
                {
                    // 1 more count for failure
                    UpdateFailureCount(username, FailureType.PASSWORD_ANSWER);
                    throw new MembershipPasswordException("The entered password neither is null or does not match the correct one.");
                }
            }

            passowrd = MembershipEncryptionManager.Decrypt(user.Password, PasswordFormat);
            
            return passowrd;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            MembershipUser membershipUser = null;
            if (username != null)
            {
                CustomUserDAO userDAO = new CustomUserDAO();
                CustomUser user = userDAO.FindByName(username, ApplicationName);
                if (user != null)
                {
                    // update last activity date in case the user is to be considered online
                    if (userIsOnline)
                    {
                        user.LastActivityDate = DateTime.Now;
                        userDAO.SaveOrUpdate(user);
                    }
                    
                    membershipUser = CreateMembershipUser(user);
                }
            }
            return membershipUser;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            MembershipUser membershipUser = null;
            if (providerUserKey != null)
            {
                CustomUserDAO userDAO = new CustomUserDAO();
                CustomUser user = userDAO.LoadById((Guid) providerUserKey);
                if (user != null)
                {
                    if (userIsOnline)
                    {
                        user.LastActivityDate = DateTime.Now;
                        userDAO.SaveOrUpdate(user);
                    }
                    membershipUser = CreateMembershipUser(user);
                }
            }
            return membershipUser;
        }

        public override string GetUserNameByEmail(string email)
        {
            String userName = null;
            if (email != null)
            {
                CustomUserDAO userDAO = new CustomUserDAO();
                userName = userDAO.FindByEmail(email, ApplicationName).Name;
            }

            return userName;
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return providerConfig.Get("maxInvalidPasswordAttempts") != null ? Convert.ToInt32(providerConfig.Get("maxInvalidPasswordAttempts")) : 5; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return providerConfig.Get("minRequiredNonAlphanumericCharacters") != null ? Convert.ToInt32(providerConfig.Get("minRequiredNonAlphanumericCharacters")) : 0; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return providerConfig.Get("minRequiredPasswordLength") != null ? Convert.ToInt32(providerConfig.Get("minRequiredPasswordLength")) : 6; }
        }

        public override int PasswordAttemptWindow
        {
            get { return providerConfig.Get("passwordAttemptWindow") != null ? Convert.ToInt32(providerConfig.Get("passwordAttemptWindow")) : 10; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get 
            {
                MembershipPasswordFormat passwordFormatEnum = MembershipPasswordFormat.Encrypted;

                String passwordFormat = providerConfig.Get("passwordFormat");
                if (passwordFormat != null)
                {
                    passwordFormatEnum = (MembershipPasswordFormat) Enum.Parse(typeof(MembershipPasswordFormat), passwordFormat);
                }
                return passwordFormatEnum; 
            }

        }

        public override string PasswordStrengthRegularExpression
        {
            get { return providerConfig.Get("passwordStrengthRegularExpression"); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return providerConfig.Get("requiresQuestionAndAnswer") != null ? Convert.ToBoolean(providerConfig.Get("requiresQuestionAndAnswer")) : false; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return providerConfig.Get("requiresUniqueEmail") != null ? Convert.ToBoolean(providerConfig.Get("requiresUniqueEmail")) : false; }
        }

        public override string ResetPassword(string username, string answer)
        {
            if (username == null || username.Trim().Length == 0)
            {
                throw new ArgumentException("The username cannot be null nor empty");
            }

            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("The property EnablePasswordReset is false thus the password cannot be reset.");
            }

            CustomUserDAO userDAO = new CustomUserDAO();
            CustomUser user = userDAO.FindByName(username, ApplicationName);

            if (user == null)
            {
                throw new ArgumentException("The entered username does not represent any user.");
            }

            if (RequiresQuestionAndAnswer)
            {
                if (answer == null || user.PasswordAnswer == null || !answer.Equals(user.PasswordAnswer))
                {
                    // 1 more count for failure
                    UpdateFailureCount(username, FailureType.PASSWORD_ANSWER);

                    throw new MembershipPasswordException("The entered password neither is null or does not match the correct one.");
                }
            }

            String newPassword = Membership.GeneratePassword(4, 2);

            bool isPasswordOK = false;
            int counter = 0;
            ValidatePasswordEventArgs args = null;
            while (!isPasswordOK || counter < 50)
            {
                args = new ValidatePasswordEventArgs(username, newPassword, true);
                OnValidatingPassword(new ValidatePasswordEventArgs(username, newPassword, true));
                if (!args.Cancel)
                {
                    isPasswordOK = true;
                }
                counter++;
            }

            if (!isPasswordOK)
            {
                if (args.FailureInformation != null)
                {
                    throw args.FailureInformation;
                }
                else
                {
                    throw new MembershipPasswordException("There could not be created a password according to the defined standerds.");
                }
            }

            user.Password = MembershipEncryptionManager.Encrypt(newPassword, PasswordFormat);
            user.IsLockedOut = false;
            user.LastPasswordChangedDate = DateTime.Now;
            userDAO.SaveOrUpdate(user);
            
            return newPassword;
        }

        public override bool UnlockUser(string userName)
        {
            bool operationOk = false;
            if (userName != null && userName.Trim().Length > 0)
            {
                CustomUserDAO userDAO = new CustomUserDAO();
                CustomUser user = userDAO.FindByName(userName, ApplicationName);
                if (user != null)
                {
                    user.IsLockedOut = false;
                    user.LastLockedOutDate = DateTime.Now;
                    userDAO.SaveOrUpdate(user);
                    operationOk = true;
                }
            }
            return operationOk;
        }

        public override void UpdateUser(MembershipUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("The user cannot be null");
            }

            CustomUserDAO userDAO = new CustomUserDAO();
            CustomUser customUser = userDAO.FindByName(user.UserName, ApplicationName);
            if (customUser != null)
            {
                customUser.Name = user.UserName;
                customUser.Email = user.Email;
                customUser.Comment = user.Comment;
                customUser.PasswordQuestion = user.PasswordQuestion;
                customUser.IsApproved = user.IsApproved;
                customUser.LastActivityDate = user.LastActivityDate;
                customUser.LastLoginDate = user.LastLoginDate;
                customUser.LastPasswordChangedDate = user.LastPasswordChangedDate;
                customUser.CreationDate = user.CreationDate;
                customUser.IsLockedOut = user.IsLockedOut;
                if (user.LastLockoutDate > DateTime.MinValue)
                {
                    customUser.LastLockedOutDate = user.LastLockoutDate;
                }
                userDAO.SaveOrUpdate(customUser);
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;

            if (username == null || username.Trim().Length == 0)
            {
                throw new HttpException("The username cannot be neither null nor empty.");
            }

            if (password != null)
            {
                String hashedEnteredOldPassword = null;
                CustomUserDAO userDAO = new CustomUserDAO();
                CustomUser user = userDAO.FindByName(username, ApplicationName);

                if (user != null)
                {
                    DateTime currentDate = DateTime.Now;

                    // update last activity date
                    user.LastActivityDate = currentDate;
                    userDAO.SaveOrUpdate(user);

                    if ((!user.IsApproved.HasValue || user.IsApproved.Value) && (!user.IsLockedOut.HasValue || !user.IsLockedOut.Value))
                    {
                        // hash entered password
                        hashedEnteredOldPassword = MembershipEncryptionManager.Encrypt(password, PasswordFormat);
                        // validate hashed entered password with the user password
                        isValid = hashedEnteredOldPassword.Equals(user.Password);
                        if (isValid)
                        {
                            // update last login date
                            user.LastLoginDate = currentDate;
                            userDAO.SaveOrUpdate(user);
                        }
                    }
                }
            }

            if (!isValid)
            {
                // 1 more count for failure
                UpdateFailureCount(username, FailureType.PASSWORD);
            }

            return isValid;
        }

        private void UpdateFailureCount(String userName, FailureType failureType)
        {
            switch (failureType)
            {
                case FailureType.PASSWORD:
                    UpdateFailureForPassword(userName);
                    break;
                case FailureType.PASSWORD_ANSWER:
                    UpdateFailureForPasswordAnswer(userName);
                    break;
            }
        }

        private void UpdateFailureForPassword(String userName)
        {
            CustomUserDAO userDAO = new CustomUserDAO();
            CustomUser user = userDAO.FindByName(userName, ApplicationName);

            if (user != null)
            {
                int failutreCount = user.FailedPasswordAttemptCount.HasValue ? user.FailedPasswordAttemptCount.Value : 0;
                DateTime failureWindowStart = user.FailedPasswordAttemptWindowStart.HasValue ? user.FailedPasswordAttemptWindowStart.Value : DateTime.Now;
                if (failutreCount == 0 || DateTime.Now > failureWindowStart.AddMinutes(PasswordAttemptWindow))
                {
                    user.FailedPasswordAttemptCount = 1;
                    user.FailedPasswordAttemptWindowStart = DateTime.Now;
                }
                else if (failutreCount < MaxInvalidPasswordAttempts)
                {
                    failutreCount++;
                    user.FailedPasswordAttemptCount = failutreCount;
                }
                else
                {
                    user.IsLockedOut = true;
                    user.LastLockedOutDate = DateTime.Now;
                }
                userDAO.SaveOrUpdate(user);
            }
        }

        private void UpdateFailureForPasswordAnswer(String userName)
        {
            CustomUserDAO userDAO = new CustomUserDAO();
            CustomUser user = userDAO.FindByName(userName, ApplicationName);

            if (user != null)
            {
                int failutreCount = user.FailedPasswordAnswerAttemptCount.HasValue ? user.FailedPasswordAnswerAttemptCount.Value : 0;
                DateTime failureWindowStart = user.FailedPasswordAnswerAttemptWindowStart.HasValue ? user.FailedPasswordAnswerAttemptWindowStart.Value : DateTime.Now;
                if (failutreCount == 0 || DateTime.Now > failureWindowStart.AddMinutes(PasswordAttemptWindow))
                {
                    user.FailedPasswordAnswerAttemptCount = 1;
                    user.FailedPasswordAnswerAttemptWindowStart = DateTime.Now;
                }
                else if (failutreCount < MaxInvalidPasswordAttempts)
                {
                    failutreCount++;
                    user.FailedPasswordAnswerAttemptCount = failutreCount;
                }
                else
                {
                    user.IsLockedOut = true;
                    user.LastLockedOutDate = DateTime.Now;
                }
                userDAO.SaveOrUpdate(user);
            }
        }

        private MembershipUser CreateMembershipUser(CustomUser user)
        {
            DateTime theDateTime = new DateTime();
            return new MembershipUser(providerName, 
                                      user.Name, 
                                      user.Id, 
                                      user.Email, 
                                      user.PasswordQuestion, 
                                      user.Comment, 
                                      user.IsApproved.HasValue ? user.IsApproved.Value : true,
                                      user.IsLockedOut.HasValue ? user.IsLockedOut.Value : false,
                                      user.CreationDate = user.CreationDate,
                                      user.LastLoginDate.HasValue ? user.LastLoginDate.Value : theDateTime,
                                      user.LastActivityDate.HasValue ? user.LastActivityDate.Value : theDateTime,
                                      user.LastPasswordChangedDate.HasValue ? user.LastPasswordChangedDate.Value : theDateTime,
                                      user.LastLockedOutDate.HasValue ? user.LastLockedOutDate.Value : theDateTime);
        }
    }
}
