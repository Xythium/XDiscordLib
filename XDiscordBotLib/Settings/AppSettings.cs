using System;
using System.Configuration;
using Serilog;

namespace XDiscordBotLib.Settings
{
    public class AppSettings
    {
        protected string KeyPrefix { get; }

        public AppSettings(string prefix = null) { KeyPrefix = prefix; }

        public T GetValue<T>(string key, T def = default(T))
        {
            Log.Verbose($"Getting value for {key}, default {def}");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = config.AppSettings.Settings;
            string value;

            if (settings[toKeyPrefix(key)]
                ?.Value == null)
            {
                SetValue(key, def?.ToString() ?? "");
                value = def?.ToString() ?? "";
            }
            else
                value = settings[toKeyPrefix(key)]
                    .Value;

            return (T) Convert.ChangeType(value, typeof(T));
        }

        public void SetValue(string key, string value)
        {
            Log.Verbose($"Setting value for {key}, {value}");
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = config.AppSettings.Settings;
            if (settings[toKeyPrefix(key)]
                ?.Value == null)
                settings.Add(toKeyPrefix(key), value);
            else
                settings[toKeyPrefix(key)]
                    .Value = value;
            config.Save();
        }

        public void SetValue<T>(string key, T value) { SetValue(key, value.ToString()); }

        private string toKeyPrefix(string key) => $"{(KeyPrefix != null ? $"{KeyPrefix}:" : "")}{key}";

        public string BotToken { get { return GetValue<string>("botToken"); } set { SetValue<string>("botToken", value); } }
    }
}