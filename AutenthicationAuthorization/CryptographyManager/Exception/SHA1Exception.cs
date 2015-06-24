using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptography.Exception
{
    class SHA1Exception : ApplicationException
    {
        public SHA1Exception(String message, System.Exception ex):base(message, ex){}
    }
}
