
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

        public static (int, int)? GetWindowMousePosition(Window window)
        {
            if (SDL_GetMouseFocus() != window.WindowPtr)
                return null;
            
            _ = SDL_GetMouseState(out var x, out var y);
            return (x, y);
        }
    }
}
