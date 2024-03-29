﻿
using Cerulean.Common;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
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
        private readonly SHA256 _hash;

        // global TTF Init
        private static bool _initializedSubmodules = false;
        private int _globalX;
        private int _globalY;

        public long ActiveTextureAllocations { private set; get; }

        private long _createdPointers = 0;
        private IntPtr WindowPtr => _window.WindowPtr;
        private IntPtr RendererPtr { get; }

        private List<IntPtr> _activeAllocations = new();

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
            _hash = SHA256.Create();
        }

        public void Update()
        {
            _textureCache.DevalueTextures();

            ActiveTextureAllocations = _textureCache.ActiveAllocatedPtrs.Count;
        }

        public void Cleanup()
        {
            _fontCache.Clear();
            _textureCache.Clear();
            _hash.Dispose();
        }
        #region AUXILLIARY
        private static dynamic Min(dynamic i1, dynamic i2)
            => i1 < i2 ? i1 : i2;

        private static (GCHandle, IntPtr) SDL_RWfromBytes(byte[] bytes)
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var pointer = handle.AddrOfPinnedObject();
            var rwops = SDL_RWFromMem(pointer, bytes.Length);
            return (handle, rwops);
        }

        private string HashBytes(byte[] bytes)
        {
            var hash = _hash.ComputeHash(bytes);

            var stringBuilder = new StringBuilder();
            foreach (var b in hash)
            {
                stringBuilder.Append(b.ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        private string HashString(string value)
        {
            var hash = _hash.ComputeHash(Encoding.UTF8.GetBytes(value));

            var stringBuilder = new StringBuilder();
            foreach (var b in hash)
            {
                stringBuilder.Append(b.ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        private bool EnsureSurfaceSize(ref IntPtr surfacePtr)
        {
            if (surfacePtr == IntPtr.Zero)
            {
                CeruleanAPI.GetAPI().Log("Surface is null.", LogSeverity.Warning);
                return true;
            }
            var surface = Marshal.PtrToStructure<SDL_Surface>(surfacePtr);
            _ = SDL_GetRendererInfo(RendererPtr, out var rendererInfo);

            if (surface.w <= 0 || surface.h <= 0)
                return true;

            if (rendererInfo.max_texture_height == 0 ||
                rendererInfo.max_texture_width == 0)
                return false;

            if (surface.w <= rendererInfo.max_texture_width &&
                surface.h <= rendererInfo.max_texture_height)
                return false;

            // resize surface
            var pixelFormat = Marshal.PtrToStructure<SDL_PixelFormat>(surface.format);
            SDL_Rect rect = new()
            {
                x = 0,
                y = 0,
                w = Min(surface.w, rendererInfo.max_texture_width),
                h = Min(surface.h, rendererInfo.max_texture_height)
            };
            var newPtr = SDL_CreateRGBSurface(
                0,
                rect.w,
                rect.h,
                pixelFormat.BitsPerPixel,
                pixelFormat.Rmask,
                pixelFormat.Gmask,
                pixelFormat.Bmask,
                pixelFormat.Amask);
            SDL_BlitSurface(surfacePtr, ref rect, newPtr, ref rect);
            SDL_FreeSurface(surfacePtr);
            surfacePtr = newPtr;
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
            //Console.WriteLine("RenderArea set to: ({0}, {1}) {2}; offset: {3}, {4}", x, y, renderArea, _offsetX, _offsetY);
            if (SDL_RenderSetClipRect(RendererPtr, ref rect) != 0)
                throw new GeneralAPIException($"Could not set viewport data. {SDL_GetError()}");
            //if (SDL_RenderSetViewport(RendererPtr, ref rect) != 0)
            //    throw new GeneralAPIException($"Could not set viewport data. {SDL_GetError()}");
        }
        public void GetGlobalPosition(out int x, out int y)
        {
            x = _globalX;
            y = _globalY;
        }
        public void SetGlobalPosition(int x, int y)
        {
            _globalX = x;
            _globalY = y;
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
        public void DrawPixel(int x, int y, Color color)
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
            _ = SDL_RenderDrawPoint(RendererPtr, x, y);
            _ = SDL_SetRenderDrawColor(
                RendererPtr,
                r,
                g,
                b,
                a);
        }
        public void DrawLine(int x1, int y1, int x2, int y2)
        {
            _ = SDL_RenderDrawLine(RendererPtr, x1, y1, x2, y2);
        }
        public void DrawLine(int x1, int y1, int x2, int y2, Color color)
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
            DrawLine(x1, y1, x2, y2);
            _ = SDL_SetRenderDrawColor(
                RendererPtr,
                r,
                g,
                b,
                a);
        }
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
            var bytes = File.ReadAllBytes(fileName);
            DrawImageFromBytes(x, y, size, bytes, pictureMode, opacity);
        }
        public void DrawImageFromBytes(int x, int y, Size size, byte[] bytes, PictureMode pictureMode, double opacity)
        {
            var hash = HashBytes(bytes);
            var fingerprint = HashString($"{hash}_{pictureMode.ToString()}_{opacity}");
            Texture? texture;
            if (!_textureCache.TryGetTexture(fingerprint, out texture))
            {
                var (handle, rwops) = SDL_RWfromBytes(bytes);
                var sdlSurface = IntPtr.Zero;
                if (IMG_isSVG(rwops) != 0)
                {
                    sdlSurface = pictureMode is PictureMode.None or PictureMode.Tile
                        ? IMG_LoadSVG_RW(rwops)
                        : IMG_LoadSizedSVG_RW(rwops, size.W, size.H);
                }
                else
                {
                    sdlSurface = IMG_Load_RW(rwops, 0);
                }
                if (sdlSurface == IntPtr.Zero)
                {
                    var error = IMG_GetError();
                    CeruleanAPI.GetAPI().Log($"Could not load image file via SDL_image. Reason: {error}");
                    throw new GeneralAPIException(error);
                }

                _createdPointers++;
                var sdlTexture = SDL_CreateTextureFromSurface(RendererPtr, sdlSurface);
                _textureCache.ActiveAllocatedPtrs.Add(sdlTexture);
                if (sdlTexture == IntPtr.Zero)
                {
                    var error = SDL_GetError();
                    CeruleanAPI.GetAPI().Log($"Could not create texture from surface. Reason: {error}");
                    throw new GeneralAPIException(error);
                }
                SDL_GetClipRect(sdlSurface, out var rect);
                Size surfaceSize = new(rect.w, rect.h);
                texture = new Texture()
                {
                    Identity = fingerprint,
                    SDLTexture = sdlTexture,
                    UserData = surfaceSize,
                    Type = TextureType.Texture,
                    Score = 10000
                };
                SDL_FreeSurface(sdlSurface);
                _createdPointers--;
                SDL_RWclose(rwops);
                handle.Free();
                _textureCache.AddTexture(texture.Value);
            }
            if (texture?.UserData is Size imageSize)
            {
                // draw texture
                var sdlTexture = texture.Value.SDLTexture;
                texture.Value.AccScore(10000);
                texture.Value.SetScore(Min((long)texture.Value.Score, 100000000000));
                int imageX = x + _globalX, imageY = y + _globalY;
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
                            var w2 = (size.W - w) / 2;
                            var h2 = (size.H - h) / 2;
                            imageX = x + _globalX + w2;
                            imageY = y + _globalY + h2;
                            break;
                        case PictureMode.Cover:
                            var diffW = size.W - imageSize.W;
                            var diffH = size.H - imageSize.H;
                            if (diffW > diffH)
                            {
                                var scale = (double)size.W / imageSize.W;
                                w = size.W;
                                h = (int)(imageSize.H * scale);
                            }
                            else
                            {
                                var scale = (double)size.H / imageSize.H;
                                h = size.H;
                                w = (int)(imageSize.W * scale);
                            }
                            imageX = x + _globalX + (size.W - w) / 2;
                            imageY = y + _globalY + (size.H - h) / 2;
                            break;
                        case PictureMode.Fit:
                            var factor = Math.Min((double)size.W / imageSize.W, (double)size.H / imageSize.H);
                            w = (int)(imageSize.W * factor);
                            h = (int)(imageSize.H * factor);
                            imageX = x + _globalX + (size.W - w) / 2;
                            imageY = y + _globalY + (size.H - h) / 2;
                            break;
                    }
                }
                else
                {
                    var xRep = (int)Math.Ceiling((double)size.W / imageSize.W);
                    var yRep = (int)Math.Ceiling((double)size.H / imageSize.H);
                    imageX = x;
                    imageY = y;

                    for (var i = 0; i < xRep; i++)
                    {
                        for (var j = 0; j < yRep; j++)
                        {
                            DrawImageFromBytes(imageX + i * imageSize.W, imageY + j * imageSize.H, imageSize, bytes, PictureMode.None, opacity);
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
            }
            else
            {
                throw new GeneralAPIException("Could not load texture.");
            }
        }
        #endregion
        #region TEXT
        public void DrawText(int x, int y, string text, string fontName, string fontStyle, int fontPointSize, Color color, uint textWrap = 0, double angle = 0, string seedId = "")
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
            var textFingerprint = HashString($"{fontIdentity}@{color} \"{text}\" {textWrap}px {angle}deg {seedId}");

            // check if specific text + font + color combo is NOT in texture cache
            if (!_textureCache.TryGetTexture(textFingerprint, out var textTexture))
            {
                CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("MakeTexture");
                // fetch/make font
                var font = _fontCache.GetFont(fontName, fontStyle, fontPointSize);
                CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("TTF_Render");
                var surface = textWrap < 1 ?
                    TTF_RenderUNICODE_Blended(font.Data, text, new SDL_Color
                    {
                        r = color.R,
                        g = color.G,
                        b = color.B,
                        a = color.A
                    }) :
                    TTF_RenderUNICODE_Blended_Wrapped(font.Data, text, new SDL_Color
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

                _createdPointers++;
                CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();
                CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("EnsureSize");
                if (EnsureSurfaceSize(ref surface))
                {
                    // size error
                    SDL_FreeSurface(surface);
                    _createdPointers--;
                    return;
                }

                CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();
                CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("ConvertToTexture");
                var sdlTexture = SDL_CreateTextureFromSurface(RendererPtr, surface);
                _textureCache.ActiveAllocatedPtrs.Add(sdlTexture);
                if (sdlTexture == IntPtr.Zero)
                    throw new GeneralAPIException(SDL_GetError());
                SDL_GetClipRect(surface, out var destRect);
                SDL_FreeSurface(surface);
                _createdPointers--;
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
                    rawTexture.SetScore(Min(rawTexture.Score, 100000000000));
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

        public (int, int) MeasureText(string text, string fontName, string fontStyle, int fontPointSize,
            int textWrap = 0)
        {
            var font = _fontCache.GetFont(fontName, fontStyle, fontPointSize);
            TTF_SizeUTF8(font.Data, text, out var w, out var h);
            if (textWrap == 0 || textWrap >= w)
                return (w, h);
            var totalRows = (w / textWrap);
            return (textWrap, totalRows * h);
        }
        #endregion
        #region UTILITIES

        public float GetCurrentDisplayDpi()
        {
            var displayIndex = SDL_GetWindowDisplayIndex(WindowPtr);
            if (SDL_GetDisplayDPI(displayIndex, out var ddpi, out _, out _) != 0)
                return 0;
            return ddpi;
        }

        #endregion
    }
}
