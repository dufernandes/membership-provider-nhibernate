using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using Cryptography.Exception;

namespace Cryptography
{
    /// <summary>
    /// This class has the necessary functions to encrypt/decrypt information.
    /// In this class is used MD5 algorithm to hash the key and 3DES to perform
    /// encrypt/decrypt operations.
    /// </summary>
    public class CryptoManager
    {
        /// <summary>
        /// Criptography key that will be used on cryptographic process
        /// </summary>
        private String _cryptographyKey;

        /// <summary>
        /// Flag indication if a hash process must be used to key
        /// </summary>
        private bool _useHashing;

        public CryptoManager()
            : this("AnyApplicationOn", true)
        {
        }

        /// <summary>
        /// Constructor that initializes the cryptgraphic key
        /// </summary>
        /// <param name="String"> key to be used on cryptographic process </param>
        /// <param name="bool"> flag indicating if the hashing process to key must be used</param>
        public CryptoManager(String cryptographyKey, bool useHashing)
        {
            _cryptographyKey = cryptographyKey;
            _useHashing = useHashing;
        }

        /// <summary>
        /// This method is responsible by encrypting some information.
        /// </summary>
        /// <param name="String"> information to be encrypted</param>
        /// <returns> String - encrypted information</returns>
        /// <exception cref="eldorado.ipelibrary.exception.CryptoManagerException"/>
        public String Encrypt(String chainToEncrypt)
        {
            byte[] encryptedChain = null;

            // applying hash process to key, if necessary
            byte[] keyArray = ProcessHashKey();

            // performing encryptation process
            byte[] encodedChainToEncrypt = UTF8Encoding.UTF8.GetBytes(chainToEncrypt);

            // crypto provider
            TripleDESCryptoServiceProvider cryptoProvider = null;
            try
            {
                // preparing 3DES Algorithm provider
                cryptoProvider = new TripleDESCryptoServiceProvider();
                cryptoProvider.Key = keyArray;
                cryptoProvider.Mode = CipherMode.ECB;
                cryptoProvider.Padding = PaddingMode.PKCS7;

                // encrypting information
                ICryptoTransform cryptoTransform = cryptoProvider.CreateEncryptor();

                //transform the specified region of bytes array to resultArray
                encryptedChain = cryptoTransform.TransformFinalBlock(encodedChainToEncrypt, 0, encodedChainToEncrypt.Length);
            }
            catch (CryptographicException ce)
            {
                throw new CryptoManagerException(ce.Message, ce);
            }
            finally
            {
                if (cryptoProvider != null)
                {
                    // Release resources held by 3DES Provider
                    cryptoProvider.Clear();
                }
            }

            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(encryptedChain, 0, encryptedChain.Length);
        }

        /// <summary>
        /// This method is responsible by decrypting some information.
        /// </summary>
        /// <param name="String"> information to be decrypted</param>
        /// <returns> String - decrypted information</returns>
        /// <exception cref="eldorado.ipelibrary.exception.CryptoManagerException"/>
        public String Decrypt(String encryptedChain)
        {
            byte[] decryptedChain = null;

            // applying hash process to key, if necessary
            byte[] keyArray = ProcessHashKey();

            // performing decryptation process
            byte[] encodedChainToDecrypt = Convert.FromBase64String(encryptedChain);

            // crypto provider
            TripleDESCryptoServiceProvider cryptoProvider = null;
            try
            {
                // preparing 3DES Algorithm provider
                cryptoProvider = new TripleDESCryptoServiceProvider();
                cryptoProvider.Key = keyArray;
                cryptoProvider.Mode = CipherMode.ECB;
                cryptoProvider.Padding = PaddingMode.PKCS7;

                // encrypting information
                ICryptoTransform cryptoTransform = cryptoProvider.CreateDecryptor();

                //transform the specified region of bytes array to resultArray
                decryptedChain = cryptoTransform.TransformFinalBlock(encodedChainToDecrypt, 0, encodedChainToDecrypt.Length);
            }
            catch (CryptographicException ce)
            {
                throw new CryptoManagerException(ce.Message, ce);
            }
            finally
            {
                if (cryptoProvider != null)
                {
                    // Release resources held by 3DES Provider
                    cryptoProvider.Clear();
                }
            }

            return UTF8Encoding.UTF8.GetString(decryptedChain);
        }

        /// <summary>
        /// This method is used to apply a hashing process to chosen key 
        /// using a MD5 process
        /// </summary>
        /// <returns> key hashed using MD5</returns>
        private byte[] ProcessHashKey()
        {
            byte[] processedKey = null;

            //If hashing use get hashcode regards to your key
            if (_useHashing)
            {
                // providing hash to key
                MD5CryptoServiceProvider md5Hash = new MD5CryptoServiceProvider();
                processedKey = md5Hash.ComputeHash(UTF8Encoding.UTF8.GetBytes(_cryptographyKey));

                // Always release the resources and flush data of the Cryptographic 
                // service provide is a Best Practice
                md5Hash.Clear();
            }
            else
            {
                // just encoding the key wothout hash
                processedKey = UTF8Encoding.UTF8.GetBytes(_cryptographyKey);
            }

            return processedKey;
        }
    }
}
