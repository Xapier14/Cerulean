
using Cerulean.Common;
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

        public override void Update(IWindow window, Size clientArea)
        {
            CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            var (globalX, globalY) = Mouse.GetGlobalMousePosition();
            ClientArea = null;
            var (windowX, windowY) = window.WindowPosition;
            _x = globalX - windowX;
            _y = globalY - windowY;

            CallHook(this, EventHook.AfterUpdate, window, clientArea);
        }
    }
}