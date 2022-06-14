using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common
{
    public interface IGraphics
    {
        #region INFORMATION
        public Size GetRenderArea(out int x, out int y);
        public void SetRenderArea(Size renderArea, int x, int y);
        #endregion

        #region TOOLS
        public void RenderClear();
        public void RenderPresent();
        #endregion

        #region PRIMITIVE DRAW
        public void DrawRectangle(int x, int y, Size size);
        #endregion

        #region TEXTURE DRAW
        #endregion
        public virtual void Cleanup()
        {

        }
    }
}
