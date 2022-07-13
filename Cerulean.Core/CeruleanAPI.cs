﻿using Cerulean.Common;
using Cerulean.Core.Logging;
using System.Collections.Concurrent;
using System.Runtime.InteropServices.ComTypes;
using static SDL2.SDL;

namespace Cerulean.Core
{
    internal sealed class CeruleanQuitException : Exception
    {
        public CeruleanQuitException() : base("Cerulean Quit")
        {
        }
    }

    /// <summary>
    /// The CeruleanAPI Controller
    /// </summary>
    public sealed class CeruleanAPI
    {
        #region Private Fields
        private static readonly CeruleanAPI _instance = new();

        private const int MAX_WORK_ITEMS = 5;

        private readonly Thread _thread;
        private readonly ConcurrentDictionary<uint, Window> _windows;
        private readonly ConcurrentQueue<WorkItem> _workItems;
        private readonly EmbeddedLayouts _embeddedLayouts;

        private ILoggingService? _logger;
        private IGraphicsFactory? _graphicsFactory;
        private bool _initialized;
        private bool _running;
        private bool _stopped;
        private bool _quitting;
        private int _threadId;
        #endregion

        /// <summary>
        /// Set of all open windows.
        /// </summary>
        public IEnumerable<Window> Windows => _windows.Values;

        /// <summary>
        /// The global profiler used for logging execution time.
        /// </summary>
        public Profiler? Profiler { get; }

        #region Event Handling Methods
        /// <summary>
        /// SDL Window Event Handler
        /// </summary>
        /// <param name="sdlEvent">The SDL_Event from polling events.</param>
        private void HandleWindowEvent(SDL_Event sdlEvent)
        {
            if (!_windows.TryGetValue(sdlEvent.window.windowID, out var window))
                return;
            switch (sdlEvent.window.windowEvent)
            {
                case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                    window.OnCloseFromEvent = true;
                    window.InvokeOnClose();
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                    window.InvokeOnResize(sdlEvent.window.data1, sdlEvent.window.data2);
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                    window.InvokeOnFocusGained();
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                    window.InvokeOnFocusLost();
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                    window.InvokeOnMinimize();
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
                    window.InvokeOnRestore();
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
                    window.InvokeOnMaximize();
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                    window.InvokeOnMoved(sdlEvent.window.data1, sdlEvent.window.data2);
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
                    window.InvokeOnMouseEnter();
                    break;
                case SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
                    window.InvokeOnMouseLeave();
                    break;
            }
        }
        #endregion

        #region Private Auxilliary Methods
        /// <summary>
        /// Default constructor.
        /// </summary>
        private CeruleanAPI()
        {
            _thread = new Thread(new ThreadStart(WorkerThread));
            _windows = new ConcurrentDictionary<uint, Window>();
            _workItems = new ConcurrentQueue<WorkItem>();
            _embeddedLayouts = new EmbeddedLayouts();
            Profiler = null;
        }

        /// <summary>
        /// Checks if the instance is initialized.
        /// <exception cref="InvalidOperationException">Thrown when the instance is not yet initialized.</exception>
        /// </summary>
        private void EnsureInitialized()
        {
            var timer = 0;
            const int max = 10;
            while (!_initialized)
            {
                Thread.Sleep(1000);
                if (timer < max)
                    timer++;
                else
                {
                    var exception = new InvalidOperationException("The CeruleanAPI instance needs to be initialized first.");
                    _logger?.Log(exception.Message, LogSeverity.Error, exception);
                    throw exception;
                }
            }
        }

