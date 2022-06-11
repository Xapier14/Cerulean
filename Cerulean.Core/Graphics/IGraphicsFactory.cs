using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cerulean.Common;

namespace Cerulean.Core
{
    public interface IGraphicsFactory
    {
        public IGraphics CreateGraphics(Window window);
    }
}
