using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private LinkedListNode()
        {

        }

        public LinkedListNode(T data)
        {
            _data = data;
        }

        public static LinkedListNode<T> Null => new LinkedListNode<T>();
    }
}
