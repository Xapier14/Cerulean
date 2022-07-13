
using Cerulean.Common;
using Cerulean.Core;
using Cerulean.Core.Input;

namespace Cerulean.Components
{
    public sealed class Pointer : Component
    {
        private int _x;
        private int _y;
        public override int X => _x;
        public override int Y => _y;

        public Pointer()
        {
            CanBeParent = false;
        }

        public override void Update(object? window, Size clientArea)
        {
            var ( globalX, globalY) = Mouse.GetGlobalMousePosition();
            ClientArea = null;
            if (window is Window ceruleanWindow)
            {
                var ( windowX, windowY) = ceruleanWindow.WindowPosition;
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