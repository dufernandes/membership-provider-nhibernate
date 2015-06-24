using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Cryptography;

namespace AutenthicationAuthorization.Security
{
    public class MembershipEncryptionManager
    {
        private MembershipEncryptionManager()
        {

        }

        public static String Encrypt(String value, MembershipPasswordFormat format)
        {
            String encryptedValue = null;
            switch (format)
            {
                case MembershipPasswordFormat.Encrypted:
                    CryptoManager cryptoManager = new CryptoManager();
                    encryptedValue = cryptoManager.Encrypt(value);
                    break;
                case MembershipPasswordFormat.Hashed:
                    encryptedValue = SHA1Manager.Encript(value);
                    break;
                case MembershipPasswordFormat.Clear:
                    encryptedValue = value;
                    break;
            }
            return encryptedValue;
        }

        public static String Decrypt(String encryptedValue, MembershipPasswordFormat format)
        {
            String decryptedValue = null;
            switch (format)
            {
                case MembershipPasswordFormat.Encrypted:
                    CryptoManager cryptoManager = new CryptoManager();
                    decryptedValue = cryptoManager.Decrypt(encryptedValue);
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new NotSupportedException("Hashed encrypted values cannot be decrypted");
                case MembershipPasswordFormat.Clear:
                    decryptedValue = encryptedValue;
                    break;
            }
            return decryptedValue;
        }
    }
}
