using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Graphics
{
    /// <summary>
    /// Playable animation for an IAnimatable object.
    /// </summary>
    public class Animation
    {
        // --- Fields ---
        private (int width, int height) size; // Tuple. Stores size of any single sprite in the sprite sheet.
        private Texture2D spriteSheet;        // Sprite sheet that stores animations for the character.
        // Texture2D[] if using single-frame sprite images; Rectangle[] if using a sprite sheet.
        private Rectangle[] subsections;      // Locations of the sprites/frames for the animation in the sprite sheet
        private int row;                      // In which row of the sprite sheet the animation locates
        private string name;                  // Name of the animation; Unique within all animations for a character 
        private int frames;                   // How many frames the animation has in a complete play
        private double fps;                   // Frames per second. Can be decimals.
        private double duration;              // How long before the animation ends in seconds

        // --- Properties ---
        /// <summary>
        /// Tuple. Stores size of any single sprite in the sprite sheet.
        /// </summary>
        public (int width, int height) Size
        {
            get => size;
        }

        /// <summary>
        /// Sprite sheet that stores animations for the character.
        /// </summary>
        public Texture2D SpriteSheet
        {
            get => spriteSheet;
        }

        /// <summary>
        /// Locations of the sprites/frames for the animation in the sprite sheet
        /// </summary>
        public Rectangle[] FrameSections
        {
            get => subsections;
        }

        /// <summary>
        /// In which row of the sprite sheet the animation locates
        /// </summary>
        public int Row
        {
            get => row;
        }

        /// <summary>
        /// Name of the animation; Unique within all animations for a character 
        /// </summary>
        public string Name
        {
            get => name;
        }

        /// <summary>
        /// How many frames the animation has in a complete play
        /// </summary>
        public int Frames
        {
            get => frames;
        }

        /// <summary>
        /// Frames per second. Can be decimals.
        /// </summary>
        public double FPS
        {
            get => fps;
        }

        /// <summary>
        /// How long before the animation ends in seconds
        /// </summary>
        public double Duration
        {
            get => duration;
        }

        /// <summary>
        /// If the animation lasts forever until it's manually canceled
        /// </summary>
        public bool IsContinuous
        {
            get => duration <= 0;
        }

        // --- Constructors ---
        public Animation((int width, int height) size, Texture2D spriteSheet, Rectangle[] subsections, int row, string name, int frames, double FPS, double duration)
        {
            this.size = size;
            this.spriteSheet = spriteSheet;
            this.subsections = subsections;
            this.row = row;
            this.name = name;
            this.frames = frames;
            this.fps = FPS;
            this.duration = duration;
        }

        // --- Methods ---
        public override string ToString()
        {
            return $"{size}, #{row}, {name}, {frames} frames, {fps} fps, {duration} duration";
        }
    }
}
