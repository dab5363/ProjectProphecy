using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ProjectProphecy.ns_Graphics
{
    /// <summary>
    /// Visible; has shape. Can be drawn on the screen.
    /// The visible here is different from the return value of IsVisible().
    /// Even if it's outside of screen, it has the nature of being seen.
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// Name of the object
        /// </summary>
        string Name
        {
            get; set;
        }

        /// <summary>
        /// The image file which contains the IDrawable
        /// </summary>
        Texture2D Asset
        {
            get; set;
        }

        /// <summary>
        /// Section for the IDrawable in the asset file
        /// </summary>
        Rectangle Section
        {
            get; set;
        }

        /// <summary>
        /// Rotation when displayed. *In radians.
        /// </summary>
        float Rotation
        {
            get; set;
        }

        /// <summary>
        /// Scale when displayed.
        /// </summary>
        float Scale
        {
            get; set;
        }

        /// <summary>
        /// If the object is set to visible
        /// </summary>
        bool IsVisible
        {
            get; set;
        }

        /// <summary>
        /// Draws the object on the screen, using screen coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rectangle"> Whether it's using a rectangle image or circle one</param>
        // Do NOT implement this in any class! This is not sealed for child interfaces only!
        void Display(int x, int y, bool rectangle)
        {
            if (!IsVisible || !OnScreen(rectangle))
            {
                return;
            }
            Vector2 location = TrySynchronize(new Vector2(x, y));
            // If rotation or scaling applies, the sprite needs an offset to have the same center.
            float xOffset = Section.Width / 2f;
            float yOffset = Section.Height / 2f;
            Game1.Singleton.SpriteBatch.Draw_(
                Asset,
                new Vector2(location.X + xOffset, location.Y + yOffset),
                Section,
                Color.White,
                Rotation,
                new Vector2(xOffset, yOffset),
                Scale,
                SpriteEffects.None);
            // TODO
        }

        /// <summary>
        /// When the IDrawable is also an IEntity.
        /// Gives the display location an offset to have the same center as the entity BoundingBox.
        /// </summary>
        /// <param name="location"> Where to display the image </param>
        /// <returns></returns>
        sealed Vector2 TrySynchronize(Vector2 location)
        {
            if (this is IEntity)
            {
                IEntity entity = this as IEntity;
                Rectangle boundingBox = entity.BoundingBox;
                Rectangle displayArea = new Rectangle((int)location.X, (int)location.Y, Section.Width, Section.Height);
                Vector2 displacement = (boundingBox.Center - displayArea.Center).ToVector2();
                return location += displacement;
            }
            return location;
        }

        /// <summary>
        /// TODO:
        /// If any bit of the object is inside the screen. It should not be drawn when invisible.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        sealed bool OnScreen(bool rectangle)
        {
            return true;
        }
    }
}
