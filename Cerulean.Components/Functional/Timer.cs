using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool IsRunning { get { return _running; } }
        public int Interval { get; set; }
        public EventHandler<TimerEventArgs>? OnElapse;

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

        public override void Update(Size clientArea)
        {
            base.Update(clientArea);
            if (!_running)
                return;

            var deltaTicks = DateTime.Now.Ticks - _time;
            if (deltaTicks >= TimeSpan.FromMilliseconds(Interval).Ticks)
            {
                _time = DateTime.Now.Ticks;
                OnElapse?.Invoke(this, new()
                {
                    Interval = Interval,
                    InBetween = TimeSpan.FromTicks(deltaTicks)
                });
            }
        }
    }
}
