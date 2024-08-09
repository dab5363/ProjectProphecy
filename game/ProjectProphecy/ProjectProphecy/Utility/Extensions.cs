using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ProjectProphecy.ns_Utility
{
    /// <summary>
    /// Extension methods to various classes
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension Draw method applying NextLayerDepth instead of a 0
        /// which would cause overlapping with others having a 0 depth if
        /// the SpriteSortMode does not ignore layer depths.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="texture"></param>
        /// <param name="destinationRectangle"></param>
        /// <param name="color"></param>
        public static void Draw_(this SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            spriteBatch.Draw(texture, destinationRectangle, null, color, 0, Vector2.Zero, SpriteEffects.None, Utility.NextLayerDepth());
        }

        /// <summary>
        /// Extension Draw method applying NextLayerDepth instead of a 0
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="sourceRectangle"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        /// <param name="scale"></param>
        /// <param name="effects"></param>
        public static void Draw_(this SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects)
        {
            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, Utility.NextLayerDepth());
        }

        /// <summary>
        /// Extension DrawString method applying NextLayerDepth instead of a 0
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="spriteFont"></param>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        public static void DrawString_(this SpriteBatch spriteBatch, SpriteFont spriteFont, string text, Vector2 position, Color color)
        {
            spriteBatch.DrawString(spriteFont, text, position, color, 0, Vector2.Zero, 1, SpriteEffects.None, Utility.NextLayerDepth());
        }

        /// <summary>
        /// Extension DrawString method applying NextLayerDepth instead of a 0
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="spriteFont"></param>
        /// <param name="text"></param>
        /// <param name="position"></param>
        public static void DrawString_(this SpriteBatch spriteBatch, SpriteFont spriteFont, string text, Vector2 position)
        {
            spriteBatch.DrawString(spriteFont, text, position, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, Utility.NextLayerDepth());
        }

        /// <summary>
        /// Improved Normalize method. If the Vector is a zero vector, keep it as the same or the X, Y would become NaN.
        /// </summary>
        /// <param name="vector"></param>
        public static void Normalize_(this Vector2 vector)
        {
            if (vector != Vector2.Zero)
            {
                vector.Normalize();
            }
        }

        /// <summary>
        /// Multiplies a rectangle's size and keeps the center at the same location
        /// Note: because Rectangle is a struct type, knowing the underlying implementation of a extension method,
        /// we really really can't directly effect the original rectangle's value. (parameter copied instead of
        /// being a reference.)
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="multiplication"></param>
        /// <returns></returns>
        public static Rectangle Multiply(this Rectangle rectangle, float multiplication)
        {
            Rectangle newRect = new Rectangle(
                rectangle.X,
                rectangle.Y,
                (int)(rectangle.Width * multiplication),
                (int)(rectangle.Height * multiplication));
            newRect.Location = Utility.AlignCenters(rectangle, newRect).ToPoint();
            return newRect;
        }
    }
}
