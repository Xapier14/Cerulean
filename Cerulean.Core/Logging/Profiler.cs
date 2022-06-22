using System.Text;

namespace Cerulean.Core.Logging
{
    public sealed class ProfilerEventArgs : EventArgs
    {
        public string CallStack { get; set; } = "";
        public string Action { get; set; } = "";
    }
    public sealed class Profiler
    {
        public struct ProfilerItem
        {
            public string Label;
            public long StartTicks;
        }

        private readonly Stack<ProfilerItem> _callStack = new();

        public EventHandler<ProfilerEventArgs>? OnLog;

        private string CurrentCallStack
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(_callStack.Last().Label);
                for (int i = _callStack.Count - 2; i >= 0; --i)
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
                Action = "Invoked."
            });
        }

        public void EndProfilingCurrentPoint()
        {
            string callStack = CurrentCallStack;
            ProfilerItem item = _callStack.Pop();
            long ticks = DateTime.Now.Ticks - item.StartTicks;
            OnLog?.Invoke(this, new()
            {
                CallStack = callStack,
                Action = $"Finished after {TimeSpan.FromTicks(ticks).TotalMilliseconds}ms."
            });
        }
    }
}
