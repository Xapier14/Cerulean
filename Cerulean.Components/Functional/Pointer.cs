using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cerulean.Core;
using Cerulean.Common;

namespace Cerulean.Components
{
    public sealed class Pointer : Component
    {
        private int _x;
        private int _y;
        public override int X
        {
            get { return _x; }
            set { _x = value; }
        }

        public Pointer()
        {
            CanBeParent = false;
        }

        public override void Update(object? window, Size clientArea)
        {
            var api = CeruleanAPI.GetAPI();
            ClientArea = clientArea;
            if (window is Window ceruleanWindow)
            {
                
            }
        }
    }
}
