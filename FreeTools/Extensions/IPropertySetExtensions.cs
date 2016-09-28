using System.Collections.Generic;

namespace Windows.Foundation.Collections
{
    public static class IPropertySetExtensions
    {
        public static void AddRange<T>(this IPropertySet set, IEnumerable<KeyValuePair<string, T>> otherCollection)
        {
            foreach (var keyValuePair in otherCollection)
            {
                set.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
