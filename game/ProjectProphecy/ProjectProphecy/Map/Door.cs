using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.Map
{
    public class Door : Tile
    {
        // --- Fields ---
        public Room ConnectedRoom
        {
            get; set;
        }

        // --- Constructors ---
        public Door(
            string name, Point tileLoc, float scale = 1) : base(name, tileLoc, scale)
        {

        }

        public override void Update()
        {
            base.Update();
            // Go to the next room;
            if (Game1.Singleton.Player.BoundingBox.Intersects(BoundingBox))
            {
                if (ConnectedRoom != null)
                {
                    Game1.Singleton.CurrentRoom = ConnectedRoom;
                }
            }
        }
    }
}
