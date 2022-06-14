using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.Common.Collections
{
    /// <summary>
    /// A doubly linked list.
    /// </summary>
    /// <typeparam name="T">Generic datatype</typeparam>
    public class LinkedList<T> : IEnumerable<T>
    {
        private LinkedListNode<T>? _head, _tail;
        public LinkedListNode<T>? Head => _head;
        public LinkedListNode<T>? Tail => _tail;
        public int Count
        {
            get
            {
                int count = 0;
                foreach (var node in this)
                {
                    count++;
                }
                return count;
            }
        }
        public LinkedList()
        {

        }

        public void AddLast(T data)
        {
            if (_head == null)
            {
                _head = new(data);
                _tail = _head;
            } else if (_head == _tail)
            {
                _head.Next = new(data);
                _tail = _head.Next;
                _tail.Previous = _head;
            } else if (_tail is not null)
            {
                _tail.Next = new(data);
                _tail.Next.Previous = _tail;
                _tail = _tail.Next;
            }
        }
        public void SwitchNodes(LinkedListNode<T> node1, LinkedListNode<T> node2)
        {
            if (_head is null || _tail is null || node1 == node2)
                return;

            if (node1 == _head)
            {
                _head = node2;
            } else if (node2 == _head)
            {
                _head = node1;
            }

            if (node1 == _tail)
            {
                _tail = node2;
            } else if (node2 == _tail)
            {
                _tail = node1;
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

        public void Clear()
        {
            _head = null;
            _tail = null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new LinkedListEnumerator<T>(_head);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new LinkedListEnumerator<T>(_head);
        }
    }

    public class LinkedListEnumerator<T> : IEnumerator<T>
    {
        private bool _started;
        private LinkedListNode<T>? _head;
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
            if (_current.Next is LinkedListNode<T> nextNode)
            {
                _current = nextNode;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _current = LinkedListNode<T>.Null;
            _started = false;
        }
    }
}
