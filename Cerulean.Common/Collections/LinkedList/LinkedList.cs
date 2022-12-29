using System.Collections;

namespace Cerulean.Common.Collections
{
    /// <summary>
    /// A doubly linked list.
    /// </summary>
    /// <typeparam name="T">Generic node data type</typeparam>
    public class LinkedList<T> : IEnumerable<T>
    {
        /// <summary>
        /// The head or the first node in the list.
        /// </summary>
        public LinkedListNode<T>? Head { get; private set; }

        /// <summary>
        /// The tail or the last node in the list.
        /// </summary>
        public LinkedListNode<T>? Tail { get; private set; }

        /// <summary>
        /// Returns the amount of nodes in the list.
        /// </summary>
        public int Count => this.Count();

        /// <summary>
        /// Adds data as a new node to the end of the list.
        /// </summary>
        /// <param name="data"></param>
        public void AddLast(T data)
        {
            if (Head == null)
            {
                Head = new LinkedListNode<T>(data);
                Tail = Head;
            }
            else if (Head == Tail)
            {
                Head.Next = new LinkedListNode<T>(data);
                Tail = Head.Next;
                Tail.Previous = Head;
            }
            else if (Tail is not null)
            {
                Tail.Next = new LinkedListNode<T>(data);
                Tail.Next.Previous = Tail;
                Tail = Tail.Next;
            }
        }

        /// <summary>
        /// Deletes a specific node from the list.
        /// </summary>
        /// <param name="node">The node to delete.</param>
        public void DeleteNode(LinkedListNode<T> node)
        {
            var prev = node.Previous;
            var next = node.Next;
            if (prev is not null)
                prev.Next = next;
            if (next is not null)
                next.Previous = prev;
            if (Tail == node)
                Tail = prev;
            if (Head == node)
                Head = next;
        }

        /// <summary>
        /// Deletes a node from the list using an index.
        /// </summary>
        /// <param name="index">The index of the node to delete.</param>
        public void DeleteNode(int index)
        {
            if (Head is null)
                return;
            var node = Head;
            for (var i = 1; i <= index; ++i)
                if (node.Next is not null)
                    node = node.Next;
            DeleteNode(node);
        }

        /// <summary>
        /// Swaps the positions of two nodes.
        /// </summary>
        /// <param name="node1">The first node to swap.</param>
        /// <param name="node2">The second node to swap with.</param>
        public void SwitchNodes(LinkedListNode<T> node1, LinkedListNode<T> node2)
        {
            if (Head is null || Tail is null || node1 == node2)
                return;

            if (node1 == Head)
            {
                Head = node2;
            }
            else if (node2 == Head)
            {
                Head = node1;
            }

            if (node1 == Tail)
            {
                Tail = node2;
            }
            else if (node2 == Tail)
            {
                Tail = node1;
            }

            var temp = node1.Next;
            node1.Next = node2.Next;
            node2.Next = temp;

            if (node1.Next is not null)
                node1.Next.Previous = node1;
            if (node2.Next is not null)
                node2.Next.Previous = node2;

            temp = node1.Previous;
            node1.Previous = node2.Previous;
            node2.Previous = temp;

            if (node1.Previous is not null)
                node1.Previous.Next = node1;
            if (node2.Previous is not null)
                node2.Previous.Next = node2;
        }

        /// <summary>
        /// Removes all nodes from the list.
        /// </summary>
        public void Clear()
        {
            Head = null;
            Tail = null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new LinkedListEnumerator<T>(Head);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new LinkedListEnumerator<T>(Head);
        }

        public ref T? this[int index]
        {
            get
            {
                if (index < 0 || index >= Count || Head is null)
                    throw new IndexOutOfRangeException();
                var node = Head;
                for (var i = 0; i < index; ++i)
                    if (node.Next is not null)
                        node = node.Next;
                ref var data = ref node.GetRefData();
                if (node.Data is null)
                    throw new NullReferenceException();
                return ref data;
            }
        }
    }

    public class LinkedListEnumerator<T> : IEnumerator<T>
    {
        private bool _started;
        private readonly LinkedListNode<T>? _head;
        private LinkedListNode<T> _current;
        public T Current => _current.Data;

        object IEnumerator.Current => _current;

        public LinkedListEnumerator(LinkedListNode<T>? headNode = null)
        {
            _head = headNode;
            _current = LinkedListNode<T>.Null;
            _started = false;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public bool MoveNext()
        {
            if (!_started && _head is not null)
            {
                _current = _head;
                _started = true;
                return true;
            }
            if (_current.Next is null)
                return false;
            _current = _current.Next;
            return true;
        }

        public void Reset()
        {
            _current = LinkedListNode<T>.Null;
            _started = false;
        }
    }
}
