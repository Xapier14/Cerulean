namespace Cerulean.Common
{
    public interface IGraphics
    {
        #region GENERAL
        public Size GetRenderArea(out int x, out int y);
        public void SetRenderArea(Size renderArea, int x, int y);
        #endregion

        #region RENDER
        public void RenderClear();
        public void RenderPresent();
        #endregion

        #region PRIMITIVES
        public void DrawRectangle(int x, int y, Size size);
        public void DrawRectangle(int x, int y, Size size, Color color);
        public void DrawFilledRectangle(int x, int y, Size size);
        public void DrawFilledRectangle(int x, int y, Size size, Color color);
        #endregion

        #region TEXTURE
        public void DrawImage(int x, int y, Size size, string fileName, PictureMode pictureMode = PictureMode.None, double opacity = 1.0);
        // DrawImageFromBytes(byte[])
        // DrawImageFromStream(Srteam)
        #endregion

        #region TEXT
        public void DrawText(int x, int y, string text, string fontName, string fontStyle, int fontPointSize, Color color, uint textWrap = 0, double angle = 0.0);
        #endregion

        #region API FUNCTIONS
        public void Update();
        public virtual void Cleanup()
        {

        }
        #endregion
    }
}
