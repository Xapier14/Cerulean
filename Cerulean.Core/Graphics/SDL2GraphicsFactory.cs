using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cerulean.Common;

namespace Cerulean.Core
{
    public class SDL2GraphicsFactory : IGraphicsFactory
    {
        public IGraphics CreateGraphics(Window window)
        {
            return new SDL2Graphics(window);
        }
    }
}
