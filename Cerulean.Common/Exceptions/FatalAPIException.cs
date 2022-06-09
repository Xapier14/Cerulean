using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public class FatalAPIException : Exception
    {
        public FatalAPIException() : base()
        {
        }

        public FatalAPIException(string message) : base(message)
        {
        }
    }
}