        /// <summary>
        /// The main CeruleanAPI thread.
        /// </summary>
        /// <exception cref="FatalAPIException">Thrown when SDL2 could not be initialized.</exception>
        private void WorkerThread()
        {
            // start profiling
            Profiler?.StartProfilingPoint("WorkerThread");
            _logger?.Log("CeruleanAPI thread started...");
            _threadId = Environment.CurrentManagedThreadId;

            // initialize SDL2 and set needed hints
            SDL_SetHint("SDL_HINT_VIDEO_HIGHDPI_ENABLED", "1");
            SDL_SetHint(SDL_HINT_RENDER_DRIVER, "opengl");
            if (SDL_InitSubSystem(SDL_INIT_EVERYTHING) != 0)
                throw new FatalAPIException($"Could not initialize SDL2. Reason: {SDL_GetError()}");
            SDL_VERSION(out var version);
            _logger?.Log($"Running on SDL {version.major}.{version.minor}.{version.patch}.");
            _initialized = true;
            _logger?.Log("Initialized SDL2.");

            // Start event loop
            try
            {
                while (_running)
                {
                    // start offloading and running work items
                    Profiler?.StartProfilingPoint("WorkQueue_Offload");
                    var offloaded = 0;
                    while (!_workItems.IsEmpty && offloaded < MAX_WORK_ITEMS)
                    {
                        if (_workItems.TryDequeue(out var workItem))
                        {
                            workItem.BeginTask();
                        }
                        offloaded++;
                    }
                    Profiler?.EndProfilingCurrentPoint();

                    // start processing sdl events
                    Profiler?.StartProfilingPoint("Handle_SDLEvents");
                    while (SDL_PollEvent(out var sdlEvent) != 0)
                    {
                        switch (sdlEvent.type)
                        {
                            case SDL_EventType.SDL_WINDOWEVENT:
                                HandleWindowEvent(sdlEvent);
                                break;
                        }
                    }
                    Profiler?.EndProfilingCurrentPoint();

                    // update and draw windows
                    foreach (var (windowId, window) in _windows)
                    {
                        Profiler?.StartProfilingPoint($"Window_{windowId}");
                        var clientArea = window.WindowSize;

                        // update window graphics context and reset viewport + global position offset
                        window.GraphicsContext?.Update();
                        window.GraphicsContext?.SetRenderArea(clientArea, 0, 0);
                        window.GraphicsContext?.SetGlobalPosition(0, 0);

                        // update and draw window
                        window.Layout.Update(window, clientArea);
                        window.Draw();
                        Profiler?.EndProfilingCurrentPoint();
                    }
                }
            }
            // this exception is thrown by Quit() which functions as a stop signal for the event pump/loop.
            catch (CeruleanQuitException)
            {
                _logger?.Log("CeruleanAPI thread stopped (called by event on main thread).");
            }
            Profiler?.EndProfilingCurrentPoint();
            _stopped = true;
        }
        #endregion

        /// <summary>
        /// Gets the current instance of the controller singleton.
        /// </summary>
        /// <returns>The singleton instance of CeruleanAPI.</returns>
        public static CeruleanAPI GetAPI()
        {
            return _instance;
        }

        /// <summary>
        /// Initializes the instance once.
        /// Does nothing when called from an initialized instance.
        /// </summary>
        /// <returns>The current initialized controller instance.</returns>
        public CeruleanAPI Initialize()
        {
            if (_initialized)
                return this;
            _logger?.Log("Initializing CeruleanAPI...");
            _running = true;
            _stopped = false;
            _logger?.Log("Loading embedded layouts...");
            _embeddedLayouts.RetrieveLayouts();
            _thread.Start();
            return this;
        }

        /// <summary>
        /// Quits the CeruleanAPI controller instance.
        /// Handles the shutdown of SDL subsystems and window disposal.
        /// </summary>
        public void Quit()
        {
            if (!_initialized || _quitting)
                return;

            // start quitting
            _quitting = true;
            _logger?.Log("Quitting CeruleanAPI...");

            // invoke CloseWindow on all open windows
            ConcurrentDictionary<uint, Window> copy = new(_windows);
            _logger?.Log("Closing open windows...");
            foreach (var pair in copy)
            {
                CloseWindow(pair.Value);
            }

            // signal thread stop via _running and CeruleanQuitException if Quit() was called from the CeruleanAPI thread.
            // if not, wait for the thread to stop via _running & _stopped flag.
            _running = false;
            if (_threadId != Environment.CurrentManagedThreadId)
            {
                _logger?.Log("Waiting for thread to stop...");
                while (!_stopped)
                {
                    Thread.Sleep(500);
                }
            }
            else
            {
                _logger?.Log("Quit() was called from the CeruleanAPI thread, interuptting thread...");
                throw new CeruleanQuitException();
            }
            _initialized = false;
        }

