namespace Cerulean.Core
{
    internal class WorkItem
    {
        //private readonly Action? _action;
        private readonly Action<object[]>? _action;
        //private readonly Func<object>? _func;
        private readonly Func<object[], object>? _func;
        private readonly object[] _args;
        public object? Result { get; private set; }
        public bool IsCompleted { get; private set; }

        // public WorkItem(Action action)
        // {
        //     _action = action;
        //     _func = null;
        //     Result = null;
        //     _args = Array.Empty<object>();
        // }
        
        public WorkItem(Action<object[]> action, params object[] args)
        {
            _action = action;
            _func = null;
            Result = null;
            _args = args;
        }

        // public WorkItem(Func<object> func)
        // {
        //     _func = func;
        //     _action = null;
        //     Result = null;
        //     _args = Array.Empty<object>();
        // }

        public WorkItem(Func<object[], object> func, params object[] args)
        {
            _func = func;
            _action = null;
            Result = null;
            _args = args;
        }

        public void BeginTask()
        {
            if (IsCompleted) return;
            _action?.Invoke(_args);
            Result = _func?.Invoke(_args);
            IsCompleted = true;
        }

        public object? WaitForCompletion()
        {
            while (!IsCompleted)
                Task.Delay(200);
            return Result;
        }
    }
}