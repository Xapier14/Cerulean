using System;
using System.Collections.Generic;
using System.Text;

namespace Cerulean.Common
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class LateBoundAttribute : Attribute
    {
    }
}
