using System;

namespace toolbelt
{
    public class KeyItemPair<TKey, T> : Tuple<TKey, T>, IComparable<KeyItemPair<TKey, T>> where TKey : IComparable<TKey>
    {
        public KeyItemPair(TKey item1 = default(TKey), T item2 = default(T)) : base(item1, item2)
        { }

        public int CompareTo(KeyItemPair<TKey, T> other)
        {
            return Item1.CompareTo(other.Item1);
        }
    }
}