
using Cerulean.Common;

namespace Cerulean.Components
{
    /// <summary>
    /// Cerulean Timer Event Args
    /// </summary>
    public class TimerEventArgs : EventArgs
    {
        public int Interval { get; init; }
        public TimeSpan InBetween { get; init; }
    }

    /// <summary>
    /// Cerulean Timer Component
    /// </summary>
    public sealed class Timer : Component
    {
        private long _time;

        /// <summary>
        /// Running status of the timer.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Interval in milliseconds between each tick.
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// General Timer Event Handler
        /// </summary>
        /// <param name="sender">The timer that fired the event.</param>
        /// <param name="window">The window it originated from.</param>
        /// <param name="e">The event args from the fired event.</param>
        public delegate void TimerEventHandler(Timer sender,
            object? window,
            TimerEventArgs e);

        /// <summary>
        /// Fired when the elapsed time is greater than the interval.
        /// </summary>
        public TimerEventHandler? OnElapse;

        /// <summary>
        /// Creates a stopped timer instance.
        /// </summary>
        /// <param name="interval">The timer's interval in milliseconds between ticks.</param>
        public Timer(int interval = 1000)
        {
            CanBeParent = false;
            Interval = interval;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            _time = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
            }
        }

        /// <summary>
        /// The timer's update step.
        /// </summary>
        /// <param name="window">The window that performed the update.</param>
        /// <param name="clientArea">The client area given to the component.</param>
        public override void Update(object? window, Size clientArea)
        {
            ClientArea = clientArea;
            if (!IsRunning)
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