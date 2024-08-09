using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;

namespace ProjectProphecy.ns_Utility
{
    /// <summary>
    /// Utility class. Helps with certain algorithm and aspects
    /// </summary>
    public static class Utility
    {
        public static long currentLayerDepth = 0;
        private static TimeSpan lastProcessedFrame = TimeSpan.Zero;

        /// <summary>
        /// Gets the next layer depth float number so every element drawn using this will have unique
        /// layer depths.
        /// </summary>
        /// <returns></returns>
        public static float NextLayerDepth()
        {
            TimeSpan currentFrame = Game1.Singleton.GameTime.TotalGameTime;
            // Resets the counter to zero so it's not accumulating across frames
            if (lastProcessedFrame != currentFrame)
            {
                lastProcessedFrame = currentFrame;
                currentLayerDepth = 0;
            }
            currentLayerDepth++;
            return Microsoft.Xna.Framework.MathHelper.Clamp(currentLayerDepth / UIManager.MaxLayerDepth, 0, 1);
        }

        /// <summary>
        /// Aligns the center of toAlign to the center of standard. Returns the new location of toAlign.
        /// </summary>
        /// <param name="standard"></param>
        /// <param name="toAlign"></param>
        /// <returns>Adjusted top left point of toAlign</returns>
        public static Vector2 AlignCenters(Rectangle standard, Rectangle toAlign)
        {
            Vector2 displacement = (standard.Center - toAlign.Center).ToVector2();
            return toAlign.Location.ToVector2() + displacement;
        }

        public static Vector2 AlignString(string text, string fontName)
        {
            return AlignString(text, fontName, new Vector2(1, 1));
        }

        public static Vector2 AlignString(string text, string fontName, float xScale, float yScale)
        {
            return AlignString(text, fontName, new Vector2(xScale, yScale));
        }

        public static Vector2 AlignString(string text, string fontName, Vector2 scale)
        {
            SpriteFont font = UIManager.Singleton.GetFont(fontName);
            return -font.MeasureString(text) / 2 * scale;
        }

        public class MathHelper
        {
            /// <summary>
            /// Double version of MathHelper.Clamp
            /// </summary>
            /// <param name="value"> value would fall within the range from min to max </param>
            /// <param name="min"> min value </param>
            /// <param name="max"> max value</param>
            /// <returns> clamped value using min and max </returns>
            public static double Clamp(double value, double min, double max)
            {
                if (value > max)
                {
                    value = max;
                }
                else if (value < min)
                {
                    value = min;
                }
                return value;
            }

            /// <summary>
            /// Re-maps a number from one range to another.
            /// </summary>
            /// <param name="value"> the incoming value to be converted </param>
            /// <param name="start1"> lower bound of the value's current range </param>
            /// <param name="stop1"> upper bound of the value's current range </param>
            /// <param name="start2"> lower bound of the value's target range </param>
            /// <param name="stop2"> upper bound of the value's target range </param>
            /// <param name="withinBounds"> constrain the value to the newly mapped range </param>
            /// <returns></returns>
            public static double Map(double value, double start1, double stop1, double start2, double stop2, bool withinBounds = true)
            {
                double ratio = (stop2 - start2) / (stop1 - start1);
                double mapped = start2 + (value - start1) * ratio;
                if (withinBounds)
                {
                    mapped = Clamp(mapped, start2, stop2);
                }
                return mapped;
            }

            /// <summary>
            /// TODO: I don't know how to describe this in English right now. I->0->I->0->...
            /// value *= 0/I -> I/I=1 -> 0 -> 1...
            /// </summary>
            /// <param name="interval"></param>
            /// <param name="step"></param>
            /// <returns></returns>
            public static double BackAndForth(double value, int interval, int step)
            {
                return value * (interval - Math.Abs(interval - step % (2 * interval))) / interval;
            }

            /// <summary>
            /// Gets proportional value in a time period of interval seconds
            /// </summary>
            /// <param name="value"></param>
            /// <param name="time"></param>
            /// <param name="interval"></param>
            /// <returns> Value per interval seconds * time seconds </returns>
            public static double SmoothUpdate(double value, double time, double interval)
            {
                return value * time / interval;
            }
        }
    }
}
