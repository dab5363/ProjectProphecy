using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using ProjectProphecy.ns_Utility;

namespace ProjectProphecy.ns_IO
{
    /// <summary>
    /// Represents a set of settings that store configurable data for an object.
    /// </summary>
    public class Settings
    {
        private static readonly string BASE = "-base";
        private static readonly string SCALE = "-scale";

        private readonly Dictionary<string, object> settings;

        public Settings()
        {
            settings = new Dictionary<string, object>();
        }

        public Settings(Settings settings)
        {
            this.settings = new Dictionary<string, object>(settings.settings);
        }

        public void Set(string key, object value)
        {
            settings[key] = value;
        }

        public void Set(string key, double Base, double scale)
        {
            settings[key + BASE] = Base;
            settings[key + SCALE] = scale;
        }

        public double GetDouble(string key)
        {
            return GetDouble(key, 0);
        }

        public double GetDouble(string key, double defaultValue)
        {
            if (settings.ContainsKey(key))
            {
                return ValueParser.ParseDouble(settings[key].ToString());
            }
            else
            {
                Set(key, defaultValue);
                return defaultValue;
            }
        }
        public int GetInt(string key)
        {
            return GetInt(key, 0);
        }

        public int GetInt(string key, int defaultValue)
        {
            if (settings.ContainsKey(key))
            {
                return ValueParser.ParseInt(settings[key].ToString());
            }
            else
            {
                Set(key, defaultValue);
                return defaultValue;
            }
        }

        public bool GetBool(string key)
        {
            return GetBool(key, false);
        }

        public bool GetBool(string key, bool defaultValue)
        {
            if (settings.ContainsKey(key))
            {
                return ValueParser.ParseBool(settings[key].ToString());
            }
            else
            {
                Set(key, defaultValue);
                return defaultValue;
            }
        }

        public string GetString(string key)
        {
            return GetString(key, null);
        }

        public string GetString(string key, string defaultValue)
        {
            if (settings.ContainsKey(key) && settings[key] != null)
            {
                return settings[key].ToString();
            }
            else
            {
                Set(key, defaultValue);
                return defaultValue;
            }
        }

        public IList<string> GetStringList(string key)
        {
            if (settings.ContainsKey(key))
            {
                object value = settings[key];
                Type type = value.GetType();
                if (type is IList<string>)
                {
                    return (IList<string>)value;
                }
                else
                {
                    return ImmutableList.Create(value.ToString());
                }
            }
            else
            {
                return new List<string>();
            }
        }

        public double GetAttr(string key, int level)
        {
            return GetAttr(key, level, 0);
        }

        public double GetAttr(string key, int level, double defaultValue)
        {
            if (!Has(key))
            {
                Set(key, defaultValue, 0);
                return defaultValue;
            }
            return GetBase(key) + GetScale(key) * (level - 1);
        }

        public double GetBase(string key)
        {
            if (!settings.ContainsKey(key + BASE))
            {
                return 0;
            }
            else
            {
                return ValueParser.ParseDouble(settings[key + BASE].ToString());
            }
        }

        public double GetScale(string key)
        {
            if (!settings.ContainsKey(key + SCALE))
            {
                return 0;
            }
            else
            {
                return ValueParser.ParseDouble(settings[key + SCALE].ToString());
            }
        }

        public object GetObj(string key, int level)
        {
            if (settings.ContainsKey(key))
            {
                return settings[key];
            }
            else if (settings.ContainsKey(key + BASE))
            {
                return GetAttr(key, level);
            }
            else
            {
                return 0;
            }
        }

        public bool Has(string key)
        {
            return settings.ContainsKey(key) || settings.ContainsKey(key + BASE);
        }

        /// <summary>
        /// Removes a setting.
        /// If the setting is not set, this will not do anything.
        /// </summary>
        /// <param name="key"> key name of the attribute </param>
        public void Remove(string key)
        {
            settings.Remove(key);
            settings.Remove(key + BASE);
            settings.Remove(key + SCALE);
        }

        public void Save(string path)
        {
            throw new NotImplementedException();
        }

        public void Load(string path)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Dumps the settings to the console for debugging purposes
        /// </summary>
        public void DumpToConsole()
        {
            Logger.Log("Settings:");
            foreach (string key in settings.Keys)
            {
                Logger.Log($"- {key}: {settings[key]}");
            }
        }
    }
}
