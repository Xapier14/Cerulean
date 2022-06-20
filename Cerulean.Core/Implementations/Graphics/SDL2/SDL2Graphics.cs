
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
        private readonly FontCache _fontCache;

        // global TTF Init
        private static bool _initializedTTF = false;
        internal IntPtr WindowPtr => _window.WindowPtr;
        internal IntPtr RendererPtr => _renderer;
        public SDL2Graphics(Window window)
        {
            if (!_initializedTTF)
            {
                _ = TTF_Init();
                _initializedTTF = true;
            }
            _window = window;
            var renderer = SDL_CreateRenderer(WindowPtr, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (renderer == IntPtr.Zero)
            {
                throw new FatalAPIException("Could not create SDL2 renderer.");
            }
            SDL_SetRenderDrawBlendMode(renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);
            _renderer = renderer;
            _textureCache = new();
            _fontCache = new();
        }

        public void Update()
        {
            // TODO: keep texture cache to a specific size by removing old textures
        }

        public void Cleanup()
        {

        }

        #region GENERAL
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
        #endregion
        #region RENDER
        public void RenderClear()
        {
            SDL_RenderClear(_renderer);
        }

        public void RenderPresent()
        {
            SDL_RenderPresent(RendererPtr);
        }
        #endregion
        #region PRIMITIVES
        public void DrawFilledRectangle(int x, int y, Size size)
        {
            SDL_Rect rect = new()
            {
                x = x,
                y = y,
                w = size.W,
                h = size.H
            };
            _ = SDL_RenderFillRect(RendererPtr, ref rect);
        }

        public void DrawFilledRectangle(int x, int y, Size size, Color color)
        {
            _ = SDL_GetRenderDrawColor(
                RendererPtr,
                out byte r,
                out byte g,
                out byte b,
                out byte a);
            _ = SDL_SetRenderDrawColor(
                RendererPtr,
                color.R,
                color.G,
                color.B,
                color.A);
            DrawFilledRectangle(x, y, size);
            _ = SDL_SetRenderDrawColor(
                RendererPtr,
                r,
                g,
                b,
                a);
        }
        #endregion
        #region TEXT
        public void DrawText(int x, int y, string text, string fontName, int fontPointSize, Color color, uint textWrap, double angle)
        {
            _ = SDL_GetRenderDrawColor(
                RendererPtr,
                out byte r,
                out byte g,
                out byte b,
                out byte a);
            _ = SDL_SetRenderDrawColor(
                RendererPtr,
                255,
                255,
                255,
                255);

            // define font and text to check in caches
            string fontIdentity = FontCache.GetID(fontName, fontPointSize);
            string textFingerprint = $"{fontIdentity}@{color} \"{text}\"";
            Texture? textTexture;

            // check if specific text + font + color combo is NOT in texture cache
            if (!_textureCache.TryGetTexture(textFingerprint, out textTexture))
            {
                // fetch/make font
                var font = _fontCache.GetFont(fontName, fontPointSize);
                IntPtr surface = textWrap < 1 ?
                    TTF_RenderText_Blended(font.Data, text, new()
                    {
                        r = color.R,
                        g = color.G,
                        b = color.B,
                        a = color.A
                    }) :
                    TTF_RenderText_Blended_Wrapped(font.Data, text, new()
                    {
                        r = color.R,
                        g = color.G,
                        b = color.B,
                        a = color.A
                    }, textWrap);
                IntPtr sdlTexture = SDL_CreateTextureFromSurface(_renderer, surface);
                if (sdlTexture == IntPtr.Zero)
                    throw new GeneralAPIException("Could not create texture for text surface.");
                SDL_GetClipRect(surface, out SDL_Rect destRect);
                destRect.x = x;
                destRect.y = y;
                textTexture = new()
                {
                    Identity = textFingerprint,
                    Type = TextureType.Text,
                    SDLTexture = sdlTexture,
                    UserData = destRect
                };
                _textureCache.AddTexture(textTexture.Value);

                SDL_FreeSurface(surface);
            }

            // draw texture
            if (textTexture is not null)
            {
                if (textTexture.Value.UserData is SDL_Rect rect)
                {
                    SDL_Point center = new();
                    IntPtr texture = textTexture.Value.SDLTexture;
                    if (SDL_RenderCopyEx(_renderer, texture, IntPtr.Zero, ref rect, angle, ref center, SDL_RendererFlip.SDL_FLIP_NONE) != 0)
                        CeruleanAPI.GetAPI().Log("Error in RenderCopyEx() text!", LogSeverity.Error);
                }
                else
                {
                    CeruleanAPI.GetAPI().Log("Destination rect for text is null.", LogSeverity.Warning);
                }
            }

            _ = SDL_SetRenderDrawColor(
                RendererPtr,
                r,
                g,
                b,
                a);
        }
        #endregion
    }
}
