using Cerulean.Common;
using static SDL2.SDL;

namespace Cerulean.Core
{
    public class WindowEventArgs : EventArgs
    {
        public int WindowWidth { get; init; }
        public int WindowHeight { get; init; }
        public int WindowX { get; init; }
        public int WindowY { get; init; }
        public bool Cancel { get; set; }
    }
    public sealed class Window
    {
        #region Private+Internals
        private readonly IGraphicsFactory _graphicsFactory;
        private int _threadId;
        private IntPtr _window;
        internal IntPtr WindowPtr { get => _window; }
        internal bool _closeFromEvent = false;
        private bool _initialized = false;
        private string _windowTitle = "";
        #endregion

        public static readonly Size DefaultWindowSize = new(600, 400);
        public delegate void WindowEventHandler(Window sender, WindowEventArgs e);
        public WindowEventHandler? OnResize, OnMininize, OnRestore, OnMaximize, OnMoved, OnClose, OnMouseLeave, OnMouseEnter, OnFocusGained, OnFocusLost;
        public bool IsInitialized { get => _initialized; }
        public bool Closed { get; private set; }
        public IGraphics? GraphicsContext { get; private set; } = null;

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
                    GraphicsContext?.SetRenderArea(value, 0, 0);
                    _windowSize = value;
                }
            }
        }

        private (int, int) _windowPosition;

        public (int, int) WindowPosition
        {
            get
            {
                return (_windowPosition.Item1, _windowPosition.Item2);
            }
            set
            {
                EnsureMainThread("Changing window position must be done on the thread that created the window.");
                // move window via SDL call
                SDL_SetWindowPosition(_window, value.Item1, value.Item2);
                _windowPosition.Item1 = value.Item1;
                _windowPosition.Item2 = value.Item2;
            }
        }

        public bool Enabled { get; set; } = true;
        public bool HandleClose { get; set; } = true;
        public Window? ParentWindow { get; init; }

        public dynamic Layout { get; private set; }

        public Color BackgroundColor { get; set; }

        internal Window(Layout windowLayout, string windowTitle, Size windowSize, int threadId, IGraphicsFactory graphicsFactory, Window? parentWindow = null)
        {
            _initialized = false;
            _threadId = threadId;
            WindowTitle = windowTitle;
            WindowSize = windowSize;
            Layout = windowLayout;
            ParentWindow = parentWindow;
            _graphicsFactory = graphicsFactory;
            BackgroundColor = new Color(230, 230, 230);
        }

        internal void InternalClose()
        {
            EnsureMainThread();
            GraphicsContext?.Cleanup();
            SDL_DestroyWindow(_window);
            Closed = true;
        }
        public void Close()
        {
            if (!Closed)
                CeruleanAPI.GetAPI().CloseWindow(this);
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
            if ((_window = SDL_CreateWindow(WindowTitle,
                SDL_WINDOWPOS_CENTERED,
                SDL_WINDOWPOS_CENTERED,
                WindowSize.W,
                WindowSize.H,
                SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI)) == IntPtr.Zero)
            {
                throw new FatalAPIException("Failed to create window.");
            }
            SDL_GetWindowPosition(_window, out _windowPosition.Item1, out _windowPosition.Item2);
            GraphicsContext = _graphicsFactory.CreateGraphics(this);
            Layout.Init();
        }

        internal void Draw()
        {

            GraphicsContext?.RenderClear(BackgroundColor);
            if (GraphicsContext is not null)
                Layout.Draw(GraphicsContext);
            GraphicsContext?.RenderPresent();
        }

        internal void InvokeOnResize(int w, int h)
        {
            GraphicsContext?.SetRenderArea(new(w, h), 0, 0);
            _windowSize = new(w, h);
            SDL_GetWindowPosition(_window,
                out _windowPosition.Item1,
                out _windowPosition.Item2);
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
            _closeFromEvent = false;
        }

        internal void InvokeOnMinimize()
        {
            _windowPosition.Item1 = 0;
            _windowPosition.Item2 = 0;
            OnMininize?.Invoke(this, new());
        }

        internal void InvokeOnRestore()
        {
            SDL_GetWindowPosition(_window,
                out _windowPosition.Item1,
                out _windowPosition.Item2);
            OnRestore?.Invoke(this, new());
        }

        internal void InvokeOnMaximize()
        {
            SDL_GetWindowPosition(_window,
                out _windowPosition.Item1,
                out _windowPosition.Item2);
            OnMaximize?.Invoke(this, new());
        }

        internal void InvokeOnMoved(int x, int y)
        {
            _windowPosition.Item1 = x;
            _windowPosition.Item2 = y;
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