namespace Cerulean.Common.Collections
{
    public class LinkedListNode<T>
    {
        public LinkedListNode<T>? Previous { get; set; } = null;
        public LinkedListNode<T>? Next { get; set; } = null;
        private T? _data;
        public T Data
        {
            get
            {
                if (_data is T data)
                    return data;
                throw new GeneralAPIException("Node data is null.");
            }
        }
        public object? Metadata { get; set; }

        private LinkedListNode()
        {

        }

        public LinkedListNode(T data)
        {
            _data = data;
        }

        public static LinkedListNode<T> Null => new LinkedListNode<T>();

        internal ref T? GetRefData()
        {
            return ref _data;
        }
    }
}
