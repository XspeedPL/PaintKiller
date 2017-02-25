using System;
using System.Collections;
using System.Collections.Generic;

namespace PaintKilling.Mechanics.Content
{
    public class UnsafeList<T> : IReadOnlyCollection<T>
    {
        public Node First { get; private set; }
        public Node Last { get; private set; }

        public int Count { get; private set; }

        public virtual void Add(T value)
        {
            Last = new Node(value) { Prev = Last };
            if (Last.Prev != null) Last.Prev.Next = Last;
            if (First == null) First = Last;
            ++Count;
        }

        public Enumerator GetEnumerator() { return new Enumerator(this); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public virtual void Remove(Node node)
        {
            --Count;
            if (First == node) First = node.Next;
            if (Last == node) Last = node.Prev;
            if (node.Next != null) node.Next.Prev = node.Prev;
            if (node.Prev != null) node.Prev.Next = node.Next;
        }

        public sealed class Node
        {
            public readonly T Value;

            public Node Prev { get; internal set; }
            public Node Next { get; internal set; }

            public Node(T value) { Value = value; }
        }

        public sealed class Enumerator : IEnumerator<T>
        {
            private readonly UnsafeList<T> list;
            private Node node;

            public T Current { get { return node.Value; } }
            object IEnumerator.Current { get { return node.Value; } }

            public Enumerator(UnsafeList<T> source)
            {
                list = source;
                Reset();
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                node = node.Next;
                return node != null;
            }

            public void Remove()
            {
                list.Remove(node);
                node = node.Prev;
                if (node == null) Reset();
            }

            public void Reset()
            {
                node = new Node(default(T)) { Next = list.First };
            }
        }
    }
}
