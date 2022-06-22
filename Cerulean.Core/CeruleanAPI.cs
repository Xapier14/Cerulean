using Cerulean.Common;
using Cerulean.Core.Logging;
using System.Collections.Concurrent;
using static SDL2.SDL;

namespace Cerulean.Core
{
    internal sealed class CeruleanQuitException : Exception
    {
        public CeruleanQuitException() : base("Cerulean Quit")
        {
        }
    }
    public sealed class CeruleanAPI
    {
        #region Private
        private static readonly CeruleanAPI _instance = new();

        private const int MAX_WORKITEMS = 5;

        private readonly Thread _thread;
        private readonly ConcurrentDictionary<uint, Window> _windows;
        private readonly ConcurrentQueue<WorkItem> _workItems;
        private readonly EmbeddedLayouts _embeddedLayouts;
        private readonly Profiler _profiler;

        private ILoggingService? _logger;
        private IGraphicsFactory? _graphicsFactory;
        private bool _initialized = false;
        private bool _running = false, _stopped = false, _quitting = false;
        private int _threadId;
        #endregion

        public IEnumerable<Window> Windows { get => _windows.Values; }
        public Profiler Profiler => _profiler;

        private CeruleanAPI()
        {
            _thread = new(new ThreadStart(WorkerThread));
            _windows = new();
            _workItems = new();
            _embeddedLayouts = new();
            _profiler = new();
            _profiler.OnLog += (s, e) =>
            {
                lock (_profiler)
                {
                    File.AppendAllText("profiler.txt", $"[{e.CallStack}] {e.Action}\n");
                }
                //Log($"[{e.CallStack}] {e.Action}");
            };
        }

