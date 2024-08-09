using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace ProjectProphecy.Map
{
    /// <summary>
    /// 
    /// </summary>
    public class Tile : ns_Graphics.IDrawable, IEntity
    {
        // --- Fields ---
        private Rectangle boundingBox;
        private float rotation;
        private float scale;
        private Room room;
        private bool passable;

        // --- Properties ---
        public string Name
        {
            get;
            set;
        }
        = "Undefined";

        #region IEntity
        public Rectangle BoundingBox
        {
            get => boundingBox;
            set => boundingBox = value;
        }

        public Point Location
        {
            get => boundingBox.Location;
            set => boundingBox.Location = value;
        }

        public Point Size
        {
            get => boundingBox.Size;
            set => boundingBox.Size = value;
        }
        #endregion

        #region IDrawable
        public Texture2D Asset
        {
            get; set;
        }

        public Rectangle Section
        {
            // TODO: multiple tiles in a single sheet
            get => new Rectangle(0, 0, Size.X, Size.Y);
            set
            {
                // TODO: this setter has no use
            }
        }

        public float Rotation
        {
            get => rotation;
            set
            {
                MathHelper.ToRadians(rotation);
            }
        }

        public float Scale
        {
            get => scale;
            set
            {
                scale = value;
                boundingBox.Size = new Vector2(boundingBox.Width * value, boundingBox.Height * value).ToPoint();
            }
        }

        public bool IsVisible
        {
            get; set;
        } = true;

        #endregion

        #region Own
        public Point TileLoc
        {
            get; set;
        }

        public Room Room
        {
            get => room;
            set
            {
                if (room == null)
                {
                    room = value;
                }
            }
        }

        public bool Passable
        {
            get => passable;
        }
        #endregion

        /// <summary>
        /// Creates a tile using all specified properties
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tileLoc"></param>
        /// <param name="scale"></param>
        /// <param name="passable"></param>
        public Tile(
            string name, Point tileLoc, float scale = 2, bool passable = true)
        {
            Name = name;
            TileLoc = tileLoc;
            Asset = UIManager.Singleton.GetTexture(name);

            this.scale = scale;
            this.passable = passable;
            // Default values - size*scale= 64*64
            // TODO: Custom tile size in each room??
            Size = new Point(32, 32);
        }

        /// <summary>
        /// Creates a tile using its pre-defined data along with room-related properties. (loc, scale...)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="loc"></param>
        /// <param name="scale"></param>
        public Tile(TileData data, Point loc, float scale = 2)
        {
            // TileData-related properties set up
            Name = data.Name;
            Size = data.Size;
            passable = data.Passable;
            Asset = UIManager.Singleton.GetTexture(Name);
            // Other properties
            TileLoc = loc;
            this.scale = scale;
        }

        public virtual void Update()
        {
            // Updates euclidean coordinates based off the screen center.
            Location = room.Self.Location + new Point(
                (int)(TileLoc.X * Scale * Size.X),
                (int)(TileLoc.Y * Scale * Size.Y));
        }
    }
}
