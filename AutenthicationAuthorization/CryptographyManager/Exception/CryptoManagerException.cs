using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptography.Exception
{
    class CryptoManagerException : ApplicationException
    {
        public CryptoManagerException(String message, System.Exception ex) : base(message, ex) { }
    }
}
