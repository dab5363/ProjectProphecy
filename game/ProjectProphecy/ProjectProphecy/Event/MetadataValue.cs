using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Event
{
    /// <summary>
    /// Data of data. Each metadata holds a list of values, like in Minecraft
    /// </summary>
    public class MetadataValue
    {
        // --- Fields ---
        private object value;

        // --- Constructors ---
        public MetadataValue(object value)
        {
            this.value = value;
        }

        // --- Methods ---
        /// <summary>
        /// Converts the stored object as an Int.
        /// In most cases used when you know the type of it.
        /// </summary>
        /// <returns></returns>
        public int AsInt()
        {
            if (value is int)
            {
                return (int)value;
            }
            return default;
        }

        /// <summary>
        /// Converts the stored object as a float.
        /// In most cases used when you know the type of it.
        /// </summary>
        /// <returns></returns>
        public float AsFloat()
        {
            if (value is float)
            {
                return (float)value;
            }
            return default;
        }

        /// <summary>
        /// Converts the stored object as a double.
        /// In most cases used when you know the type of it.
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            if (value is double)
            {
                return (double)value;
            }
            return default;
        }

        /// <summary>
        /// Converts the stored object as a long.
        /// In most cases used when you know the type of it.
        /// </summary>
        /// <returns></returns>
        public double AsLong()
        {
            if (value is long)
            {
                return (long)value;
            }
            return default;
        }

        /// <summary>
        /// Converts the stored object as a short.
        /// In most cases used when you know the type of it.
        /// </summary>
        /// <returns></returns>
        public double AsShort()
        {
            if (value is short)
            {
                return (short)value;
            }
            return default;
        }

        /// <summary>
        /// Converts the stored object as a byte.
        /// In most cases used when you know the type of it.
        /// </summary>
        /// <returns></returns>
        public double AsByte()
        {
            if (value is byte)
            {
                return (byte)value;
            }
            return default;
        }

        /// <summary>
        /// Converts the stored object as a bool.
        /// In most cases used when you know the type of it.
        /// </summary>
        /// <returns></returns>
        public bool AsBool()
        {
            if (value is bool)
            {
                return (bool)value;
            }
            return default;
        }

        /// <summary>
        /// Converts the stored object as a string.
        /// In most cases used when you know the type of it.
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            if (value is string)
            {
                return (string)value;
            }
            return default;
        }
    }
}
