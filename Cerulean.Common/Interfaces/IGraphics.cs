using System.Security.Principal;

namespace Cerulean.Common
{
    public interface IGraphics
    {
        #region GENERAL
        public Size GetRenderArea(out int x, out int y);
        public void SetRenderArea(Size renderArea, int x, int y);
        public void GetGlobalPosition(out int x, out int y);
        public void SetGlobalPosition(int x, int y);
        #endregion

        #region RENDER
        public void RenderClear();
        public void RenderPresent();
        #endregion

        #region PRIMITIVES
        public void DrawLine(int x1, int y1, int x2, int y2);
        public void DrawLine(int x1, int y1, int x2, int y2, Color color);
        public void DrawRectangle(int x, int y, Size size);
        public void DrawRectangle(int x, int y, Size size, Color color);
        public void DrawFilledRectangle(int x, int y, Size size);
        public void DrawFilledRectangle(int x, int y, Size size, Color color);
        #endregion

        #region TEXTURE

        public void DrawImage(int x, int y, Size size, string fileName, PictureMode pictureMode = PictureMode.None,
            double opacity = 1.0);

        public void DrawImageFromBytes(int x, int y, Size size, byte[] bytes,
            PictureMode pictureMode = PictureMode.None, double opacity = 1.0);
        // DrawImageFromBytes(byte[])
        // DrawImageFromStream(Srteam)
        #endregion

        #region TEXT

        public void DrawText(int x, int y, string text, string fontName, string fontStyle, int fontPointSize,
            Color color, uint textWrap = 0, double angle = 0.0);

        public (int, int) MeasureText(string text, string fontName, string fontStyle, int fontPointSize,
            int textWrap = 0);
        #endregion

        #region Utilities

        public float GetCurrentDisplayDpi();

        #endregion

        #region API FUNCTIONS
        public void Update();
        public virtual void Cleanup()
        {

        }
        #endregion
    }
}
