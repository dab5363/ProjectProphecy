using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Controls;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Skill;
using ProjectProphecy.ns_Utility;
using ProjectProphecy.Entity.Stats;

namespace ProjectProphecy.ns_Entity
{
    /// <summary>
    /// Avatar of the player that the player controls.
    /// </summary>
    public class Player : LivingEntity
    {
        // --- Fields ---
        private double mana;
        private double maxMana = 100;
        // --- Properties ---
        /// <summary>
        /// Temporary. Only player can have mana.
        /// </summary>
        public double Mana
        {
            get => mana;
            set
            {
                mana = Utility.MathHelper.Clamp(value, 0, maxMana);
            }
        }

        /// <summary>
        /// Temporary. Maximum mana of the player
        /// </summary>
        public double MaxMana
        {
            get => maxMana;
        }

        // --- Constructors
        public Player(
            Rectangle boundingBox, float baseSpeed, Vector2 direction, string defaultAnimation, double health, double maxHealth, string name) :
            base(boundingBox, baseSpeed, direction, defaultAnimation, health, maxHealth, name)
        {
            // Adds default dash skill to the player.
            EntitySkill dash = new EntitySkill(this, new Dash());
            AddSkill(dash);
        }

        // --- Methods ---
        public override void Update()
        {
            base.Update();
            GameTime gameTime = Game1.Singleton.GameTime;
            // TODO:
            // Regens 10 mana per second for now. Test only.
            RegenMana(Utility.MathHelper.SmoothUpdate(10, gameTime.ElapsedGameTime.TotalSeconds, 1));

            // If player is not dashing, resets the speed and direction lock.
            InputManager input = InputManager.Singleton;
            if (!(Skills["Dash"].Data as Dash).IsOn)
            {
                Speed = BaseSpeed;
                FixedDirection = false;
            }
            // If the player just pressed Left Shift (hotkey for dash), casts dash.
            if (input.IsPressed(Keys.LeftShift, false))
            {
                Cast("Dash");
            }
            // Checks direction input ONLY IF change is allowed (when the player is not passively moving)
            if (!FixedDirection)
            {
                // Re-computes new direction according to the player's input
                Vector2 newDirection = Vector2.Zero;
                // Adds vertical up component to the future direction
                if (input.IsPressing(Keys.W))
                {
                    newDirection.Y -= Speed;
                }
                // Adds horizontal left component to the future direction
                if (input.IsPressing(Keys.A))
                {
                    newDirection.X -= Speed;
                }
                // Adds vertical down component to the future direction
                if (input.IsPressing(Keys.S))
                {
                    newDirection.Y += Speed;
                }
                // Adds horizontal right component to the future direction
                if (input.IsPressing(Keys.D))
                {
                    newDirection.X += Speed;
                }
                // Normalizes the direction to be a unit vector (true direction vector)
                if (newDirection != Vector2.Zero)
                {
                    newDirection.Normalize();
                }
                Direction = newDirection;
                // Updates Facing of animation based off the direction of motion, IF the player is moving.
                if (Direction.X != 0)
                {
                    IsFaceLeft = Direction.X < 0;
                }
                // If is moving, sets current animation to Move
                if (Direction.Length() != 0)
                {
                    (this as IAnimatable).SetAnimation("Move", false, false);
                }
                // If not moving, sets current animation to Stand
                else
                {
                    (this as IAnimatable).SetAnimation("Stand", false, false);
                }
            }
            (this as IMoveable).Move();

            // TODO: improvements
            // Left button to attack enemies nearby within the attack range.
            if (input.IsPressed(MouseButtons.LeftButton) && ActionState != "Attack")
            {
                Attack(null);
                ActionState = new ActionState("Attack", this, 500).RegisterTransition((subject) =>
                {
                    Damage(EntityManager.Singleton.GetNearest(this, distance: 180), damage: Stats[EntityStats.Attack]);
                    ActionState = null;
                }
                );
            }
        }

        /// <summary>
        /// Regenerates the player's mana.
        /// </summary>
        /// <param name="amount"> mana regeneration per second </param>
        public void RegenMana(double amount)
        {
            Mana += amount;
        }

        /// <summary>
        /// Reduces the player's mana when the player uses it.
        /// </summary>
        /// <param name="amount"> How much mana used </param>
        /// <param name="cause"> Cause of mana use </param>
        public void UseMana(double amount, string cause)
        {
            Mana -= amount;
            Logger.Log($"Player used {amount} mana due to {cause}");
        }
    }
}
