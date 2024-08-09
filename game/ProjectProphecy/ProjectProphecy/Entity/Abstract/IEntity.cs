using Microsoft.Xna.Framework;
using ProjectProphecy.Map;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Entity
{
    /// <summary>
    /// An entity that's located somewhere in the world.
    /// Not necessrily visible(drawable).
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Name of the object
        /// </summary>
        string Name
        {
            get; set;
        }

        /// <summary>
        /// Representation of the entity in the game world, combination of Location and Size
        /// And this representation may be seen as a "bounding box" used to check collision.
        /// </summary>
        Rectangle BoundingBox
        {
            get; set;
        }

        /// <summary>
        /// Where the entity is on screen
        /// </summary>
        Point Location
        {
            get; set;
        }

        /// <summary>
        /// How much space the entity takes up
        /// </summary>
        Point Size
        {
            get; set;
        }

        /// <summary>
        /// Which room the entity is located
        /// </summary>
        Room Room
        {
            get;set;
        }
    }
}
