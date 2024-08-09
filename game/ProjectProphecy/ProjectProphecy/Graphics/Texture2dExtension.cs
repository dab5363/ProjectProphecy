using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Graphics
{
    /// <summary>
    /// Extension methods for Texture2D class. Can get a 1D
    /// or 2D array of the pixels of a sprite / image
    /// </summary>
    public static class Texture2dExtension
    {
        public static Color GetPixel(this Color[] colors, int x, int y, int width)
        {
            return colors[x + (y * width)];
        }
        public static Color[] GetPixels(this Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(colors1D);
            return colors1D;
        }

        public static Color[,] GetPixels2(this Texture2D texture)
        {
            return GetPixels2(texture, 0, 0, texture.Width, texture.Height);
        }

        public static Color[,] GetPixels2(this Texture2D texture, Rectangle section)
        {
            return GetPixels2(texture, section.X, section.Y, section.Width, section.Height);

        }

        public static Color[,] GetPixels2(this Texture2D texture, int x, int y, int width, int height)
        {
            Color[] colors1D = texture.GetPixels();
            Color[,] colors2D = new Color[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    colors2D[i, j] = colors1D[(i + x) + (j + y) * texture.Width];
                }
            }
            return colors2D;
        }
    }
}
