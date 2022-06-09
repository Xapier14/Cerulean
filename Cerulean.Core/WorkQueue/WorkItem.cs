namespace Cerulean.Core
{
    internal class WorkItem
    {
        private Action? _action;
        private Func<object>? _func;
        public object? Result { get; private set; }
        public bool IsCompleted { get; private set; }

        public WorkItem(Action action)
        {
            _action = action;
            _func = null;
            Result = null;
        }

        public WorkItem(Func<object> func)
        {
            _func = func;
            _action = null;
            Result = null;
        }

        public void BeginTask(params object[] args)
        {
            if (!IsCompleted)
            {
                if (_action != null)
                    _action();
                if (_func != null)
                    Result = _func();
                IsCompleted = true;
            }
        }

        public object? WaitForCompletion()
        {
            while (!IsCompleted)
                Task.Delay(200);
            return Result;
        }
    }
}