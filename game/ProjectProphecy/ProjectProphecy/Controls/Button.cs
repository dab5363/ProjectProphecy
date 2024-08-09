using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Controls
{
    /// <summary>
    /// A button that can have onTick, on Hover and onClick EventHandlers.
    /// </summary>
    public class Button : UIComponent
    {
        /// <summary>
        /// Available events to register handlers to the button for
        /// </summary>
        public enum Events
        {
            OnTick,
            OnClick,
            OnHover
        }

        // --- Field ---
        private bool isHovering;   // If the mouse cursor is over this object
        private Texture2D texture; // Image for the button
        private SpriteFont font;   // Font for the button's text info (if has)

        // --- Properties ---
        /// <summary>
        /// Text to display
        /// </summary>
        public string Text
        {
            get; set;
        }

        /// <summary>
        /// Color for the text font
        /// </summary>
        public Color FontColor
        {
            get; set;
        }

        /// <summary>
        /// Position (X, Y coordinates) of the button
        /// </summary>
        public Vector2 Position
        {
            get => Center - Size / 2;
            set => Center = value + Size / 2;
        }

        /// <summary>
        /// Size (width by height) of the button
        /// </summary>
        public Vector2 Size
        {
            get => new Vector2(texture.Width, texture.Height);
        }

        /// <summary>
        /// Center of the button
        /// </summary>
        public Vector2 Center
        {
            get; set;
        }

        /// <summary>
        /// Used for hover and click, determine if mouse is over button
        /// **Should be multiplied by the scalar if buttons are being scaled**
        /// </summary>
        public Rectangle Rectangle
        {
            get => new Rectangle((int)Position.X, (int)Position.Y, texture.Width, texture.Height);
        }

        // Events available
        public event EventHandler OnTick;
        public event EventHandler OnClick;
        public event EventHandler OnHover;

        // --- Constructor ---
        /// <summary>
        /// Instantiates a Button with only texture and font pre-set.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="font"></param>
        public Button(Texture2D texture, SpriteFont font)
        {
            this.texture = texture;
            this.font = font;

            FontColor = Color.Black;
        }
        /// <summary>
        /// Constructs a button with full details
        /// </summary>
        /// <param name="name"></param>
        /// <param name="center"></param>
        /// <param name="text"></param>
        /// <param name="texture"></param>
        /// <param name="font"></param>
        /// <param name="color"></param>
        public Button(string name, Vector2 center, string text, Texture2D texture, SpriteFont font, Color color)
        {
            Name = name;
            Center = center;
            Text = text;
            this.texture = texture;
            this.font = font;
            FontColor = color;
        }

        // --- Methods ---
        /// <summary>
        /// Registers a button event handler to the corresponding event.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public Button RegisterEvent(Events type, EventHandler method)
        {
            switch (type)
            {
                case Events.OnClick:
                    OnClick += method;
                    break;
                case Events.OnTick:
                    OnTick += method;
                    break;
                case Events.OnHover:
                    OnHover += method;
                    break;
            }
            return this;
        }

        public override void Update(GameTime gameTime)
        {
            // OnTick handlers process change for the button every game loop
            OnTick?.Invoke(this, new EventArgs());

            MouseState currentMsState = InputManager.Singleton.Mouse;

            // Automatically set to false so it is checked each frame
            isHovering = false;

            // Collision box for mouse
            var mouseCollider = new Rectangle(currentMsState.X, currentMsState.Y, 1, 1);

            // If mouse is over the button, executes OnHover handlers if there is any.
            if (mouseCollider.Intersects(Rectangle))
            {
                isHovering = true;
                OnHover?.Invoke(this, new EventArgs());

                // If the left mouse button was clicked (pressed then released), executes
                // OnClick handlers.
                if (InputManager.Singleton.IsPressed(MouseButtons.LeftButton))
                {
                    OnClick?.Invoke(this, new EventArgs());
                }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Default color variable set to white as button changes color with hovering
            var color = Color.White;

            // If mouse is on button chnage button to gray
            if (isHovering)
            {
                color = Color.LightGray;
            }

            // Draw button
            spriteBatch.Draw_(texture, Rectangle, color);

            // If there is text for the button, draw it in the center of the button
            if (!string.IsNullOrEmpty(Text))
            {
                var x = Rectangle.Center.X - font.MeasureString(Text).X / 2;
                var y = Rectangle.Center.Y - font.MeasureString(Text).Y / 2;

                spriteBatch.DrawString_(font, Text, new Vector2(x, y), FontColor);
            }
        }
    }
}
