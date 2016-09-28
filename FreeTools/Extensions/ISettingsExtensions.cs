using System.Collections.Generic;

namespace Plugin.Settings.Abstractions
{
    public static class ISettingsExtensions
    {
        public static void AddOrUpdateAllValues<T>(this ISettings settings, IEnumerable<KeyValuePair<string, T>> otherCollection)
        {
            foreach (var keyValuePair in otherCollection)
            {
                settings.AddOrUpdateValue(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