        private void EnsureInitialized()
        {
            int timer = 0, max = 10;
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

        private void WorkerThread()
        {
            _profiler.StartProfilingPoint("WorkerThread");
            _logger?.Log("CeruleanAPI thread started...");
            _threadId = Environment.CurrentManagedThreadId;

            // Initialize SDL2
            SDL_SetHint("SDL_HINT_VIDEO_HIGHDPI_ENABLED", "1");
            if (SDL_InitSubSystem(SDL_INIT_EVERYTHING) != 0)
                throw new FatalAPIException("Could not initialize SDL2.");
            SDL_VERSION(out SDL_version version);
            _logger?.Log($"Running on SDL {version.major}.{version.minor}.{version.patch}.");
            _initialized = true;
            _logger?.Log("Initialized SDL2.");

            // Start event loop
            try
            {
                while (_running)
                {
                    _profiler.StartProfilingPoint("WorkQueue_Offload");
                    int offloaded = 0;
                    while (!_workItems.IsEmpty && offloaded < MAX_WORKITEMS)
                    {
                        if (_workItems.TryDequeue(out var workItem))
                        {
                            workItem.BeginTask();
                        }
                        offloaded++;
                    }
                    _profiler.EndProfilingCurrentPoint();
                    _profiler.StartProfilingPoint("Handle_SDLEvents");
                    while (SDL_PollEvent(out SDL_Event sdlEvent) != 0)
                    {
                        switch (sdlEvent.type)
                        {
                            case SDL_EventType.SDL_WINDOWEVENT:
                                if (_windows.TryGetValue(sdlEvent.window.windowID, out Window? window))
                                {
                                    switch (sdlEvent.window.windowEvent)
                                    {
                                        case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                                            window._closeFromEvent = true;
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
                                break;
                        }
                    }
                    _profiler.EndProfilingCurrentPoint();
                    foreach (var pair in _windows)
                    {
                        _profiler.StartProfilingPoint($"Window_{pair.Key}");
                        var window = pair.Value;
                        var clientArea = window.WindowSize;
                        window.GraphicsContext?.Update();
                        window.GraphicsContext?.SetRenderArea(clientArea, 0, 0);
                        window.Layout.Update(window, clientArea);
                        window.Draw();
                        _profiler.EndProfilingCurrentPoint();
                    }
                }
            }
            catch (CeruleanQuitException)
            {
                _logger?.Log("CeruleanAPI thread stopped (called by event on main thread).");
            }
            _profiler.EndProfilingCurrentPoint();
            _stopped = true;
        }

        public static CeruleanAPI GetAPI()
        {
            return _instance;
        }

        public CeruleanAPI Initialize()
        {
            if (!_initialized)
            {
                _logger?.Log("Initializing CeruleanAPI...");
                _running = true;
                _stopped = false;
                _logger?.Log("Loading embedded layouts...");
                _embeddedLayouts.RetrieveLayouts();
                _thread.Start();
            }
            return this;
        }

        public void Quit()
        {
            if (_initialized && !_quitting)
            {
                _quitting = true;
                _logger?.Log("Quitting CeruleanAPI...");
                ConcurrentDictionary<uint, Window> copy = new(_windows);
                _logger?.Log("Closing open windows...");
                foreach (var pair in copy)
                {
                    CloseWindow(pair.Value);
                }
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
        }

        public void WaitForAllWindowsClosed(bool quitOnComplete = false)
        {
            if (_initialized)
            {
                while (Windows.Any())
                {
                    Thread.Sleep(100);
                }
                if (quitOnComplete)
                    Quit();
            }
        }

        public CeruleanAPI UseLogger(ILoggingService loggingService)
        {
            _logger = loggingService;
            _embeddedLayouts.SetLogger(loggingService);
            //_logger.Init();
            return this;
        }

        public CeruleanAPI UseConsoleLogger()
            => UseLogger(new ConsoleLoggingService());

        public CeruleanAPI UseGraphicsFactory(IGraphicsFactory graphicsFactory)
        {
            _graphicsFactory = graphicsFactory;
            return this;
        }

        public CeruleanAPI UseSDL2Graphics()
            => UseGraphicsFactory(new SDL2GraphicsFactory());

        public Window CreateWindow(Layout windowLayout, string windowTitle = "CeruleanAPI Window", Size? windowSize = null, bool initialize = true)
        {
            EnsureInitialized();
            if (_graphicsFactory is null)
            {
                var exception = new FatalAPIException("No graphics factory specified.");
                _logger?.Log(exception.Message, LogSeverity.Fatal, exception);
                throw exception;
            }
            object? result = DoOnThread(new Func<Window>(() =>
            {
                Window window = new(windowLayout, windowTitle, windowSize ?? Window.DefaultWindowSize, _threadId, _graphicsFactory);
                if (initialize)
                    InitializeWindow(window);
                return window;
            }));

            if (result is not Window)
            {
                var exception = new GeneralAPIException("Window could not be created.");
                _logger?.Log(exception.Message, LogSeverity.Error, exception);
                throw exception;
            }
            return (Window)result;
        }

        public Window CreateWindow(string windowLayoutName, string windowTitle = "CeruleanAPI Window", Size? windowSize = null, bool initialize = true)
        {
            return CreateWindow(FetchLayout(windowLayoutName), windowTitle, windowSize, initialize);
        }

        public void CloseWindow(Window window)
        {
            EnsureInitialized();
            DoOnThread(() =>
            {
                if (!window.Closed)
                {
                    if (!window._closeFromEvent)
                    {
                        window._closeFromEvent = true;
                        window.InvokeOnClose();
                    }
                    else
                    {
                        uint windowID = SDL_GetWindowID(window.WindowPtr);
                        window.InternalClose();
                        _windows.Remove(windowID, out _);
                    }
                }
            });
        }

        public void InitializeWindow(Window window)
        {
            DoOnThread(() =>
            {
                if (!window.IsInitialized)
                {
                    window.Initialize();
                    _windows.TryAdd(SDL_GetWindowID(window.WindowPtr), window);
                }
            });
        }

        public Layout FetchLayout(string name)
        {
            EnsureInitialized();
            return _embeddedLayouts.FetchLayout(name);
        }

        public object? DoOnThread(Func<object> func)
        {
            EnsureInitialized();
            WorkItem work = new(func);

            // if thread is the CeruleanAPI thread
            if (Environment.CurrentManagedThreadId == _threadId)
            {
                work.BeginTask();
            }
            else
            {
                // thread is NOT the CeruleanAPI thread
                while (_workItems.Count >= MAX_WORKITEMS)
                    Task.Delay(200).Wait();
                _workItems.Enqueue(work);
            }

            return work.WaitForCompletion();
        }

        public void DoOnThread(Action action)
        {
            EnsureInitialized();
            WorkItem work = new(action);

            // if thread is the CeruleanAPI thread
            if (Environment.CurrentManagedThreadId == _threadId)
            {
                work.BeginTask();
            }
            else
            {
                // thread is NOT the CeruleanAPI thread
                while (_workItems.Count >= MAX_WORKITEMS)
                    Task.Delay(200).Wait();
                _workItems.Enqueue(work);
            }

            work.WaitForCompletion();
        }
        public void Log(string message, LogSeverity severity = LogSeverity.General)
        {
            _logger?.Log(message, severity);
        }

        public void Log(string message, LogSeverity severity, Exception exception)
        {
            _logger?.Log(message, severity, exception);
        }
    }
}