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

        public override void Update(Size clientArea)
        {
            base.Update(clientArea);
        }

        public override void Init()
        {
            base.Init();
        }
    }
}