using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ProjectProphecy.ns_Event
{
    /// <summary>
    /// Stores metadata for a custom event. Each metadata holds a list of values, like in Minecraft
    /// </summary>
    public class EventMetadata : EventArgs
    {
        // --- Fields ---
        private readonly Dictionary<string, List<MetadataValue>> metadata = new Dictionary<string, List<MetadataValue>>();

        // --- Indexers ---
        public List<MetadataValue> this[string key]
        {
            // Returns null if no metadata was set with the key
            get => metadata[key];
        }

        // --- Methods ---
        /// <summary>
        /// Adds a new bit to the specified metadata
        /// </summary>
        /// <param name="key"> The key for the metadata</param>
        /// <param name="value"> New member of the metadata</param>
        public void SetMetadata(string key, MetadataValue value)
        {
            if (!HasMetadata(key))
            {
                metadata[key] = new List<MetadataValue>();
            }
            metadata[key].Add(value);
        }

        /// <summary>
        /// Checks whether or not this has a metadata set for the key.
        /// </summary>
        /// <param name="key"> The key for the metadata</param>
        /// <returns> Whether or not there is metadata set for the key </returns>
        public bool HasMetadata(string key)
        {
            return metadata.ContainsKey(key);
        }

        /// <summary>
        /// Removes a metadata value from the list.
        /// </summary>
        /// <param name="key"> The key for the metadata </param>
        public void RemoveMetadata(string key)
        {
            metadata.Remove(key);
        }

    }
}
