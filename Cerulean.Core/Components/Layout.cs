using Cerulean.Common;

namespace Cerulean.Core
{
    public class Layout : Component
    {
        public dynamic DynamicLayout => this;
        public Layout()
        {
            CanBeChild = false;
            CanBeParent = true;
        }

        public override void Update(object? window, Size clientArea)
        {
            CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("Layout_Update");
            // update our client area first before the child components
            ClientArea = clientArea;

            // update the child components
            base.Update(window, clientArea);
            CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();
        }
    }
}