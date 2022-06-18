
using Cerulean.Common;

namespace Cerulean.Core
{
    public interface IGraphicsFactory
    {
        public IGraphics CreateGraphics(Window window);
    }
}