        /// <summary>
        /// Waits for all windows to be closed.
        /// </summary>
        /// <param name="quitOnComplete">Invoke Quit() on completion?</param>
        public void WaitForAllWindowsClosed(bool quitOnComplete = false)
        {
            if (!_initialized)
                return;
            while (Windows.Any())
            {
                Thread.Sleep(100);
            }
            if (quitOnComplete)
                Quit();
        }

        /// <summary>
        /// Sets the single logging service to be used globally.
        /// </summary>
        /// <param name="loggingService">The logging service dependency.</param>
        /// <returns>The current CeruleanAPI controller instance.</returns>
        public CeruleanAPI UseLogger(ILoggingService loggingService)
        {
            _logger = loggingService;
            _embeddedLayouts.SetLogger(loggingService);
            return this;
        }

        /// <summary>
        /// Sets the console logger as the logging service dependency.
        /// </summary>
        /// <returns>The current CeruleanAPI controller instance.</returns>
        public CeruleanAPI UseConsoleLogger()
            => UseLogger(new ConsoleLoggingService());

        /// <summary>
        /// Sets the graphics factory used when creating windows. 
        /// </summary>
        /// <param name="graphicsFactory">The graphics factory dependency.</param>
        /// <returns>The current CeruleanAPI controller instance.</returns>
        public CeruleanAPI UseGraphicsFactory(IGraphicsFactory graphicsFactory)
        {
            _graphicsFactory = graphicsFactory;
            return this;
        }

        /// <summary>
        /// Sets the SDL2 graphics factory as graphics factory dependency.
        /// </summary>
        /// <returns>The current CeruleanAPI controller instance.</returns>
        public CeruleanAPI UseSDL2Graphics()
            => UseGraphicsFactory(new SDL2GraphicsFactory());

        /// <summary>
        /// Creates a window from a given layout.
        /// </summary>
        /// <param name="windowLayout">The layout to use.</param>
        /// <param name="windowTitle">The initial window title.</param>
        /// <param name="windowSize">The initial window size.</param>
        /// <param name="initialize">Whether to initialize window on creation.</param>
        /// <returns>A Cerulean window instance.</returns>
        public Window CreateWindow(Layout windowLayout,
                                   string windowTitle = "CeruleanAPI Window",
                                   Size? windowSize = null,
                                   bool initialize = true)
        {
            EnsureInitialized();
            if (_graphicsFactory is null)
            {
                var error1 = new FatalAPIException("No graphics factory specified.");
                _logger?.Log(error1.Message, LogSeverity.Fatal, error1);
                throw error1;
            }
            var result = DoOnThread((args) =>
            {
                Window window = new(windowLayout, windowTitle, windowSize ?? Window.DefaultWindowSize, _threadId, _graphicsFactory);
                if ((bool)args[0])
                    InitializeWindow(window);
                return window;
            }, initialize);

            if (result is Window resultWindow) return resultWindow;
            
            var error2 = new GeneralAPIException("Window could not be created.");
            _logger?.Log(error2.Message, LogSeverity.Error, error2);
            throw error2;
        }

        /// <summary>
        /// Creates a window from the name of an embedded layout.
        /// </summary>
        /// <param name="windowLayoutName">The name of the embedded layout to use.</param>
        /// <param name="windowTitle">The initial window title.</param>
        /// <param name="windowSize">The initial window size.</param>
        /// <param name="initialize">Whether to initialize window on creation.</param>
        /// <returns>A Cerulean window instance.</returns>
        public Window CreateWindow(string windowLayoutName,
                                   string windowTitle = "CeruleanAPI Window",
                                   Size? windowSize = null,
                                   bool initialize = true)
        {
            return CreateWindow(FetchLayout(windowLayoutName), windowTitle, windowSize, initialize);
        }

