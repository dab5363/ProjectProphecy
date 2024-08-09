using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using static ProjectProphecy.ns_Graphics.Animation;
using System.Net.NetworkInformation;

namespace ProjectProphecy.ns_Graphics
{
    /// <summary>
    /// Has one or more animations to play while performing corresponding actions (not necessarily).
    /// </summary>
    public interface IAnimatable : IDrawable
    //TODO : improvements
    {
        // --- Properties ---
        /// <summary>
        /// If the object is facing left. Determines whether to flip the sprite.
        /// </summary>
        bool IsFaceLeft
        {
            get; set;
        }

        /// <summary>
        /// If set to true, IsFaceLeft can't be set.
        /// </summary>
        bool FixedFacing
        {
            get; set;
        }

        /// <summary>
        /// Animations available for this character
        /// </summary>
        Dictionary<string, Animation> Animations
        {
            get; set;
        }

        /// <summary>
        /// Animation to swap to when the current animation ends.
        /// </summary>
        Animation DefaultAnimation
        {
            get; set;
        }

        /// <summary>
        /// Current Animation being displayed.
        /// </summary>
        Animation CurrentAnimation
        {
            get; set;
        }

        /// <summary>
        /// If the animation can be changed. Generally means the current animation is very important.
        /// </summary>
        bool FixedAnimation
        {
            get; set;
        }

        /// <summary>
        /// When the current animation started
        /// </summary>
        DateTime AnimationStartTime
        {
            get; set;
        }

        /// <summary>
        /// When the current animation should stop and change to the default
        /// </summary>
        sealed DateTime AnimationExpireTime
        {
            get => AnimationStartTime.Add(TimeSpan.FromSeconds(CurrentAnimation.Duration));
        }

        /// <summary>
        /// Counter for the elapsed time since last frame + leftover (excess in the last frame over TimePerFrame)
        /// </summary>
        double TimeCounter
        {
            get; set;
        }

        /// <summary>
        /// The current animation frame
        /// </summary>
        int CurrentFrame
        {
            get; set;
        }

        /// <summary>
        /// The rectangle used to represent the current frame in the sprite sheet.
        /// </summary>
        sealed Rectangle CurrentFrameSection
        {
            get => CurrentAnimation.FrameSections[CurrentFrame];
        }

        /// <summary>
        /// How many frames the current animation has.
        /// </summary>
        int Frames
        {
            get => CurrentAnimation.Frames;
        }

        /// <summary>
        /// How many frames to play every second. Can be decimals.
        /// </summary>
        double FPS
        {
            get; set;
        }

        /// <summary>
        /// The amount of time (in fractional seconds) per frame
        /// </summary>
        double TimePerFrame
        {
            get => 1.0 / FPS;
        }

        // --- Methods ---
        // TODO
        /// <summary>
        /// Updates the animation based on the current animation settings.
        /// TODO: Accepts one AnimationLogic parameter for state transitions -> ???
        /// </summary>
        sealed void Update()
        {
            GameTime gameTime = Game1.Singleton.GameTime;
            // Time elapsed since the last frame
            double elapsedTime = gameTime.ElapsedGameTime.TotalSeconds;
            // Adds interval from the last frame to the counter.
            TimeCounter += elapsedTime;
            // Passed frames using accumulated time in the counter
            int frames;
            // Either a continuous animation or the default animation loops until manually changed to another.
            // If the animation is temporary, check if it has run out of time
            if (!CurrentAnimation.IsContinuous && CurrentAnimation != DefaultAnimation)
            {
                // If the current animation has played for enough time, swap to the default animation.
                frames = (int)(TimeCounter / TimePerFrame);
                int lastFrameIndex = Frames - 1;
                if (CurrentFrame + frames > lastFrameIndex)
                {
                    // When the current animation ends, the animation locks/restrictions shall be removed.
                    // May be added again depending on the new animation's properties.
                    FixedAnimation = false;
                    FixedFacing = false;
                    Animation expired = CurrentAnimation;
                    if (SetAnimation(DefaultAnimation, false, false))
                    {
                        // Calls the end event for the just expired animation
                        OnAnimationEnd(expired);
                        // Part of the elapsed time is within the finishing animation's duration.
                        // We only need the excess to be taken into the calculation of the frame advance for the new animation
                        TimeCounter -= (lastFrameIndex - CurrentFrame) * TimePerFrame;
                    }
                }
            }
            // If the current frame has played for enough time, proceed to the next.
            // *May skip frames to reduce calculation if the game is hard lagging
            frames = (int)(TimeCounter / TimePerFrame);
            while (frames > 0)
            {
                CurrentFrame++;
                frames--;
                // If no more frames in the animation sequence, replay.
                if (CurrentFrame >= Frames)
                {
                    CurrentFrame = 0;
                }
                // Doesn't reset the time counter - rest of time should also be used.
                TimeCounter -= TimePerFrame;
            }
        }

        /// <summary>
        /// Sets the current animation to the specified and restarts current animation.
        /// </summary>
        /// <param name="animation"> New animation </param>
        /// <returns> If the animation was changed </returns>
        sealed bool SetAnimation(string animation)
        {
            return SetAnimation(animation, false, false);
        }

        /// <summary>
        /// Sets the current animation to the specified and restarts current animation.
        /// Has restrictions to determine if change is really needed.
        /// </summary>
        /// <param name="animation"> New animation </param>
        /// <param name="canBeSame"> Whether the new animation can be same as the current. </param>
        /// <param name="ignoreFixed"> Whether to still change the animation when FixedAnimation is true. </param>
        /// <returns> If the animation was changed </returns>
        sealed bool SetAnimation(string animation, bool canBeSame, bool ignoreFixed)
        {
            if (!Animations.ContainsKey(animation))
            {
                Logger.Log($"Error setting animation - no animation called \"{animation}\" available for {Name}.");
                return false;
            }
            return SetAnimation(Animations[animation], canBeSame, ignoreFixed);
        }

        /// <summary>
        /// Sets the current animation to the specified and restarts current animation.
        /// </summary>
        /// <param name="animation"> New animation </param>
        /// <returns> If the animation was changed </returns>
        sealed bool SetAnimation(Animation animation)
        {
            return SetAnimation(animation, true, false);
        }

        /// <summary>
        /// Sets the current animation to the specified and restarts current animation.
        /// Has restrictions to determine if change is really needed.
        /// </summary>
        /// <param name="animation"> New animation </param>
        /// <param name="canBeSame"> Whether the new animation can be same as the current. </param>
        /// <param name="ignoreFixed"> Whether to still change the animation when FixedAnimation is true. </param>
        /// <returns> If the animation was changed </returns>
        sealed bool SetAnimation(Animation animation, bool canBeSame, bool ignoreFixed)
        {
            bool same = CurrentAnimation == animation;
            // When change of animation is not allowed, continues only when the new animation can ignore the restriction.
            // If the new animation is the same, ignore this.
            if (FixedAnimation && !ignoreFixed && !same)
            {
                return false;
            }
            // When both animations are the same, resets the animation only when reset is allowed.
            if (same && !canBeSame)
            {
                return false;
            }
            CurrentAnimation = animation;
            CurrentFrame = 0;
            TimeCounter = 0;
            FPS = animation.FPS;
            AnimationStartTime = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Draws the current frame of the object's animation on the screen, using screen coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rectangle"></param>
        void IDrawable.Display(int x, int y, bool rectangle)
        {
            if (!IsVisible || !OnScreen(rectangle))
            {
                return;
            }
            Vector2 location = TrySynchronize(new Vector2(x, y));
            SpriteEffects flipSprite = IsFaceLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Animation animation = CurrentAnimation;
            // If rotation or scaling applies, the sprite needs an offset to have the same center.
            float xOffset = Section.Width / 2f;
            float yOffset = Section.Height / 2f;
            Game1.Singleton.SpriteBatch.Draw_(
                animation.SpriteSheet,
                new Vector2(location.X + xOffset, location.Y + yOffset),
                animation.FrameSections[CurrentFrame],
                Color.White,
                Rotation,
                new Vector2(xOffset, yOffset),
                Scale,
                flipSprite);
        }

        public virtual void OnAnimationEnd(Animation expired)
        {

        }
    }
}
