using System.Text;

namespace Cerulean.Core.Logging
{
    public enum ProfilerEventType
    {
        Start,
        Stop
    }
    public sealed class ProfilerEventArgs : EventArgs
    {
        public string CallStack { get; init; } = "";
        public string Action { get; init; } = "";
        public ProfilerEventType EventType { get; init; }
        public double? ElapsedTime { get; init; }
    }
    public sealed class Profiler
    {
        public struct ProfilerItem
        {
            public string Label { get; init; }
            public long StartTicks { get; init; }
        }

        private readonly Stack<ProfilerItem> _callStack = new();

        public EventHandler<ProfilerEventArgs>? OnLog;

        private string CurrentCallStack
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(_callStack.Last().Label);
                for (var i = _callStack.Count - 2; i >= 0; --i)
                {
                    sb.Append($".{_callStack.ElementAt(i).Label}");
                }
                return sb.ToString();
            }
        }

        public void StartProfilingPoint(string label)
        {
            _callStack.Push(new()
            {
                Label = label,
                StartTicks = DateTime.Now.Ticks
            });
            OnLog?.Invoke(this, new()
            {
                CallStack = CurrentCallStack,
                Action = "Invoked.",
                EventType = ProfilerEventType.Start
            });
        }

        public void EndProfilingCurrentPoint()
        {
            var callStack = CurrentCallStack;
            var item = _callStack.Pop();
            var ticks = DateTime.Now.Ticks - item.StartTicks;
            var elapsed = TimeSpan.FromTicks(ticks).TotalMilliseconds;
            OnLog?.Invoke(this, new ProfilerEventArgs
            {
                CallStack = callStack,
                Action = $"Finished after {elapsed}ms.",
                EventType = ProfilerEventType.Start,
                ElapsedTime = elapsed
            });
        }
    }
}
