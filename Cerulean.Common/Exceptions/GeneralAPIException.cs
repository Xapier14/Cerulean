using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public class GeneralAPIException : Exception
    {
        public GeneralAPIException() : base()
        {
        }

        public GeneralAPIException(string message) : base(message)
        {
        }
    }
}
