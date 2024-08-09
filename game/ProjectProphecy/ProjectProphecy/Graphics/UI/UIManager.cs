using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectProphecy.ns_Controls;
using ProjectProphecy.ns_Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using ProjectProphecy.ns_Utility;


namespace ProjectProphecy.ns_Graphics
{
    /// <summary>
    /// Loads and stores all the UI assets available.
    /// Manages UIs, UI components and etc.
    /// </summary>
    public class UIManager
    {
        // --- Fields ---
        // Singleton of the manager class
        private static readonly UIManager manager = new UIManager();
        public static float MaxLayerDepth = 1E8f;

        private HashSet<UIComponent> components = new HashSet<UIComponent>();

        // --- Static Constructor ---
        static UIManager()
        {
            ContentManager content = Game1.Singleton.Content;
            DirectoryInfo dir;
            // Loads Texture2D assets
            // Assuming only image and config files exist, excludes all config files so the rest are image files
            dir = new DirectoryInfo(root);
            FileInfo[] textures = dir.GetFiles().Where(file => file.Extension != ".yml" && file.Extension != ".txt").ToArray();
            Logger.Log($"{textures.Length} texture assets found. Trying to load...");
            foreach (FileInfo texture in textures)
            {
                string name = texture.Name.Substring(0, texture.Name.Length - texture.Extension.Length);
                Logger.Log($"Loading texture asset: {texture.Name}");
                try
                {
                    manager.textures[name] = content.Load<Texture2D>(contentRoot + name);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error loading texture {texture.Name} - {ex.Message}\n{ex.StackTrace}");
                }
                Logger.Log($"{name} Successfully loaded");
            }

            // Loads SpriteFont assets
            dir = new DirectoryInfo(fontDir.root);
            FileInfo[] fonts = dir.GetFiles().Where(file => file.Extension == ".spritefont").ToArray();
            Logger.Log($"{textures.Length} font assets found. Trying to load...");
            foreach (FileInfo font in fonts)
            {
                string name = font.Name.Substring(0, font.Name.Length - font.Extension.Length);
                Logger.Log($"Loading font asset: {font.Name}");
                try
                {
                    manager.fonts[name] = content.Load<SpriteFont>(fontDir.contentRoot + name);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error loading font {font.Name} - {ex.Message}\n{ex.StackTrace}");
                }
                Logger.Log($"{name} Successfully loaded");
            }
        }

        // All UI assets
        private readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>(StringComparer.OrdinalIgnoreCase);
        // All buttons
        private readonly Dictionary<string, Button> buttons = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);
        // Where all the UI assets are stored
        private readonly static string root = @"..\..\..\Content\UI\";
        private readonly static string contentRoot = @"UI\";
        private readonly static (string root, string contentRoot) fontDir = (root + @"Fonts\", contentRoot + @"Fonts\");

        public Texture2D GetTexture(string name)
        {
            return textures.GetValueOrDefault(name, null);
        }

        public SpriteFont GetFont(string name)
        {
            return fonts.GetValueOrDefault(name, null);
        }

        /// <summary>
        /// Use this property to call manager functions.
        /// </summary>
        public static UIManager Singleton
        {
            get => manager;
        }

        public Button RegisterButton(string name, Vector2 center, string text, string path, SpriteFont font, Color color)
        {
            path = contentRoot + path;
            Button button = new Button(name, center, text, Game1.Singleton.Content.Load<Texture2D>(path), font, color);
            buttons[name] = button;
            return button;
        }

        public Button RegisterButton(Button button)
        {
            buttons[button.Name] = button;
            return button;
        }

        public bool RegisterComponent(UIComponent component)
        {
            return components.Add(component);
        }

        public bool UnregisterComponent(UIComponent component)
        {
            return components.Remove(component);
        }

        public void UpdateAllComponents()
        {
            GameTime gameTime = Game1.Singleton.GameTime;
            List<UIComponent> components = this.components.ToList();
            foreach (UIComponent component in components)
            {
                component.Update(gameTime);
            }
        }

        public void DrawAllComponents()
        {
            GameTime gameTime = Game1.Singleton.GameTime;
            SpriteBatch spriteBatch = Game1.Singleton.SpriteBatch;
            foreach (UIComponent component in components)
            {
                component.Draw(gameTime, spriteBatch);
            }
        }

        /// <summary>
        /// Creates and returns a 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="xScale"></param>
        /// <param name="yScale"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public CustomUI GetBar(string name, int x, int y, int z, int xScale, int yScale, double min, double max)
        {
            return GetBar(name, name, x, y, z, xScale, yScale, min, max);
        }

        public CustomUI GetBar(string name, string url, int x, int y, int z, int xScale, int yScale, double min, double max)
        {
            return CustomUI.New().Initialize(name, url).SetPosition(x, y).SetDepth(z).SetScale((int)(xScale * (min / max)), yScale).Build();
        }

        public CustomUI GetFrame(string name, int x, int y, int z, int xScale, int yScale)
        {
            return GetFrame(name, name, x, y, z, xScale, yScale);
        }

        public CustomUI GetFrame(string name, string url, int x, int y, int z, int xScale, int yScale)
        {
            return CustomUI.New().Initialize(name, url).SetPosition(x, y).SetDepth(z).SetScale(xScale, yScale).Build();
        }

        public CustomUI GetImageSection(string name, int x, int y, int z, int u, int v, int uWidth, int uHeight, int xScale, int yScale)
        {
            return GetImageSection(name, name, x, y, z, u, v, uWidth, uHeight, xScale, yScale);
        }

        public CustomUI GetImageSection(string name, string url, int x, int y, int z, int u, int v, int uWidth, int uHeight, int xScale, int yScale)
        {
            return CustomUI.New().Initialize(name, url).SetPosition(x, y).SetDepth(z).SetSection(u, v, uWidth, uHeight).SetScale(xScale, yScale).Build();
        }

        public CustomUI GetText(string name, int x, int y, int z, float xScale, float yScale, params string[] lines)
        {
            return GetText(name, name, x, y, z, xScale, yScale, lines);
        }

        public CustomUI GetText(string name, string url, int x, int y, int z, float xScale, float yScale, params string[] lines)
        {
            return GetText(name, url, x, y, z, xScale, yScale, Color.White, lines);
        }

        public CustomUI GetText(string name, string url, int x, int y, int z, float xScale, float yScale, Color color, params string[] lines)
        {
            return CustomUI.New().Initialize(name, url).SetPosition(x, y).SetDepth(z).SetText(lines).SetScale(xScale, yScale).SetColor(color).Build();
        }
    }
}
