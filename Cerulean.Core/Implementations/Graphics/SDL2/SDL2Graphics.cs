using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cerulean.Common;
using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace Cerulean.Core
{
    internal class SDL2Graphics : IGraphics
    {
        private readonly Window _window;
        private readonly IntPtr _renderer;
        private readonly TextureCache _textureCache;
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
            _textureCache = new();
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

        public void DrawRectangle(int x, int y, Size size)
        {
            SDL_Rect rect = new()
            {
                x = x,
                y = y,
                w = size.W,
                h = size.H
            };
            SDL_SetRenderDrawColor(RendererPtr, 200, 10, 10, 255);
            SDL_RenderFillRect(RendererPtr, ref rect);
            SDL_SetRenderDrawColor(RendererPtr, 0, 0, 0, 255);
        }

        public void Update()
        {
            // TODO: keep texture cache to a specific size by removing old textures
        }
    }
}
