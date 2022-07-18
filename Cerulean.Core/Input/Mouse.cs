
using static SDL2.SDL;

namespace Cerulean.Core.Input
{
    public static class Mouse
    {
        public static (int, int) GetGlobalMousePosition()
        {
            SDL_GetGlobalMouseState(out var x, out var y);
            return (x, y);
        }
    }
}
