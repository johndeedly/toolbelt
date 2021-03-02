using System;
using System.Collections.Generic;

namespace toolbelt
{
    public interface ISkipDictionary<TKey, T> : ISkipList<KeyItemPair<TKey, T>> where TKey : IComparable<TKey>
    {
        T this[TKey key] { get; set; }
        void Add(TKey key, T item);
        void CopyTo(T[] array, int arrayIndex);
        bool Contains(TKey key);
        T Get(TKey key);
        bool Remove(TKey key);
        bool TryGet(TKey key, out T item);
    }

    public class SkipDictionary<TKey, T> : SkipList<KeyItemPair<TKey, T>>, ISkipDictionary<TKey, T> where TKey : IComparable<TKey>
    {
        public T this[TKey key]
        {
            get => Get(key);
            set => Add(key, value);
        }

        public SkipDictionary(int height = 32) : base(height)
        { }

        public void Add(TKey key, T item)
        {
            KeyItemPair<TKey, T> kip = new KeyItemPair<TKey, T>(key, item);
            Add(kip);
        }

        public bool Contains(TKey key)
        {
            KeyItemPair<TKey, T> kip = new KeyItemPair<TKey, T>(key);
            return Contains(kip);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in this)
                array[arrayIndex++] = item.Item2;
        }

        public T Get(TKey key)
        {
            SkipNode<KeyItemPair<TKey, T>> iter = head;
            for (int i = height - 1; i >= 0; i--)
            {
                while (iter.Next[i] != null)
                {
                    int diff = key.CompareTo(iter.Next[i].Item.Item1);
                    if (diff == 0)
                        return iter.Next[i].Item.Item2;
                    if (diff < 0)
                        break;
                    iter = iter.Next[i];
                }
            }
            throw new KeyNotFoundException();
        }

        public bool Remove(TKey key)
        {
            KeyItemPair<TKey, T> kip = new KeyItemPair<TKey, T>(key);
            return Remove(kip);
        }

        public bool TryGet(TKey key, out T item)
        {
            SkipNode<KeyItemPair<TKey, T>> iter = head;
            for (int i = height - 1; i >= 0; i--)
            {
                while (iter.Next[i] != null)
                {
                    int diff = key.CompareTo(iter.Next[i].Item.Item1);
                    if (diff == 0)
                    {
                        item = iter.Next[i].Item.Item2;
                        return true;
                    }
                    if (diff < 0)
                        break;
                    iter = iter.Next[i];
                }
            }
            item = default(T);
            return false;
        }
    }
}