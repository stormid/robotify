using System;
using System.Configuration;

namespace Robotify.Config
{
    internal static class ConfigurationManagerUtils
    {
        public static string FromAppSettings(this string key)
        {
            return GetAppSetting<string>(key, null, null);
        }

        public static string FromConnectionStrings(this string key)
        {
            return GetConnectionString(key, null, null);
        }

        public static T FromAppSettingsWithPrefix<T>(this string key, string prefix, T @default)
        {
            return GetAppSetting(key, @default, prefix);
        }

        public static T FromAppSettingsWithPrefix<T>(this string key, string prefix)
        {
            return GetAppSetting(key, default(T), prefix);
        }

        public static string FromAppSettingsWithPrefix(this string key, string prefix, string @default = null)
        {
            return GetAppSetting(key, @default, prefix);
        }

        public static T FromAppSettings<T>(this string key, T @default)
        {
            return GetAppSetting(key, @default, null);
        }

        public static string FromConnectionStrings(this string key, string @default)
        {
            return GetConnectionString(key, @default, null);
        }

        public static T FromAppSettings<T>(this string key, T @default, string prefix)
        {
            return GetAppSetting(key, @default, prefix);
        }

        public static string FromConnectionStrings(this string key, string @default, string prefix)
        {
            return GetConnectionString(key, @default, prefix);
        }

        public static string FromConnectionStringsWithPrefix(this string key, string prefix, string @default = null)
        {
            return GetConnectionString(key, @default, prefix);
        }
        
        public static T GetAppSetting<T>(string key, T @default = default(T), string prefix = "")
        {
            var keyToRead = string.IsNullOrWhiteSpace(prefix) ? key : $"{prefix}{key}";
            var value = ConfigurationManager.AppSettings.Get(keyToRead);
            if (string.IsNullOrWhiteSpace(value))
            {
                return @default;
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static string GetConnectionString(string key, string @default = null, string prefix = "")
        {
            var keyToRead = string.IsNullOrWhiteSpace(prefix) ? key : $"{prefix}{key}";
            var connectionStringValue = ConfigurationManager.ConnectionStrings[keyToRead];
            if (string.IsNullOrWhiteSpace(connectionStringValue?.ConnectionString))
            {
                if (@default == null) throw new ConfigurationErrorsException($"Unable to read file system configuration from connection strings section [{keyToRead}]");
                return @default;
            }
            return connectionStringValue.ConnectionString;
        }
    }
}