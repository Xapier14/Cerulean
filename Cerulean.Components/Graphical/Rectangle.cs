using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cerulean.Core;
using Cerulean.Common;

namespace Cerulean.Components
{
    public class Rectangle : Component
    {
        public Rectangle()
        {

        }

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = clientArea;
        }

        public override void Draw(IGraphics graphics)
        {
            base.Draw(graphics);
            if (ClientArea is Size area)
                graphics.DrawRectangle(0, 0, area);
        }
    }
}
