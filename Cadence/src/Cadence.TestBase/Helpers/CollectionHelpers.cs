namespace Drachma.Base.Tests.Helpers;

public static class CollectionHelpers
{
    public static bool IsSorted<T, TKey>(IEnumerable<T> collection, Func<T, TKey> keySelector) where TKey : IComparable<TKey>
    {
        using (var enumerator = collection.GetEnumerator())
        {
            if (!enumerator.MoveNext())
            {
                return true;
            }

            var current = keySelector(enumerator.Current);
            while (enumerator.MoveNext())
            {
                var next = keySelector(enumerator.Current);
                if (current.CompareTo(next) > 0)
                {
                    return false;
                }

                current = next;
            }
        }

        return true;
    }

}
