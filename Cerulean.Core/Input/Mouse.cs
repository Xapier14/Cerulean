
using Cerulean.Common;
using static SDL2.SDL;

namespace Cerulean.Core.Input
{
    public static class Mouse
    {
        public static (int, int) GetGlobalMousePosition()
        {
            _ = SDL_GetGlobalMouseState(out var x, out var y);
            return (x, y);
        }

        public static (int, int)? GetWindowMousePosition(IWindow window)
        {
            if (SDL_GetMouseFocus() != window.WindowPtr)
                return null;
            
            _ = SDL_GetMouseState(out var x, out var y);
            return (x, y);
        }

        public static bool CheckMouseButton(MouseButton mouseButton)
        {
            var state = SDL_GetMouseState(out _, out _);
            var mask = mouseButton switch
            {
                MouseButton.MB1 => SDL_BUTTON_LMASK,
                MouseButton.MB2 => SDL_BUTTON_RMASK,
                MouseButton.MB3 => SDL_BUTTON_MMASK,
                MouseButton.MB4 => SDL_BUTTON_X1MASK,
                MouseButton.MB5 => SDL_BUTTON_X2MASK,
                _ => SDL_BUTTON_LMASK
            };
            return (mask & state) != 0;
        }
    }
}
