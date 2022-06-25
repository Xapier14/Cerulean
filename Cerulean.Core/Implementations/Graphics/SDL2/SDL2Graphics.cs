
using Cerulean.Common;
using System.Runtime.InteropServices;
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
            _textureCache = new(64);
            _fontCache = new();
        }

        public void Update()
        {
            // TODO: keep texture cache to a specific size by removing old textures
            //var ceruleanApi = CeruleanAPI.GetAPI();
            //ceruleanApi.Log($"Texture Cache: {_textureCache.Count()}; Font Cache: {_fontCache.Count()}");
            _textureCache.DevalueTextures();
        }

        public void Cleanup()
        {
            _fontCache.Clear();
            _textureCache.Clear();
        }
        #region AUXILLIARY
        private static dynamic Min(dynamic i1, dynamic i2)
            => i1 < i2 ? i1 : i2;
        private void EnsureSurfaceSize(ref IntPtr surfacePtr)
        {
            if (surfacePtr == IntPtr.Zero)
            {
                CeruleanAPI.GetAPI().Log("Surface is null.", LogSeverity.Warning);
                return;
            }
            SDL_Surface surface = Marshal.PtrToStructure<SDL_Surface>(surfacePtr);
            _ = SDL_GetRendererInfo(_renderer, out SDL_RendererInfo rendererInfo);

            if (surface.w > rendererInfo.max_texture_width ||
                surface.h > rendererInfo.max_texture_height)
            {
                // resize surface
                SDL_PixelFormat pixelFormat = Marshal.PtrToStructure<SDL_PixelFormat>(surface.format);
                SDL_Rect rect = new()
                {
                    x = 0,
                    y = 0,
                    w = Min(surface.w, rendererInfo.max_texture_width),
                    h = Min(surface.h, rendererInfo.max_texture_height)
                };
                IntPtr newPtr = SDL_CreateRGBSurface(
                    0,
                    rect.w,
                    rect.h,
                    32,
                    0xFF000000,
                    0x00FF0000,
                    0x0000FF00,
                    0x000000FF);
                SDL_BlitSurface(surfacePtr, ref rect, newPtr, ref rect);
                SDL_FreeSurface(surfacePtr);
                surfacePtr = newPtr;
            }
        }
        #endregion
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
        public void DrawText(int x, int y, string text, string fontName, string fontStyle, int fontPointSize, Color color, uint textWrap, double angle)
        {
            CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("DrawText");
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
            string fontIdentity = FontCache.GetID(fontName, fontStyle, fontPointSize);
            string textFingerprint = $"{fontIdentity}@{color} \"{text}\" {textWrap}px {angle}deg";
            Texture? textTexture;

            // check if specific text + font + color combo is NOT in texture cache
            if (!_textureCache.TryGetTexture(textFingerprint, out textTexture))
            {
                CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("MakeTexture");
                // fetch/make font
                var font = _fontCache.GetFont(fontName, fontStyle, fontPointSize);
                CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("TTF_Render");
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
                if (surface == IntPtr.Zero)
                {
                    CeruleanAPI.GetAPI().Log($"Could not render text: {TTF_GetError()}", LogSeverity.Error);
                    return;
                }
                CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();
                CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("EnsureSize");
                EnsureSurfaceSize(ref surface);
                CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();
                CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("ConverttoTexture");
                IntPtr sdlTexture = SDL_CreateTextureFromSurface(_renderer, surface);
                if (sdlTexture == IntPtr.Zero)
                    throw new GeneralAPIException(SDL_GetError());
                SDL_GetClipRect(surface, out SDL_Rect destRect);
                destRect.x = x;
                destRect.y = y;
                textTexture = new()
                {
                    Identity = textFingerprint,
                    Type = TextureType.Text,
                    SDLTexture = sdlTexture,
                    UserData = destRect,
                    Score = 5
                };
                _textureCache.AddTexture(textTexture.Value);
                CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();

                SDL_FreeSurface(surface);
                CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();
            }
            CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("DrawTexture");
            // draw texture
            if (textTexture is not null)
            {
                if (textTexture.Value.UserData is SDL_Rect rect)
                {
                    Texture rawTexture = textTexture.Value;
                    rawTexture.AccScore(10000);
                    rawTexture.SetScore(Min((long)rawTexture.Score, 100000000000));
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
            CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();

            _ = SDL_SetRenderDrawColor(
                RendererPtr,
                r,
                g,
                b,
                a);
            CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();
        }
        #endregion
    }
}
