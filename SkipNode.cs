using System;

namespace toolbelt
{
    public class SkipNode<T> where T : IComparable<T>
    {
        public SkipNode<T>[] Next;

        public T Item;

        public SkipNode(int height = 32, T item = default(T))
        {
            Next = new SkipNode<T>[height];
            Item = item;
        }
    }
}