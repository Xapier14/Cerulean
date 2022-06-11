using System.Collections.Concurrent;
using Cerulean.Common;
using static SDL2.SDL;

namespace Cerulean.Core
{
    public class WindowEventArgs : EventArgs
    {
        public int WindowWidth { get; init; }
        public int WindowHeight { get; init; }
        public int WindowX { get ; init; }
        public int WindowY { get; init; }
        public bool Cancel { get; set; }
    }
    public sealed class Window
    {
        public static readonly Size DefaultWindowSize = new(600, 400);

        private int _threadId;
        public bool Closed { get; private set; }

        private IntPtr _window;
        internal IntPtr WindowPtr { get => _window; }
        private IntPtr _renderer;
        internal IntPtr RendererPtr { get => _renderer; }
        internal bool CloseFromEvent = false;

        private bool _initialized = false;
        public bool IsInitialized { get => _initialized; }

        private string _windowTitle = "";

        public EventHandler<WindowEventArgs>? OnResize, OnMininize, OnRestore, OnMaximize, OnMoved, OnClose, OnMouseLeave, OnMouseEnter, OnFocusGained, OnFocusLost;

        public string WindowTitle
        {
            get
            {
                return _windowTitle;
            }
            set
            {
                if (Closed)
                    throw new GeneralAPIException("Cannot set title to a closed window.");
                if (!_initialized)
                {
                    _windowTitle = value;
                }
                else
                {
                    EnsureMainThread("Changing window title must be done on the thread that created the window.");
                    // change window title via SDL call
                    SDL_SetWindowTitle(_window, value);
                    _windowTitle = value;
                }
            }
        }

        private Size _windowSize;

        public Size WindowSize
        {
            get
            {
                return _windowSize;
            }
            set
            {
                if (Closed)
                    throw new GeneralAPIException("Cannot set size to a closed window.");
                if (!_initialized)
                {
                    _windowSize = value;
                }
                else
                {
                    EnsureMainThread("Changing window size must be done on the thread that created the window.");
                    // change window size via SDL call
                    SDL_SetWindowSize(_window, value.W, value.H);
                    SDL_RenderSetLogicalSize(_renderer, value.W, value.H);
                    _windowSize = value;
                }
            }
        }

        public bool Enabled { get; set; } = true;
        public bool HandleClose { get; set; } = true;
        public Window? ParentWindow { get; init; }

        public dynamic Layout { get; private set; }

        public void Close()
        {
            if (!Closed)
                CeruleanAPI.GetAPI().CloseWindow(this);
        }

        internal Window(Layout windowLayout, string windowTitle, Size windowSize, int threadId, Window? parentWindow = null)
        {
            _initialized = false;
            _threadId = threadId;
            WindowTitle = windowTitle;
            WindowSize = windowSize;
            Layout = windowLayout;
            ParentWindow = null;
        }

        internal void InternalClose()
        {
            EnsureMainThread();
            SDL_DestroyRenderer(_renderer);
            SDL_DestroyWindow(_window);
            Closed = true;
        }

        private void EnsureMainThread(string? message = null)
        {
            if (_threadId != Environment.CurrentManagedThreadId)
                throw message == null ?
                    new ThreadSafetyException() :
                    new ThreadSafetyException(message);
        }

        internal void Initialize()
        {
            EnsureMainThread("Window initialization must be called from the thread that initialized the CeruleanAPI instance.");
            _initialized = true;
            if (SDL_CreateWindowAndRenderer(WindowSize.W, WindowSize.H, SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI, out _window, out _renderer) != 0)
            {
                throw new FatalAPIException("Failed to create window and renderer.");
            }
            SDL_SetWindowTitle(_window, WindowTitle);
            SDL_RenderGetLogicalSize(_renderer, out int w, out int h);
            Layout.Init();
        }

        internal void Draw()
        {
            SDL_RenderClear(_renderer);



            SDL_RenderPresent(_renderer);
        }

        internal void InvokeOnResize(int w, int h)
        {
            SDL_RenderSetLogicalSize(_renderer, w, h);
            _windowSize = new(w, h);
            OnResize?.Invoke(this, new()
            {
                WindowWidth = w,
                WindowHeight = h
            });
        }

        internal void InvokeOnClose()
        {
            bool cancel = false;
            if (OnClose?.GetInvocationList().Length > 0)
            {
                WindowEventArgs eventArgs = new();
                OnClose?.Invoke(this, eventArgs);
                cancel = eventArgs.Cancel;
            }
            if (HandleClose && !cancel)
            {
                CeruleanAPI.GetAPI().CloseWindow(this);
            }
            CloseFromEvent = false;
        }

        internal void InvokeOnMinimize()
        {
            OnMininize?.Invoke(this, new());
        }

        internal void InvokeOnRestore()
        {
            OnRestore?.Invoke(this, new());
        }

        internal void InvokeOnMaximize()
        {
            OnMaximize?.Invoke(this, new());
        }

        internal void InvokeOnMoved(int x, int y)
        {
            OnMoved?.Invoke(this, new()
            {
                WindowX = x,
                WindowY = y
            });
        }

        internal void InvokeOnFocusGained()
        {
            OnFocusGained?.Invoke(this, new());
        }

        internal void InvokeOnMouseLeave()
        {
            OnMouseLeave?.Invoke(this, new());
        }

        internal void InvokeOnMouseEnter()
        {
            OnMouseEnter?.Invoke(this, new());
        }

        internal void InvokeOnFocusLost()
        {
            OnFocusLost?.Invoke(this, new());
        }
    }
}