using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace FreeTools.Helpers
{
    public static class SettingsHelper
    {
        private static readonly ISettings _localSettings = CrossSettings.Current;

        public static void AddOrUpdateValue<T>(string key, T value)
        {
            _localSettings.AddOrUpdateValue(key, value);

            _localSettings.AddOrUpdateValue("timestamp", DateTime.Now.Ticks.ToString());
        }

        public static T GetValueOrDefault<T>(string key, T defaultValue) => _localSettings.GetValueOrDefault(key, defaultValue);

        public static void UpdateLocalSettings()
        {
            var localTimestamp = _localSettings.GetValueOrDefault("timestamp", 0.ToString());
            var roamingTimestamp = ApplicationData.Current.RoamingSettings.Values.ContainsKey("timestamp") 
                ? (string)ApplicationData.Current.RoamingSettings.Values["timestamp"]
                : 0.ToString();
            
            if (ulong.Parse(roamingTimestamp) <= ulong.Parse(localTimestamp)) return;

            _localSettings.AddOrUpdateAllValues(ApplicationData.Current.RoamingSettings.Values);
        }

        public static void UpdateRoamingSettings()
        {
            ApplicationData.Current.RoamingSettings.Values.Clear();

            ApplicationData.Current.RoamingSettings.Values.AddRange(ApplicationData.Current.LocalSettings.Values);

            ApplicationData.Current.SignalDataChanged();
        }
    }
}
