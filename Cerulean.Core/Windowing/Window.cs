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
        public string Text { get; set; } = string.Empty;
        public bool Cancel { get; set; }
    }
    public sealed class Window : IWindow
    {
        #region Private+Internals
        private readonly IGraphicsFactory _graphicsFactory;
        private readonly int _threadId;

        internal bool OnCloseFromEvent;
        private string _windowTitle = "";
        private Component? _imeComponentOwner;
        internal bool IsModalWindow = false;
        #endregion

        public Action<string>? TextUpdatedDelegate { get; set; }
        public IntPtr WindowPtr { get; private set; }

        /// <summary>
        /// The default window size for windows created without size specified.
        /// </summary>
        //public static readonly Size DefaultWindowSize = new(600, 400);
        public delegate void WindowEventHandler(Window sender, WindowEventArgs e);

        #region Events
        /// <summary>
        /// Called when the window has finished resizing.
        /// </summary>
        public WindowEventHandler? OnResize;
        /// <summary>
        /// Called when the window is minimized.
        /// </summary>
        public WindowEventHandler? OnMinimize;
        /// <summary>
        /// Called when the window is restored.
        /// </summary>
        public WindowEventHandler? OnRestore;
        /// <summary>
        /// Called when the window is maximized.
        /// </summary>
        public WindowEventHandler? OnMaximize;
        /// <summary>
        /// Called when the window has finished moving.
        /// </summary>
        public WindowEventHandler? OnMoved;
        /// <summary>
        /// Called when the window has received a window close event.
        /// i.e: clicking the 'x' button, or pressing alt-f4.
        /// </summary>
        public WindowEventHandler? OnClose;
        /// <summary>
        /// Called when the mouse pointer leaves the window area.
        /// </summary>
        public WindowEventHandler? OnMouseLeave;
        /// <summary>
        /// Called when the mouse pointer enters the window area.
        /// </summary>
        public WindowEventHandler? OnMouseEnter;
        /// <summary>
        /// Called when the window is focused.
        /// </summary>
        public WindowEventHandler? OnFocusGained;
        /// <summary>
        /// Called when the window loses focus.
        /// </summary>
        public WindowEventHandler? OnFocusLost;
        #endregion

        /// <summary>
        /// Returns true if the window has been initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }
        /// <summary>
        /// Returns true if the window has been closed.
        /// </summary>
        public bool Closed { get; private set; }
        /// <summary>
        /// The redraw flag that determines if the window should be redrawn for the cycle.
        /// </summary>
        public bool IsFlaggedForRedraw { get; private set; } = true;
        /// <summary>
        /// An override for the redraw flag that makes the window be drawn for each cycle.
        /// Makes the window draw function like a game draw loop.
        /// </summary>
        public bool AlwaysRedraw { get; set; } = false;
        /// <summary>
        /// Scales the components of this window via dpi scaling.
        /// </summary>
        public bool AutoScale { get; set; } = false;
        /// <summary>
        /// The graphics context or backend that the window is using.
        /// </summary>
        public IGraphics? GraphicsContext { get; private set; }
        /// <summary>
        /// The title of the window.
        /// <remarks>
        /// Must be called on the thread that created the window.
        /// </remarks>
        /// </summary>
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
        /// <summary>
        /// The size of the window.
        /// <remarks>
        /// Must be called on the thread that created the window.
        /// </remarks>
        /// </summary>
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
        /// <summary>
        /// The minimum size of the window.
        /// <remarks>
        /// Must be called on the thread that created the window.
        /// </remarks>
        /// </summary>
        public Size? MinimumWindowSize
        {
            get => _minimumWindowSize;
            set
            {
                if (Closed)
                    throw new GeneralAPIException("Cannot set minimum size to a closed window.");
                if (!IsInitialized)
                {
                    _minimumWindowSize = value ?? new Size(1, 1);
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
                        SDL_SetWindowMinimumSize(WindowPtr, 1, 1);
                        _minimumWindowSize = new Size(1, 1);
                    }
                }
            }
        }
        private Size _maximumWindowSize;
        /// <summary>
        /// The maximum size of the window.
        /// <remarks>
        /// Must be called on the thread that created the window.
        /// </remarks>
        /// </summary>
        public Size? MaximumWindowSize
        {
            get => _maximumWindowSize;
            set
            {
                if (Closed)
                    throw new GeneralAPIException("Cannot set maximum size to a closed window.");
                if (!IsInitialized)
                {
                    _maximumWindowSize = value ?? new Size(ushort.MaxValue, ushort.MaxValue);
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
                        SDL_SetWindowMaximumSize(WindowPtr, ushort.MaxValue, ushort.MaxValue);
                        _maximumWindowSize = new Size(ushort.MaxValue, ushort.MaxValue);
                    }
                }
            }
        }
        private (int, int) _windowPosition;
        /// <summary>
        /// The position of the window.
        /// <remarks>
        /// Must be called on the thread that created the window.
        /// </remarks>
        /// </summary>
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
        /// <summary>
        /// The window enabled property.
        /// <remarks>
        /// Setting this to false will disable event processing.
        /// </remarks>
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// Tells whether close events are processed.
        /// <remarks>
        /// Setting this to false will disable the functionality of the window close button and alt-f4.
        /// As a result, the OnClose event will not be called.
        /// </remarks>
        /// </summary>
        public bool HandleClose { get; set; } = true;
        /// <summary>
        /// The parent window of this window.
        /// </summary>
        public IWindow? ParentWindow { get; init; }
        /// <summary>
        /// The layout being used by this window.
        /// </summary>
        public dynamic Layout { get; }
        /// <summary>
        /// The background fill color for this window.
        /// </summary>
        public Color BackColor { get; set; }
        /// <summary>
        /// The OpenGL context created by SDL.
        /// </summary>
        public IntPtr GLContext { get; private set; }
        /// <summary>
        /// The top-most hovered component.
        /// </summary>
        public Component? HoveredComponent { get; internal set; }
        /// <summary>
        /// Tells whether the window is a modal dialog.
        /// </summary>
        public bool IsModal => IsModalWindow;

        internal Window(Layout windowLayout, string windowTitle, Size windowSize, int threadId, IGraphicsFactory graphicsFactory, Window? parentWindow = null)
        {
            IsInitialized = false;
            _threadId = threadId;
            WindowTitle = windowTitle;
            WindowSize = windowSize;
            MinimumWindowSize = null;
            MaximumWindowSize = null;
            Layout = windowLayout;
            windowLayout.ParentWindow = this;
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

        /// <summary>
        /// Sends a window close event.
        /// </summary>
        public void Close()
        {
            if (!Closed)
                CeruleanAPI.GetAPI().CloseWindow(this);
        }

        /// <summary>
        /// Starts IME Text Input.
        /// </summary>
        /// <param name="inputOwner">The component that owns the IME input session.</param>
        /// <param name="x">The X coordinate of where the input box is located at. This is relative to the window.</param>
        /// <param name="y">The Y coordinate of where the input box is located at. This is relative to the window.</param>
        /// <param name="area">The size of the input box.</param>
        /// <param name="text">The initial text of the IME text input.</param>
        /// <param name="index">The index of the text cursor.</param>
        public void StartTextInput(Component? inputOwner, int x, int y, Size area, string text = "", int index = 0,
            int maxLength = 2048)
        {
            _imeComponentOwner = inputOwner;
            TextUpdatedDelegate = null;
            CeruleanAPI.GetAPI().StartTextInput(this, x, y, area, text, index, maxLength);
        }

        /// <summary>
        /// Stops IME Text Input.
        /// </summary>
        public void StopTextInput(Component? inputOwner = null)
        {
            if (inputOwner == null || inputOwner == _imeComponentOwner)
                CeruleanAPI.GetAPI().StopTextInput(this);
        }

        /// <summary>
        /// Raises the redraw flag for this window.
        /// </summary>
        public void FlagForRedraw()
        {
            IsFlaggedForRedraw = true;
        }

        public int GetDpiScaledValue(int value)
        {
            return Scaling.GetDpiScaledValue(this, value);
        }

        public double GetDpiScaledValue(double value)
        {
            return Scaling.GetDpiScaledValue(this, value);
        }

        public float GetDpiScaledValue(float value)
        {
            return (float)Scaling.GetDpiScaledValue(this, value);
        }

        public Size GetDpiScaledValue(Size value)
        {
            var ret = new Size
            {
                W = GetDpiScaledValue(value.W),
                H = GetDpiScaledValue(value.H)
            };
            return ret;
        }

        public void SetComponentScaledX(Component component, int x)
        {
            component.X = GetDpiScaledValue(x);
        }

        public void SetComponentScaledY(Component component, int y)
        {
            component.Y = GetDpiScaledValue(y);
        }

        public void SetComponentScaledPosition(Component component, int x, int y)
        {
            component.X = GetDpiScaledValue(x);
            component.Y = GetDpiScaledValue(y);
        }

        public void SetComponentScaledSize(ISized component, Size size)
        {
            component.Size = Scaling.GetDpiScaledValue(this, size);
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
                SDL_WindowFlags.SDL_WINDOW_RESIZABLE
                | SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI
                | SDL_WindowFlags.SDL_WINDOW_OPENGL)) == IntPtr.Zero)
            {
                var exception = new FatalAPIException("Failed to create window.");
                CeruleanAPI.GetAPI().Log(exception.Message, LogSeverity.Fatal, exception);
                throw exception;
            }

            GLContext = SDL_GL_CreateContext(WindowPtr);
            SDL_GetWindowPosition(WindowPtr, out _windowPosition.Item1, out _windowPosition.Item2);
            CeruleanAPI.GetAPI().Log("Window created.");
            CeruleanAPI.GetAPI().Log($"Minimum size {_minimumWindowSize}.");
            CeruleanAPI.GetAPI().Log($"Maximum size {_maximumWindowSize}.");
            SDL_SetWindowMinimumSize(WindowPtr, _minimumWindowSize.W, _minimumWindowSize.H);
            SDL_SetWindowMaximumSize(WindowPtr, _maximumWindowSize.W, _maximumWindowSize.H);
            SDL_SetWindowAlwaysOnTop(WindowPtr, IsModalWindow ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE);
            GraphicsContext = _graphicsFactory.CreateGraphics(this);
            Layout.Init();
            ((Layout)Layout).ApplyStyles();
        }

        internal void Draw()
        {
            CeruleanAPI.GetAPI().Profiler?.StartProfilingPoint("Draw");
            GraphicsContext?.RenderClear();
            GraphicsContext?.DrawFilledRectangle(0, 0, _windowSize, BackColor);
            if (GraphicsContext is not null)
            {
                GraphicsContext.SetRenderArea(_windowSize, 0, 0);
                GraphicsContext.SetGlobalPosition(0, 0);
                Layout.Draw(GraphicsContext, 0, 0, _windowSize);
            }

            GraphicsContext?.RenderPresent();
            CeruleanAPI.GetAPI().Profiler?.EndProfilingCurrentPoint();
        }

        internal void UnFlagRedraw()
        {
            IsFlaggedForRedraw = false;
        }

        internal void InvokeOnResize(int w, int h)
        {
            GraphicsContext?.SetRenderArea(new(w, h), 0, 0);
            _windowSize = new Size(w, h);
            SDL_GetWindowPosition(WindowPtr,
                out _windowPosition.Item1,
                out _windowPosition.Item2);
            FlagForRedraw();
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
            FlagForRedraw();
            OnRestore?.Invoke(this, new WindowEventArgs());
        }

        internal void InvokeOnMaximize()
        {
            SDL_GetWindowPosition(WindowPtr,
                out _windowPosition.Item1,
                out _windowPosition.Item2);
            FlagForRedraw();
            OnMaximize?.Invoke(this, new WindowEventArgs());
        }

        internal void InvokeOnMoved(int x, int y)
        {
            _windowPosition.Item1 = x;
            _windowPosition.Item2 = y;
            FlagForRedraw();
            OnMoved?.Invoke(this, new WindowEventArgs
            {
                WindowX = x,
                WindowY = y
            });
        }

        internal void InvokeOnFocusGained()
        {
            FlagForRedraw();
            OnFocusGained?.Invoke(this, new WindowEventArgs());
        }

        internal void InvokeOnMouseLeave()
        {
            OnMouseLeave?.Invoke(this, new WindowEventArgs());
        }

        internal void InvokeOnMouseEnter()
        {
            FlagForRedraw();
            OnMouseEnter?.Invoke(this, new WindowEventArgs());
        }

        internal void InvokeOnFocusLost()
        {
            FlagForRedraw();
            OnFocusLost?.Invoke(this, new WindowEventArgs());
        }

        internal void InvokeOnTextUpdate(string text)
        {
            FlagForRedraw();
            TextUpdatedDelegate?.Invoke(text);
        }
    }
}