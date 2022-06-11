using Cerulean.Common;

namespace Cerulean.Core
{
    public class Layout : Component
    {
        public Layout()
        {
            CanBeChild = false;
            CanBeParent = true;
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Update(Size clientArea)
        {
            // update our client area first before the child components
            ClientArea = clientArea;

            // update the child components
            base.Update(clientArea);
        }
    }
}