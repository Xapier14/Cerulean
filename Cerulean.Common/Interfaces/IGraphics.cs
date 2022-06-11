using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public interface IGraphics
    {
        public Size GetRenderArea(out int x, out int y);
        public void SetRenderArea(Size renderArea, int x, int y);

        public void RenderClear();
        public void RenderPresent();
        public virtual void Cleanup()
        {

        }
    }
}
