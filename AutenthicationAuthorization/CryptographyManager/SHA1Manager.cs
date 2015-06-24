using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Cryptography.Exception;

namespace Cryptography
{
    public class SHA1Manager
    {
        public static string Encript(string entry)
        {
            string hashedEntry = String.Empty;
            try
            {

                SHA1 hash = SHA1.Create();
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] combined = encoder.GetBytes(entry);
                hash.ComputeHash(combined);
                hashedEntry = Convert.ToBase64String(hash.Hash);
            }
            catch (System.Exception ex)
            {
                throw new SHA1Exception("Problems hashing string", ex);
            }
            return hashedEntry;
        }  
    }
}
