
using Cerulean.Common;
using Cerulean.Core;
using Cerulean.Core.Input;

namespace Cerulean.Components
{
    public sealed class Pointer : Component
    {
        private int _x;
        private int _y;
        public override int X
        {
            get { return _x; }
        }
        public override int Y
        {
            get { return _y; }
        }

        public Pointer()
        {
            CanBeParent = false;
        }

        public override void Update(object? window, Size clientArea)
        {
            (int globalX, int globalY) = Mouse.GetGlobalMousePosition();
            ClientArea = clientArea;
            if (window is Window ceruleanWindow)
            {
                (int windowX, int windowY) = ceruleanWindow.WindowPosition;
                _x = globalX - windowX;
                _y = globalY - windowY;
            }
            else
            {
                _x = globalX;
                _y = globalY;
            }
        }
    }
}
