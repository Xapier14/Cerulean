using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Cerulean.Test
{
    internal class MockGraphicsFactory : IGraphicsFactory
    {
        public IGraphics CreateGraphics(Window window)
        {
            return new MockGraphics();
        }
    }
    internal class MockGraphics : IGraphics
    {
        private Size _renderSize = new();
        private int _renderX = 0;
        private int _renderY = 0;
        private int _globalX = 0;
        private int _globalY = 0;
        public Size GetRenderArea(out int x, out int y)
        {
            x = _renderX;
            y = _renderY;
            return _renderSize;
        }

        public void SetRenderArea(Size renderArea, int x, int y)
        {
            _renderSize = renderArea;
            _renderX = x;
            _renderY = y;
        }

        public void GetGlobalPosition(out int x, out int y)
        {
            x = _globalX;
            y = _globalY;
        }

        public void SetGlobalPosition(int x, int y)
        {
            _globalX = x;
            _globalY = y;
        }

        public void RenderClear()
        {
        }

        public void RenderPresent()
        {
        }

        public void DrawRectangle(int x, int y, Size size)
        {
        }

        public void DrawRectangle(int x, int y, Size size, Color color)
        {
        }

        public void DrawFilledRectangle(int x, int y, Size size)
        {
        }

        public void DrawFilledRectangle(int x, int y, Size size, Color color)
        {
        }

        public void DrawImage(int x, int y, Size size, string fileName, PictureMode pictureMode = PictureMode.None,
            double opacity = 1)
        {
        }

        public void DrawText(int x, int y, string text, string fontName, string fontStyle, int fontPointSize, Color color,
            uint textWrap = 0, double angle = 0)
        {
        }

        public (int, int) MeasureText(string text, string fontName, string fontStyle, int fontPointSize, int textWrap = 0)
        {
            return (0, 0);
        }

        public void Update()
        {
        }
    }
}
