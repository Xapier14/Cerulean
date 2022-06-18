using Cerulean.Common;
using System.Collections.Concurrent;
using static SDL2.SDL;

namespace Cerulean.Core
{
    public sealed class CeruleanAPI
    {
        #region Private
        private static readonly CeruleanAPI _instance = new();

        private const int MAX_WORKITEMS = 5;

        private readonly Thread _thread;
        private readonly ConcurrentDictionary<uint, Window> _windows;
        private readonly ConcurrentQueue<WorkItem> _workItems;
        private readonly EmbeddedLayouts _embeddedLayouts;

        private ILoggingService? _logger;
        private IGraphicsFactory? _graphicsFactory;
        private bool _initialized = false;
        private bool _running = false, _stopped = false;
        private int _threadId;
        #endregion

        public IEnumerable<Window> Windows { get => _windows.Values; }

        private CeruleanAPI()
        {
            _thread = new(new ThreadStart(WorkerThread));
            _windows = new();
            _workItems = new();
            _embeddedLayouts = new();
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
                    throw new InvalidOperationException("The CeruleanAPI instance needs to be initialized first.");
            }
        }

        private void WorkerThread()
        {
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
            while (_running)
            {
                int offloaded = 0;
                while (!_workItems.IsEmpty && offloaded < MAX_WORKITEMS)
                {
                    if (_workItems.TryDequeue(out var workItem))
                    {
                        workItem.BeginTask();
                    }
                    offloaded++;
                }
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
                foreach (var pair in _windows)
                {
                    var window = pair.Value;
                    var clientArea = window.WindowSize;
                    window.GraphicsContext?.Update();
                    window.GraphicsContext?.SetRenderArea(clientArea, 0, 0);
                    window.Layout.Update(window, clientArea);
                    window.Draw();
                }
            }
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
            if (_initialized)
            {
                _logger?.Log("Quitting CeruleanAPI...");
                ConcurrentDictionary<uint, Window> copy = new(_windows);
                _logger?.Log("Closing open windows...");
                foreach (var pair in copy)
                {
                    CloseWindow(pair.Value);
                }
                _logger?.Log("Stopping thread...");
                _running = false;
                while (!_stopped)
                {
                    Thread.Sleep(500);
                }
            }
        }

        public void WaitAllWindows(bool quitOnComplete = false)
        {
            while (Windows.Any())
            {
                Thread.Sleep(100);
            }
            if (quitOnComplete)
                Quit();
        }

        public CeruleanAPI UseLogger(ILoggingService loggingService)
        {
            _logger = loggingService;
            _embeddedLayouts.SetLogger(loggingService);
            //_logger.Init();
            return this;
        }

        public CeruleanAPI UseGraphicsFactory(IGraphicsFactory graphicsFactory)
        {
            _graphicsFactory = graphicsFactory;
            return this;
        }

        public Window CreateWindow(Layout windowLayout, string windowTitle = "CeruleanAPI Window", Size? windowSize = null, bool initialize = true)
        {
            EnsureInitialized();
            if (_graphicsFactory is null)
            {
                throw new FatalAPIException("No graphics factory specified.");
            }
            object? result = DoOnThread(new Func<Window>(() =>
            {
                Window window = new(windowLayout, windowTitle, windowSize ?? Window.DefaultWindowSize, _threadId, _graphicsFactory);
                if (initialize)
                    InitializeWindow(window);
                return window;
            }));

            if (result is not Window)
                throw new GeneralAPIException("Window could not be created.");
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
    }
}