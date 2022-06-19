namespace Cerulean.Common
{
    public interface IGraphics
    {
        #region INFORMATION
        public Size GetRenderArea(out int x, out int y);
        public void SetRenderArea(Size renderArea, int x, int y);
        #endregion

        #region TOOLS
        public void RenderClear(Color clearColor);
        public void RenderPresent();
        #endregion

        #region PRIMITIVE DRAW
        public void DrawFilledRectangle(int x, int y, Size size);
        public void DrawFilledRectangle(int x, int y, Size size, Color color);
        #endregion

        #region TEXTURE DRAW
        #endregion

        #region API FUNCTIONS
        public void Update();
        public virtual void Cleanup()
        {

        }
        #endregion
    }
}
