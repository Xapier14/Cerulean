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
        public void DrawFilledRectangle(int x, int y, Size size);
        public void DrawFilledRectangle(int x, int y, Size size, Color color);
        #endregion

        #region TEXTURE DRAW
        // DrawImage()
        // DrawImageBMP(byte[])
        #endregion

        #region TEXT DRAW
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
