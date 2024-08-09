using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectProphecy.ns_Controls;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectProphecy.ns_Graphics
{
    /// <summary>
    /// A UI component that has inifinite potentials. A preliminary attempt to UI production. UI components will be more
    /// integrated, with different variations and belonging to a user interface that's managed by the UIManager.
    /// </summary>
    public class CustomUI
    {
        private string name;        // Name of the component
        // Image attributes
        private Texture2D texture;  // Texture for the component
        private Rectangle section;  // Subsection of image to display. The whole texture image is shown by default.
        // Text attributes
        private List<string> lines; // Lines of text
        private SpriteFont font;    // Font for the text
        // Common attributes
        private Point position;     // Where (the topleft of) the component would be
        private Vector2 scale;      // Width * Height or size multiplier. Depends.
        private float depth;        // layerDepth. In this game, the bigger the number is, the higher display priority
        private Color color;        // Color for the sprite or text

        /// <summary>
        /// Whether the CustomUI is a text component.
        /// </summary>
        public bool IsTextUI
        {
            get => lines != null && lines.Count > 0;
        }

        public Point Position
        {
            get => position;
            set => position = value;
        }

        /// <summary>
        /// Creates a frame, bar, image section or text
        /// </summary>
        /// <param name="name"></param>
        /// <param name="url"></param>
        /// <param name="position"></param>
        /// <param name="section"></param>
        /// <param name="scale"></param>
        public CustomUI(string name, string url, Point position, Rectangle section, Vector2 scale, float depth, List<string> lines, Color color)
        {
            this.name = name;

            this.lines = lines;
            if (IsTextUI)
            {
                font = UIManager.Singleton.GetFont(url);
            }
            else
            {
                texture = UIManager.Singleton.GetTexture(url);
                // If section is not set, use the whole asset.
                if (section.IsEmpty)
                {
                    this.section = new Rectangle(0, 0, texture.Width, texture.Height);
                }
                else
                {
                    this.section = section;
                }
            }
            this.position = position;
            this.scale = scale;
            // A layerDepth of 0 assigns the next unique layer depth to the UI.
            if (depth == default)
            {
                this.depth = Utility.NextLayerDepth();
            }
            // A layerDepth < 0 represents depth of 0 in effect.
            else if (depth < default(float))
            {
            }
            // depth > 0 - keeps the depth set.
            else
            {
                this.depth = depth;
            }
            this.color = color;
        }

        /// <summary>
        /// Converts the lines string array into a single string, each connected with a line break character
        /// </summary>
        private string SingleString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];
                    sb.Append(line);
                    if (i < lines.Count - 1)
                    {
                        sb.Append("\n");
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Creates a new UIBuilder to help construct the CustomUI.
        /// </summary>
        /// <returns></returns>
        public static UIBuilder New()
        {
            return new UIBuilder();
        }

        /// <summary>
        /// Draws the custom UI component
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsTextUI)
            {
                spriteBatch.DrawString(font, SingleString, position.ToVector2(), color, 0, Vector2.Zero, scale, SpriteEffects.None, depth);
            }
            else
            {
                float xScale = scale.X / section.Width;
                float yScale = scale.Y / section.Height;
                spriteBatch.Draw(texture, position.ToVector2(), section, color, 0, Vector2.Zero, new Vector2(xScale, yScale), SpriteEffects.None, depth);
            }
        }

        public class UIBuilder
        {
            // --- Fields ---
            private string name;               // Name of the asset
            private string url;                // Path (effectively just the file name) of the asset -> Texture2D or SpriteFont
            private Point position;            // Position on the screen

            private Rectangle section;         // Subsection of the texture in the asset
            private List<string> lines;        // Texts to display

            private Vector2 scale;             // Displayed width and height
            private int depth;                 // Determines the render order in the same sprite batch
            private Color color = Color.White; // Color for the sprite or text

            // --- Methods
            /// <summary>
            /// Sets the UI component's name and file name of its asset
            /// </summary>
            /// <param name="name"></param>
            /// <param name="url"></param>
            /// <returns></returns>
            public UIBuilder Initialize(string name, string url)
            {
                this.name = name;
                this.url = url;
                return this;
            }

            /// <summary>
            /// Sets the location of the UI component on the screen
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public UIBuilder SetPosition(int x, int y)
            {
                position = new Point(x, y);
                return this;
            }

            /// <summary>
            /// Sets the layer depth of the UI component
            /// </summary>
            /// <param name="z"></param>
            /// <returns></returns>
            public UIBuilder SetDepth(int z)
            {
                depth = z;
                return this;
            }

            /// <summary>
            /// Sets the X coordinate of the UI component
            /// </summary>
            /// <param name="x"></param>
            /// <returns></returns>
            public UIBuilder SetX(int x)
            {
                position.X = x;
                return this;
            }

            /// <summary>
            /// Sets the Y coordinate of the UI component
            /// </summary>
            /// <param name="y"></param>
            /// <returns></returns>
            public UIBuilder SetY(int y)
            {
                position.Y = y;
                return this;
            }

            /// <summary>
            /// Sets the subsection of the texture to display if it's an image section
            /// </summary>
            /// <param name="section"></param>
            /// <returns></returns>
            public UIBuilder SetSection(Rectangle section)
            {
                this.section = section;
                return this;
            }

            /// <summary>
            /// Sets the subsection of the texture to display if it's an image section
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="width"></param>
            /// <param name="height"></param>
            /// <returns></returns>
            public UIBuilder SetSection(int x, int y, int width, int height)
            {
                return SetSection(new Rectangle(x, y, width, height));
            }

            /// <summary>
            /// Sets the actual width and height for the component. Final size = xScale * yScale
            /// </summary>
            /// <param name="xScale"></param>
            /// <param name="yScale"></param>
            /// <returns></returns>
            public UIBuilder SetScale(int xScale, int yScale)
            {
                scale = new Vector2(xScale, yScale);
                return this;
            }

            /// <summary>
            /// Sets the multipliers for the scale, final size = (x * xScale, y * yScale)
            /// </summary>
            /// <param name="xScale"></param>
            /// <param name="yScale"></param>
            /// <returns></returns>
            public UIBuilder SetScale(float xScale, float yScale)
            {
                scale = new Vector2(xScale, yScale);
                return this;
            }

            /// <summary>
            /// Sets the text to display. This action tags the CustomUI as a text component.
            /// </summary>
            /// <param name="lines"></param>
            /// <returns></returns>
            public UIBuilder SetText(params string[] lines)
            {
                this.lines = lines.ToList();
                return this;
            }

            /// <summary>
            /// Sets the color for the sprite or text
            /// </summary>
            /// <param name="color"></param>
            /// <returns></returns>
            public UIBuilder SetColor(Color color)
            {
                this.color = color;
                return this;
            }

            /// <summary>
            /// Constructs the CustomUI using already loaded data.
            /// </summary>
            /// <returns></returns>
            public CustomUI Build()
            {
                return new CustomUI(name, url, position, section, scale, MathHelper.Clamp(depth / UIManager.MaxLayerDepth, 0, 1), lines, color);
            }
        }
    }
}
