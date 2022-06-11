using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cerulean.Common;
using static SDL2.SDL;

namespace Cerulean.Core
{
    internal class SDL2Graphics : IGraphics
    {
        private readonly Window _window;
        private readonly IntPtr _renderer;
        internal IntPtr WindowPtr => _window.WindowPtr;
        internal IntPtr RendererPtr => _renderer;
        public SDL2Graphics(Window window)
        {
            _window = window;
            var renderer = SDL_CreateRenderer(WindowPtr, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (renderer == IntPtr.Zero)
            {
                throw new FatalAPIException("Could not create SDL2 renderer.");
            }
            _renderer = renderer;
        }

        public Size GetRenderArea(out int x, out int y)
        {
            _ = SDL_RenderGetViewport(RendererPtr, out SDL_Rect rect);
            x = rect.x;
            y = rect.y;
            return new(rect.w, rect.h);
        }
        public void SetRenderArea(Size renderArea, int x, int y)
        {
            SDL_Rect rect = new()
            {
                w = renderArea.W,
                h = renderArea.H,
                x = x,
                y = y
            };
            if (SDL_RenderSetViewport(RendererPtr, ref rect) != 0)
                throw new GeneralAPIException("Could not set viewport data.");
        }

        public void RenderClear()
        {
            if (SDL_RenderClear(RendererPtr) != 0)
                throw new GeneralAPIException("Could not clear renderer.");
        }

        public void RenderPresent()
        {
            SDL_RenderPresent(RendererPtr);
        }
    }
}
