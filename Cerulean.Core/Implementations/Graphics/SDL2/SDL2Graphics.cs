
using Cerulean.Common;
using System.Runtime.InteropServices;
using static SDL2.SDL;
using static SDL2.SDL_image;
using static SDL2.SDL_ttf;

namespace Cerulean.Core
{
    internal class SDL2Graphics : IGraphics
    {
        private readonly Window _window;
        private readonly TextureCache _textureCache;
        private readonly FontCache _fontCache;

        // global TTF Init
        private static bool _initializedSubmodules = false;
        private int _globalX = 0;
        private int _globalY = 0;
        internal IntPtr WindowPtr => _window.WindowPtr;
        internal IntPtr RendererPtr { get; }

        public SDL2Graphics(Window window)
        {
            if (!_initializedSubmodules)
            {
                _ = TTF_Init();
                _ = IMG_Init(IMG_InitFlags.IMG_INIT_JPG | IMG_InitFlags.IMG_INIT_PNG | IMG_InitFlags.IMG_INIT_TIF);
                _initializedSubmodules = true;
            }
            _window = window;
            var renderer = SDL_CreateRenderer(WindowPtr, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (renderer == IntPtr.Zero)
            {
                throw new FatalAPIException("Could not create SDL2 renderer.");
            }
            _ = SDL_SetRenderDrawBlendMode(renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);
            RendererPtr = renderer;
            _textureCache = new TextureCache(64);
            _fontCache = new FontCache();
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
        private bool EnsureSurfaceSize(ref IntPtr surfacePtr)
        {
            if (surfacePtr == IntPtr.Zero)
            {
                CeruleanAPI.GetAPI().Log("Surface is null.", LogSeverity.Warning);
                return true;
            }
            SDL_Surface surface = Marshal.PtrToStructure<SDL_Surface>(surfacePtr);
            _ = SDL_GetRendererInfo(RendererPtr, out SDL_RendererInfo rendererInfo);
            if (surface.w <= 0 || surface.h <= 0)
                return true;

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
            return false;
        }
        #endregion
        #region GENERAL
        public Size GetRenderArea(out int x, out int y)
        {
            //_ = SDL_RenderGetViewport(RendererPtr, out var rect);
            SDL_RenderGetClipRect(RendererPtr, out var rect);
            x = rect.x;
            y = rect.y;
            _globalX = x;
            _globalY = y;
            return new Size(rect.w, rect.h);
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
            _globalX = x;
            _globalY = y;
            if (SDL_RenderSetClipRect(RendererPtr, ref rect) != 0)
                throw new GeneralAPIException($"Could not set viewport data. {SDL_GetError()}");
            //if (SDL_RenderSetViewport(RendererPtr, ref rect) != 0)
            //    throw new GeneralAPIException($"Could not set viewport data. {SDL_GetError()}");
        }
        #endregion
        #region RENDER
        public void RenderClear()
        {
            SDL_RenderClear(RendererPtr);
        }

        public void RenderPresent()
        {
            SDL_RenderPresent(RendererPtr);
        }
        #endregion
        #region PRIMITIVES
        public void DrawRectangle(int x, int y, Size size)
        {
            SDL_Rect rect = new()
            {
                x = x + _globalX,
                y = y + _globalY,
                w = size.W,
                h = size.H
            };
            _ = SDL_RenderDrawRect(RendererPtr, ref rect);
        }
        public void DrawRectangle(int x, int y, Size size, Color color)
        {
            _ = SDL_GetRenderDrawColor(
                RendererPtr,
                out var r,
                out var g,
                out var b,
                out var a);
            _ = SDL_SetRenderDrawColor(
                RendererPtr,
                color.R,
                color.G,
                color.B,
                color.A);
            DrawRectangle(x, y, size);
            _ = SDL_SetRenderDrawColor(
                RendererPtr,
                r,
                g,
                b,
                a);
        }
        public void DrawFilledRectangle(int x, int y, Size size)
        {
            SDL_Rect rect = new()
            {
                x = x + _globalX,
                y = y + _globalY,
                w = size.W,
                h = size.H
            };
            _ = SDL_RenderFillRect(RendererPtr, ref rect);
        }

        public void DrawFilledRectangle(int x, int y, Size size, Color color)
        {
            _ = SDL_GetRenderDrawColor(
                RendererPtr,
                out var r,
                out var g,
                out var b,
                out var a);
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
        #region TEXTURE
        public void DrawImage(int x, int y, Size size, string fileName, PictureMode pictureMode, double opacity)
        {
            string fingerprint = $"{fileName}_{pictureMode.ToString()}_{opacity}";
            Texture? texture;
            if (!_textureCache.TryGetTexture(fingerprint, out texture))
            {
                IntPtr sdlSurface = IMG_Load(fileName);
                if (sdlSurface == IntPtr.Zero)
                {
                    string error = IMG_GetError();
                    CeruleanAPI.GetAPI().Log($"Could not load image file via SDL_image. Reason: {error}");
                    throw new GeneralAPIException(error);
                }
                IntPtr sdlTexture = SDL_CreateTextureFromSurface(RendererPtr, sdlSurface);
                if (sdlTexture == IntPtr.Zero)
                {
                    string error = SDL_GetError();
                    CeruleanAPI.GetAPI().Log($"Could not create texture from surface. Reason: {error}");
                    throw new GeneralAPIException(error);
                }
                SDL_GetClipRect(sdlSurface, out SDL_Rect rect);
                Size surfaceSize = new(rect.w, rect.h);
                texture = new Texture()
                {
                    Identity = fingerprint,
                    SDLTexture = sdlTexture,
                    UserData = surfaceSize,
                    Type = TextureType.Texture,
                    Score = 5
                };
                SDL_FreeSurface(sdlSurface);
                _textureCache.AddTexture(texture.Value);
            }
            if (texture is not null && texture.Value.UserData is Size imageSize)
            {
                // draw texture
                var sdlTexture = texture.Value.SDLTexture;
                texture.Value.AccScore(10000);
                texture.Value.SetScore(Min((long)texture.Value.Score, 100000000000));
                int imageX = x, imageY = y;
                int w = imageSize.W, h = imageSize.H;
                if (pictureMode != PictureMode.Tile)
                {
                    switch (pictureMode)
                    {
                        case PictureMode.Stretch:
                            w = size.W;
                            h = size.H;
                            break;
                        case PictureMode.Center:
                            int w2 = (size.W - w) / 2;
                            int h2 = (size.H - h) / 2;
                            imageX = x + w2;
                            imageY = y + h2;
                            break;
                        case PictureMode.Cover:
                            int diffW = size.W - imageSize.W;
                            int diffH = size.H - imageSize.H;
                            if (diffW > diffH)
                            {
                                double scale = (double)size.W / imageSize.W;
                                w = size.W;
                                h = (int)(imageSize.H * scale);
                            } else 
                            {
                                double scale = (double)size.H / imageSize.H;
                                h = size.H;
                                w = (int)(imageSize.W * scale);
                            }
                            imageX = x + (size.W - w) / 2;
                            imageY = y + (size.H - h) / 2;
                            break;
                        case PictureMode.Fit:
                            double factor = Math.Min((double)size.W / imageSize.W, (double)size.H / imageSize.H);
                            w = (int)(imageSize.W * factor);
                            h = (int)(imageSize.H * factor);
                            imageX = x + (size.W - w) / 2;
                            imageY = y + (size.H - h) / 2;
                            break;
                    }
                } else
                {
                    int xRep = (int)Math.Ceiling((double)size.W / imageSize.W);
                    int yRep = (int)Math.Ceiling((double)size.H / imageSize.H);

                    for (int i = 0; i < xRep; i++)
                    {
                        for (int j = 0; j < yRep; j++)
                        {
                            DrawImage(imageX + i * imageSize.W, imageY + j * imageSize.H, imageSize, fileName, PictureMode.None, opacity);
                        }
                    }
                    return;
                }
                SDL_Rect target = new()
                {
                    x = imageX,
                    y = imageY,
                    w = w,
                    h = h
                };
                
                SDL_RenderCopy(RendererPtr, sdlTexture, IntPtr.Zero, ref target);
            } else 
            {
                throw new GeneralAPIException("Could not load texture.");
            }
        }
        #endregion
        #region TEXT
        public void DrawText(int x, int y, string text, string fontName, string fontStyle, int fontPointSize, Color color, uint textWrap, double angle)
        {
            CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("DrawText");
            _ = SDL_GetRenderDrawColor(
                RendererPtr,
                out var r,
                out var g,
                out var b,
                out var a);
            _ = SDL_SetRenderDrawColor(
                RendererPtr,
                255,
                255,
                255,
                255);

            // define font and text to check in caches
            var fontIdentity = FontCache.GetID(fontName, fontStyle, fontPointSize);
            var textFingerprint = $"{fontIdentity}@{color} \"{text}\" {textWrap}px {angle}deg";

            // check if specific text + font + color combo is NOT in texture cache
            if (!_textureCache.TryGetTexture(textFingerprint, out var textTexture))
            {
                CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("MakeTexture");
                // fetch/make font
                var font = _fontCache.GetFont(fontName, fontStyle, fontPointSize);
                CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("TTF_Render");
                var surface = textWrap < 1 ?
                    TTF_RenderText_Blended(font.Data, text, new SDL_Color
                    {
                        r = color.R,
                        g = color.G,
                        b = color.B,
                        a = color.A
                    }) :
                    TTF_RenderText_Blended_Wrapped(font.Data, text, new SDL_Color
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
                if (EnsureSurfaceSize(ref surface))
                {
                    // size error
                    SDL_FreeSurface(surface);
                    return;
                }
                CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();
                CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("ConvertToTexture");
                IntPtr sdlTexture = SDL_CreateTextureFromSurface(RendererPtr, surface);
                if (sdlTexture == IntPtr.Zero)
                    throw new GeneralAPIException(SDL_GetError());
                SDL_GetClipRect(surface, out var destRect);
                destRect.x = x + _globalX;
                destRect.y = y + _globalY;
                textTexture = new Texture
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
                    var rawTexture = textTexture.Value;
                    rawTexture.AccScore(10000);
                    rawTexture.SetScore(Min((long)rawTexture.Score, 100000000000));
                    SDL_Point center = new();
                    var texture = textTexture.Value.SDLTexture;
                    if (SDL_RenderCopyEx(RendererPtr, texture, IntPtr.Zero, ref rect, angle, ref center, SDL_RendererFlip.SDL_FLIP_NONE) != 0)
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
