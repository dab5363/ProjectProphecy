using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace ProjectProphecy.ns_Utility
{
    /// <summary>
    /// Using FileIO to log messages during a single game run.
    /// </summary>
    class Logger
    {
        public enum DebugLevel
        {
            Error = 0,
            Major = 10,
            FileIO = 20,
            Game = 30,
            Detail = 40,
        }

        // --- Fields ---
        // TODO: Settings class and config file for storing debug level and other settings
        private static int debugLevel = 100;                     /* Current debug level. Determines if the message should be logged.
                                                                    The higher debug level, the more details. */
        private static long access = 0;                          // Num# of the log. Starts from 1; 0 means no message has been logged.
        private readonly static string logFilePath = @"log.yml"; // Path of the log file

        // --- Methods ---
        /// <summary>
        /// Logs the given message to both a log file and the debug console.
        /// Always logs the message no matter the debug level.
        /// </summary>
        /// <param name="message"> Message to log </param>
        public static void Log(object message)
        {
            Log(message, int.MinValue, false, false);
        }

        /// <summary>
        /// Logs the given message to both a log file and the debug console.
        /// To log the message, the current debug level must be no less than the message debug level.
        /// </summary>
        /// <param name="message"> Message to log </param>
        /// <param name="level"> Debug level of the message </param>
        public static void Log(object message, int level)
        {
            Log(message, level, false);
        }
        public static void Log(object message, DebugLevel level)
        {
            Log(message, (int)level, false);
        }

        /// <summary>
        /// Logs the given message to both a log file and the debug console.
        /// To log the message, the current debug level must be no less than the message debug level.
        /// When isOnlyCurrent is set to true, the current debug level must equal the message debug level.
        /// </summary>
        /// <param name="message"> Message to log </param>
        /// <param name="level"> Debug level of the message </param>
        /// <param name="isOnlyCurrent"> Whether the current debug level must equal the message debug level</param>
        public static void Log(object message, int level, bool isOnlyCurrent)
        {
            Log(message, level, isOnlyCurrent, true);
        }
        public static void Log(object message, DebugLevel level, bool isOnlyCurrent)
        {
            Log(message, (int)level, isOnlyCurrent, true);
        }

        /// <summary>
        /// Logs the given message to both a log file and the debug console.
        /// To log the message, the current debug level must be no less than the message debug level.
        /// When isOnlyCurrent is set to true, the current debug level must equal the message debug level.
        /// </summary>
        /// <param name="message"> Message to log </param>
        /// <param name="level"> Debug level of the message </param>
        /// <param name="isOnlyCurrent"> Whether the current debug level must equal the message debug level</param>
        /// <param name="isLogLevel"> Whether or not to log the message's debug level along with it </param>
        public static void Log(object message, int level, bool isOnlyCurrent, bool isLogLevel)
        {
            // Depending on isOnlyCurrent:
            // False - Does log when current debug level is greater than or equal to the message debug level
            if (!isOnlyCurrent)
            {
                if (debugLevel < level) return;
            }
            // True - Logs the message ONLY when current debug level is equal to the message debug level
            else if (debugLevel != level) return;

            StreamWriter writer = null;
            try
            {
                FileStream fs = null;
                FileInfo fi = File.Exists(logFilePath) ? new FileInfo(logFilePath) : null;
                DateTime creationTime;
                // Recreates the log file if it's the first time to log any after the game launched.
                if (access == 0)
                {
                    // Saves backup - copies the original log file with a suffix of its creation time.
                    if (fi != null)
                    {
                        string backupPath = logFilePath.Insert(3, fi.CreationTime.ToString("yyyy-MM-ddTHH-mm-ssZ"));
                        File.Copy(logFilePath, backupPath);
                        // Updates its creation time (if log file exists - because overwriting or
                        // even File.Delete() + File.Create() does not affect the creation time).
                        File.SetCreationTime(logFilePath, DateTime.Now);
                        fi.Refresh();
                    }
                    // Creates/overwrites the log file.
                    fs = File.Create(logFilePath);
                }
                else
                {
                    // Writes to the end of the log file
                    fs = File.Open(logFilePath, FileMode.Append);
                }

                // Initializes creationTime. Either the existing one or the one just refreshed when access == 0.
                creationTime = fi != null ? fi.CreationTime : DateTime.Now;
                writer = new StreamWriter(fs);
                string currentTime = DateTime.Now.ToString("HH:mm:ss");
                double timeDifference = DateTime.Now.Subtract(creationTime).TotalMinutes;
                // Adds a prefix of the level as a Roman number to the message
                if (isLogLevel && level > 0)
                {
                    string roman = Romanize(level);
                    if (!string.IsNullOrEmpty(roman))
                    {
                        string levelPrefix = "*" + Romanize(level) + "*";
                        message = levelPrefix + " " + message;
                    }
                }
                writer.WriteLine($"[{++access}][{timeDifference:0}:{currentTime}] {message}");
                // ONLY IF the message is logged sucessfully will Debug Output show it,
                // in order to avoid repetitivity - error message in catch already includes that.
                Debug.WriteLine($"{message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error logging \"{message}\" - {ex.Message}\n{ex.StackTrace}");
            }
            if (writer != null)
            {
                writer.Close();
            }
        }
        public static void Log(object message, DebugLevel level, bool isOnlyCurrent, bool isLogLevel)
        {
            Log(message, (int)level, isOnlyCurrent, isLogLevel);
        }

        /// <summary>
        /// Converts a given number into the Roman form.
        /// When the give number is less than or equal to 0, returns an empty string, since Roman numbers
        /// don't have 0 or negatives, (though 0 works in the algorithm and also returns an empty string)
        /// </summary>
        /// <param name="num"></param>
        private static string Romanize(int num)
        {
            if (num <= 0) return "";
            Stack<string> digits = new Stack<string>(num.ToString().Split(""));
            string[] key = {
                "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM",
                "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC",
                "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
            string roman = "";
            for (int i = 3; i > 0; i--)
            {
                int index = int.Parse(digits.Pop()) + (i * 10);
                roman = key[index] + roman;
            }
            int thousands = int.Parse(string.Join("", digits));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < thousands; i++)
            {
                sb.Append("M");
            }
            sb.Append(roman);
            return sb.ToString();
        }
    }
}

