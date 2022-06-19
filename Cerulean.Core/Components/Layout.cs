using Cerulean.Common;

namespace Cerulean.Core
{
    public class Layout : Component
    {
        public dynamic This => this;
        public Layout()
        {
            CanBeChild = false;
            CanBeParent = true;
        }

        public override void Update(object? window, Size clientArea)
        {
            // update our client area first before the child components
            ClientArea = clientArea;

            // update the child components
            base.Update(window, clientArea);
        }
    }
}