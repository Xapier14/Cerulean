using Cerulean.Common;

namespace Cerulean.Core
{
    public class Layout : Component
    {
        private readonly Queue<(Component, Style)> _queuedStyles = new();
        public dynamic DynamicLayout => this;
        public Layout()
        {
            CanBeChild = false;
            CanBeParent = true;
        }

        public override void Update(IWindow window, Size clientArea)
        {
            CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("Layout_Update");
            // update our client area first before the child components
            ClientArea = clientArea;

            // update the child components
            base.Update(window, clientArea);
            CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();
        }

        protected void QueueStyle(Component component, Style? style)
        {
            if (style == null) return;
            _queuedStyles.Enqueue((component, style));
        }

        internal void ApplyStyles()
        {
            while (_queuedStyles.Any())
            {
                var (component, style) = _queuedStyles.Dequeue();
                style.ApplyStyle(component);
            }
        }
    }
}