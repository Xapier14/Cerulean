using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SDL2.SDL;

namespace Cerulean.Core.Input
{
    public static class Mouse
    {
        public static (int, int) GetGlobalMousePosition()
        {
            SDL_GetGlobalMouseState(out int x, out int y);
            return (x, y);
        }
    }
}
