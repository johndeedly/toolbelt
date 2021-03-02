using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace toolbelt
{
    public interface ISkipList<T> : ICollection<T> where T : IComparable<T>
    {
        T this[int index] { get; }
    }

    public class SkipList<T> : ISkipList<T> where T : IComparable<T>
    {
        protected readonly Random coinflip = new ProportionalRandom();
        protected SkipNode<T> head;
        protected readonly int height;

        public int Count => this.Count();

        public bool IsReadOnly => false;

        public T this[int index] => this.Skip(index).FirstOrDefault();

        public SkipList(int height = 32)
        {
            this.height = height;
            this.head = new SkipNode<T>(height);

            Clear();
        }

        public void Add(T item)
        {
            int lvl = coinflip.Next(height);
            SkipNode<T> add = new SkipNode<T>(lvl + 1, item);
            SkipNode<T> iter = head;
            for (int i = height - 1; i >= 0; i--)
            {
                while (iter.Next[i] != null)
                {
                    int diff = item.CompareTo(iter.Next[i].Item);
                    if (diff == 0)
                    {
                        iter.Next[i] = iter.Next[i].Next[i];
                        continue;
                    }
                    if (diff < 0)
                        break;
                    iter = iter.Next[i];
                }
                if (i <= lvl)
                {
                    add.Next[i] = iter.Next[i];
                    iter.Next[i] = add;
                }
            }
        }

        public void Clear()
        {
            for (int i = height - 1; i >= 0; i--)
                head.Next[i] = null;
        }

        public bool Contains(T item)
        {
            SkipNode<T> iter = head;
            for (int i = height - 1; i >= 0; i--)
            {
                while (iter.Next[i] != null)
                {
                    int diff = item.CompareTo(iter.Next[i].Item);
                    if (diff == 0)
                        return true;
                    if (diff < 0)
                        break;
                    iter = iter.Next[i];
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in this)
                array[arrayIndex++] = item;
        }

        public bool Remove(T item)
        {
            SkipNode<T> iter = head;
            bool found = false;
            for (int i = height - 1; i >= 0; i--)
            {
                while (iter.Next[i] != null)
                {
                    int diff = item.CompareTo(iter.Next[i].Item);
                    if (diff == 0)
                    {
                        found = true;
                        iter.Next[i] = iter.Next[i].Next[i];
                        continue;
                    }
                    if (diff < 0)
                        break;
                    iter = iter.Next[i];
                }
            }
            return found;
        }

        public IEnumerator<T> GetEnumerator()
        {
            SkipNode<T> iter = head;
            while (iter.Next[0] != null)
            {
                yield return iter.Next[0].Item;
                iter = iter.Next[0];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}