using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.Map
{
    /// <summary>
    /// A single room (playable scene) in the game world.
    /// </summary>
    public class Room
    {
        // --- Fields ---
        private string name;
        private (int width, int height) dimensions;  // Width and height measured in tiles
        private Dictionary<Point, Tile> tiles;       // Tiles in the room
        private Dictionary<Room, Door> doors         // Doors connected to other rooms
                = new Dictionary<Room, Door>();
        private Rectangle self;                      // Shape of the room it self

        // --- Properties ---
        public string Name
        {
            get => name;
        }

        public (int width, int height) Dimensions
        {
            get => dimensions;
            set => dimensions = value;
        }

        public Dictionary<Point, Tile> Tiles
        {
            get => tiles;
        }

        public Dictionary<Room, Door> Doors
        {
            get => doors;
        }

        /// <summary>
        /// Shape of the room it self
        /// </summary>
        public Rectangle Self
        {
            get => self;
        }

        /// <summary>
        /// Room connected to the left
        /// </summary>
        public Room Left
        {
            get;
            set;
        }
        public Room Right
        {
            get;
            set;
        }
        public Room Top
        {
            get;
            set;
        }
        public Room Bottom
        {
            get;
            set;
        }

        // --- Constructors ---
        public Room(string name, int width, int height, Dictionary<Point, Tile> tiles)
        {
            this.name = name;
            dimensions = (width, height);
            this.tiles = tiles;

            Rectangle GameWindow = Game1.Singleton.GameWindow;

            // TODO: I'm currently using magic numbers here! But it's actually tile.scale*tile.size
            self = new Rectangle(0, 0, 64 * dimensions.width, 64 * dimensions.height);
            self.Location = Utility.AlignCenters(GameWindow, self).ToPoint();

            foreach (Tile tile in tiles.Values)
            {
                tile.Room = this;
            }
        }

        /// <summary>
        /// When the player walks, update the tiles' locations and process interactions
        /// </summary>
        public void Update()
        {
            Player player = Game1.Singleton.Player;
            Vector2 displacement = (player.BoundingBox.Location - player.PreviousBoundingBox.Location).ToVector2();

            Rectangle gameWindow = Game1.Singleton.GameWindow;
            // Scrolls the map - Only if the player is in the X/Y center
            Rectangle Xcenter = new Rectangle(0, 0, 300, int.MaxValue);
            Rectangle Ycenter = new Rectangle(0, 0, int.MaxValue, 300);
            Xcenter.Location = Utility.AlignCenters(gameWindow, Xcenter).ToPoint();
            Ycenter.Location = Utility.AlignCenters(gameWindow, Ycenter).ToPoint();

            Point previousLocation = self.Location;
            bool hasScrolled = false;
            if (Xcenter.Contains(player.PreviousBoundingBox))
            {
                //    Moving Left AND Room's left is out of window's left
                // OR Moving Right AND Room's right is out of window's right
                if (displacement.X < 0 && self.Left < gameWindow.Left
                    || displacement.X > 0 && self.Right > gameWindow.Right)
                {
                    self.X -= (int)displacement.X;
                    hasScrolled = true;
                }
            }
            //    Moving Upward AND Room's top is out of window's top
            // OR Moving Downward AND Room's bottom is out of window's bottom
            if (Ycenter.Contains(player.PreviousBoundingBox))
            {
                if (displacement.Y < 0 && self.Top < gameWindow.Top
                    || displacement.Y > 0 && self.Bottom > gameWindow.Bottom)
                {
                    self.Y -= (int)displacement.Y;
                    hasScrolled = true;
                }
            }
            // Updates all entities' locations in the room
            // TODO: actually all IEntitys should be changed, not only living entities.
            if (hasScrolled)
            {
                Point offset = self.Location - previousLocation;
                foreach (LivingEntity entity in EntityManager.Singleton.GetLivingEntities(name))
                {
                    entity.Location += offset;
                }
            }

            // Updates each tile, including their coordinates and individual functionality
            foreach (Tile tile in tiles.Values)
            {
                tile.Update();
            }
        }

        public void Draw()
        {
            Rectangle gameWindow = Game1.Singleton.GameWindow;
            foreach (Tile tile in tiles.Values)
            {
                // Only displays the tile if it's not completely out of window 
                if (gameWindow.Intersects(tile.BoundingBox))
                {
                    AnimationManager.Singleton.Update(tile);
                }
            }
        }
    }
}