        /// <summary>
        /// Closes an open window.
        /// </summary>
        /// <param name="apiWindow">The window to close.</param>
        public void CloseWindow(Window apiWindow)
        {
            EnsureInitialized();
            DoOnThread((args) =>
            {
                // early return if window is not initialized or is already closed.
                if (args.Length <= 0 ||
                    args[0] is not Window { IsInitialized: true } window)
                    return;
                if (window.Closed) return;

                // simulate OnClose event
                if (!window.OnCloseFromEvent)
                {
                    window.OnCloseFromEvent = true;
                    window.InvokeOnClose();
                }
                else
                {
                    var windowId = SDL_GetWindowID(window.WindowPtr);
                    window.InternalClose();
                    _windows.Remove(windowId, out _);
                }
            }, apiWindow);
        }

        /// <summary>
        /// Initialize a created window.
        /// </summary>
        /// <param name="apiWindow">The window to initialize.</param>
        public void InitializeWindow(Window apiWindow)
        {
            DoOnThread((args) =>
            {
                if (args.Length <= 0 ||
                    args[0] is not Window { IsInitialized: false } window)
                    return;
                window.Initialize();
                _windows.TryAdd(SDL_GetWindowID(window.WindowPtr), window);
            }, apiWindow);
        }

        /// <summary>
        /// Fetches an embedded layout by name.
        /// </summary>
        /// <param name="name">The name of the embedded layout.</param>
        /// <returns>The embedded layout.</returns>
        public Layout FetchLayout(string name)
        {
            EnsureInitialized();
            return _embeddedLayouts.FetchLayout(name);
        }

        /// <summary>
        /// Enqueue a function to be handled by the Cerulean thread.
        /// </summary>
        /// <param name="func">The function to enqueue.</param>
        /// <param name="args">Arguments to pass to the function.</param>
        /// <returns>The result of the function.</returns>
        public object? DoOnThread(Func<object[], object> func, params object[] args)
        {
            EnsureInitialized();
            WorkItem work = new(func, args);

            // if thread is the CeruleanAPI thread
            if (Environment.CurrentManagedThreadId == _threadId)
            {
                work.BeginTask();
            }
            else
            {
                // thread is NOT the CeruleanAPI thread
                while (_workItems.Count >= MAX_WORK_ITEMS)
                    Task.Delay(200).Wait();
                _workItems.Enqueue(work);
            }

            return work.WaitForCompletion();
        }

        /// <summary>
        /// Enqueue an action to be handled by the Cerulean thread.
        /// </summary>
        /// <param name="action">The action to enqueue.</param>
        /// <param name="arg">Arguments to pass to the action.</param>
        public void DoOnThread(Action<object[]> action, params object[] arg)
        {
            EnsureInitialized();
            WorkItem work = new(action, arg);

            // if thread is the CeruleanAPI thread
            if (Environment.CurrentManagedThreadId == _threadId)
            {
                work.BeginTask();
            }
            else
            {
                // thread is NOT the CeruleanAPI thread
                while (_workItems.Count >= MAX_WORK_ITEMS)
                    Task.Delay(200).Wait();
                _workItems.Enqueue(work);
            }

            work.WaitForCompletion();
        }

        /// <summary>
        /// Logs a message via the logging service.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="severity">The severity of the event.</param>
        public void Log(string message, LogSeverity severity = LogSeverity.General)
        {
            _logger?.Log(message, severity);
        }

        /// <summary>
        /// Logs a message via the logging service.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="exception">Additional exception data.</param>
        public void Log(string message, LogSeverity severity, Exception exception)
        {
            _logger?.Log(message, severity, exception);
        }
    }
}