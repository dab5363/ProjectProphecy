using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectProphecy.ns_Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Graphics
{
    /// <summary>
    /// Can assign abstract Draw() and Update() for necessary components
    /// TODO: sprint 3
    /// </summary>
    public abstract class UIComponent : IDrawable
    {
        public enum Type
        {
            Frame,
            Bar,
            Text,
            Button
        }

        // --- Properties ---
        public string Name
        {
            get;
            set;
        }
        = "Undefined";

        public virtual Texture2D Asset { get; set; }

        public Rectangle Section { get; set; }

        public float Rotation { get; set; }

        public float Scale { get; set; }

        public bool IsVisible { get; set; } = true;

        // --- Methods ---
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        public abstract void Update(GameTime gameTime);
    }
}
