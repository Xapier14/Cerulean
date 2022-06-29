
using Cerulean.Common;

namespace Cerulean.Components
{
    public class TimerEventArgs : EventArgs
    {
        public int Interval { get; init; }
        public TimeSpan InBetween { get; init; }
    }

    public sealed class Timer : Component
    {
        private long _time;
        private bool _running;
        public bool IsRunning => _running;
        public int Interval { get; set; }
        public delegate void TimerEventHandler(Timer sender,
                                               object? window,
                                               TimerEventArgs e);
        public TimerEventHandler? OnElapse;

        public Timer(int interval = 1000)
        {
            CanBeParent = false;
            Interval = interval;
        }

        public void Start()
        {
            if (!_running)
            {
                _running = true;
                _time = DateTime.Now.Ticks;
            }
        }

        public void Stop()
        {
            if (_running)
            {
                _running = false;
            }
        }

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = clientArea;
            if (!_running)
                return;

            var deltaTicks = DateTime.Now.Ticks - _time;
            if (deltaTicks < TimeSpan.FromMilliseconds(Interval).Ticks) return;
            _time = DateTime.Now.Ticks;
            OnElapse?.Invoke(this, window, new TimerEventArgs
            {
                Interval = Interval,
                InBetween = TimeSpan.FromTicks(deltaTicks)
            });
        }
    }
}
