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
        private readonly int _threadId;
        internal IntPtr WindowPtr { get; private set; }

        internal bool OnCloseFromEvent = false;
        private string _windowTitle = "";
        #endregion

        public static readonly Size DefaultWindowSize = new(600, 400);
        public delegate void WindowEventHandler(Window sender, WindowEventArgs e);
        public WindowEventHandler? OnResize, OnMinimize, OnRestore, OnMaximize, OnMoved, OnClose, OnMouseLeave, OnMouseEnter, OnFocusGained, OnFocusLost;
        public bool IsInitialized { get; private set; } = false;

        public bool Closed { get; private set; }
        public IGraphics? GraphicsContext { get; private set; } = null;

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                if (Closed)
                    throw new GeneralAPIException("Cannot set title to a closed window.");
                if (!IsInitialized)
                {
                    _windowTitle = value;
                }
                else
                {
                    EnsureMainThread("Changing window title must be done on the thread that created the window.");
                    // change window title via SDL call
                    SDL_SetWindowTitle(WindowPtr, value);
                    _windowTitle = value;
                }
            }
        }

        private Size _windowSize;

        public Size WindowSize
        {
            get => _windowSize;
            set
            {
                if (Closed)
                    throw new GeneralAPIException("Cannot set size to a closed window.");
                if (!IsInitialized)
                {
                    _windowSize = value;
                }
                else
                {
                    EnsureMainThread("Changing window size must be done on the thread that created the window.");
                    // change window size via SDL call
                    SDL_SetWindowSize(WindowPtr, value.W, value.H);
                    GraphicsContext?.SetRenderArea(value, 0, 0);
                    _windowSize = value;
                }
            }
        }

        private Size _minimumWindowSize;

        public Size? MinimumWindowSize
        {
            get => _minimumWindowSize;
            set 
            {
                if (Closed)
                    throw new GeneralAPIException("Cannot set minimum size to a closed window.");
                if (!IsInitialized)
                {
                    _minimumWindowSize = value ?? new Size(0, 0);
                }
                else
                {
                    EnsureMainThread("Changing minimum window size must be done on the thread that created the window.");
                    // change minimum window size via SDL call
                    if (value.HasValue)
                    {
                        SDL_SetWindowMinimumSize(WindowPtr, value.Value.W, value.Value.H);
                        _minimumWindowSize = value.Value;
                    }
                    else
                    {
                        SDL_SetWindowMinimumSize(WindowPtr, 0, 0);
                        _minimumWindowSize = new Size(0,0);
                    }
                }
            }
        }

        private Size _maximumWindowSize;

        public Size? MaximumWindowSize
        {
            get => _maximumWindowSize;
            set
            {
                if (Closed)
                    throw new GeneralAPIException("Cannot set maximum size to a closed window.");
                if (!IsInitialized)
                {
                    _maximumWindowSize = value ?? new Size(-1, -1);
                }
                else
                {
                    EnsureMainThread("Changing maximum window size must be done on the thread that created the window.");
                    // change maximum window size via SDL call
                    if (value.HasValue)
                    {
                        SDL_SetWindowMaximumSize(WindowPtr, value.Value.W, value.Value.H);
                        _maximumWindowSize = value.Value;
                    }
                    else
                    {
                        SDL_SetWindowMaximumSize(WindowPtr, -1, -1);
                        _maximumWindowSize = new(-1, -1);
                    }
                }
            }
        }

        private (int, int) _windowPosition;

        public (int, int) WindowPosition
        {
            get => (_windowPosition.Item1, _windowPosition.Item2);
            set
            {
                EnsureMainThread("Changing window position must be done on the thread that created the window.");
                // move window via SDL call
                SDL_SetWindowPosition(WindowPtr, value.Item1, value.Item2);
                _windowPosition.Item1 = value.Item1;
                _windowPosition.Item2 = value.Item2;
            }
        }

        public bool Enabled { get; set; } = true;
        public bool HandleClose { get; set; } = true;
        public Window? ParentWindow { get; init; }

        public dynamic Layout { get; private set; }

        public Color BackColor { get; set; }

        internal Window(Layout windowLayout, string windowTitle, Size windowSize, int threadId, IGraphicsFactory graphicsFactory, Window? parentWindow = null)
        {
            IsInitialized = false;
            _threadId = threadId;
            WindowTitle = windowTitle;
            WindowSize = windowSize;
            MinimumWindowSize = null;
            MaximumWindowSize = null;
            Layout = windowLayout;
            ParentWindow = parentWindow;
            _graphicsFactory = graphicsFactory;
            BackColor = new Color(230, 230, 230);
        }

        internal void InternalClose()
        {
            EnsureMainThread();
            GraphicsContext?.Cleanup();
            SDL_DestroyWindow(WindowPtr);
            Closed = true;
        }
        public void Close()
        {
            if (!Closed)
                CeruleanAPI.GetAPI().CloseWindow(this);
        }

        private void EnsureMainThread(string? message = null)
        {
            if (_threadId == Environment.CurrentManagedThreadId) return;
            var exception = message == null ?
                new ThreadSafetyException() :
                new ThreadSafetyException(message);
            CeruleanAPI.GetAPI().Log(exception.Message, LogSeverity.Error, exception);
            throw exception;
        }

        internal void Initialize()
        {
            EnsureMainThread("Window initialization must be called from the thread that initialized the CeruleanAPI instance.");
            IsInitialized = true;
            if ((WindowPtr = SDL_CreateWindow(WindowTitle,
                SDL_WINDOWPOS_CENTERED,
                SDL_WINDOWPOS_CENTERED,
                WindowSize.W,
                WindowSize.H,
                SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI)) == IntPtr.Zero)
            {
                var exception = new FatalAPIException("Failed to create window.");
                CeruleanAPI.GetAPI().Log(exception.Message, LogSeverity.Fatal, exception);
                throw exception;
            }
            SDL_GetWindowPosition(WindowPtr, out _windowPosition.Item1, out _windowPosition.Item2);
            CeruleanAPI.GetAPI().Log("Window created.");
            CeruleanAPI.GetAPI().Log($"Minimum size {_minimumWindowSize}.");
            CeruleanAPI.GetAPI().Log($"Maximum size {_maximumWindowSize}.");
            SDL_SetWindowMinimumSize(WindowPtr, _minimumWindowSize.W, _minimumWindowSize.H);
            SDL_SetWindowMaximumSize(WindowPtr, _maximumWindowSize.W, _maximumWindowSize.H);
            GraphicsContext = _graphicsFactory.CreateGraphics(this);
            Layout.Init();
        }

        internal void Draw()
        {
            CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("Draw");
            GraphicsContext?.RenderClear();
            GraphicsContext?.DrawFilledRectangle(0, 0, _windowSize, BackColor);
            if (GraphicsContext is not null)
            {
                GraphicsContext.SetRenderArea(_windowSize, 0, 0);
                Layout.Draw(GraphicsContext, 0, 0, _windowSize);
            }

            GraphicsContext?.RenderPresent();
            CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();
        }

        internal void InvokeOnResize(int w, int h)
        {
            GraphicsContext?.SetRenderArea(new(w, h), 0, 0);
            _windowSize = new Size(w, h);
            SDL_GetWindowPosition(WindowPtr,
                out _windowPosition.Item1,
                out _windowPosition.Item2);
            OnResize?.Invoke(this, new WindowEventArgs
            {
                WindowWidth = w,
                WindowHeight = h
            });
        }

        internal void InvokeOnClose()
        {
            var cancel = false;
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
            OnCloseFromEvent = false;
        }

        internal void InvokeOnMinimize()
        {
            _windowPosition.Item1 = 0;
            _windowPosition.Item2 = 0;
            OnMinimize?.Invoke(this, new WindowEventArgs());
        }

        internal void InvokeOnRestore()
        {
            SDL_GetWindowPosition(WindowPtr,
                out _windowPosition.Item1,
                out _windowPosition.Item2);
            OnRestore?.Invoke(this, new WindowEventArgs());
        }

        internal void InvokeOnMaximize()
        {
            SDL_GetWindowPosition(WindowPtr,
                out _windowPosition.Item1,
                out _windowPosition.Item2);
            OnMaximize?.Invoke(this, new WindowEventArgs());
        }

        internal void InvokeOnMoved(int x, int y)
        {
            _windowPosition.Item1 = x;
            _windowPosition.Item2 = y;
            OnMoved?.Invoke(this, new WindowEventArgs
            {
                WindowX = x,
                WindowY = y
            });
        }

        internal void InvokeOnFocusGained()
        {
            OnFocusGained?.Invoke(this, new WindowEventArgs());
        }

        internal void InvokeOnMouseLeave()
        {
            OnMouseLeave?.Invoke(this, new WindowEventArgs());
        }

        internal void InvokeOnMouseEnter()
        {
            OnMouseEnter?.Invoke(this, new WindowEventArgs());
        }

        internal void InvokeOnFocusLost()
        {
            OnFocusLost?.Invoke(this, new WindowEventArgs());
        }
    }
}