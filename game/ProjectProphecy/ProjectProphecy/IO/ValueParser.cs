using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_IO
{
    /// <summary>
    /// Parses values. If meets an exception, return default value of the class.
    /// </summary>
    class ValueParser
    {
        public static int ParseInt(string value)
        {
            try
            {
                return int.Parse(value);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static double ParseDouble(string value)
        {
            try
            {
                return double.Parse(value);
            }
            catch (Exception)
            {
                return 0.0D;
            }
        }

        public static bool ParseBool(string value)
        {
            try
            {
                return bool.Parse(value);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
