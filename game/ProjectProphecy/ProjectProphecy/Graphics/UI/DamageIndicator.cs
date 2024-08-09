using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Newtonsoft.Json.Linq;
using ProjectProphecy.Map;
using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.Graphics.UI
{
    public class DamageIndicator : UIComponent, IMoveable
    {
        private static Random rnd = new Random();
        // --- Fields ---
        private double damage;
        private DateTimer timer;
        private LivingEntity attacker;
        private LivingEntity attacked;
        protected Rectangle boundingBox;
        protected Rectangle previousBoundingBox;
        protected Vector2 direction;
        private float originalSpeed;
        protected float speed;

        // --- Properties ---
        public DateTimer Timer
        {
            get => timer;
        }

        #region IMoveable
        public Rectangle BoundingBox
        {
            get => boundingBox;
            set => boundingBox = value;
        }

        public Point Location
        {
            get => boundingBox.Location;
            set => boundingBox.Location = value;
        }

        public Point Size
        {
            get => boundingBox.Size;
            set => boundingBox.Size = value;
        }

        public Room Room
        {
            get; set;
        }

        public Rectangle PreviousBoundingBox
        {
            get => previousBoundingBox;
            set => previousBoundingBox = value;
        }

        public float Speed
        {
            get => speed;
            set => speed = value;
        }

        public Vector2 Direction
        {
            get => direction;
            set
            {
                if (FixedDirection)
                {
                    return;
                }
                Vector2 newDirection = value;
                // Program-wise: normalize only when vector is not zero to avoid 0/0 = NaN
                // Math-wise: The zero vector has no length, so it need not and cannot be normalized
                if (newDirection != Vector2.Zero)
                {
                    newDirection.Normalize();
                }
                direction = newDirection;
            }
        }

        public bool FixedDirection { get; set; }
        #endregion

        // --- Constructors ---
        public DamageIndicator(LivingEntity attacker, LivingEntity attacked, double damage, int duration = 1500)
        {
            Location = attacked.BoundingBox.Center;
            this.attacker = attacker;
            this.attacked = attacked;
            this.damage = damage;
            timer = new DateTimer(duration);
            UIManager.Singleton.RegisterComponent(this);
            timer.OnEnd += () => { UIManager.Singleton.UnregisterComponent(this); };

            direction = (attacked.Location - attacker.Location).ToVector2();
            direction = direction.Rotate(MathHelper.ToRadians(rnd.Next(-60, 60)));
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }

            originalSpeed = (float)(2 + rnd.NextDouble() * 2);
        }

        public override void Update(GameTime gameTime)
        {
            if (!timer.CheckValid(false))
            {
                speed = (float)(timer.TimeLeft / timer.Duration) * originalSpeed;
                (this as IMoveable).Move();
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Computes approriate location for the text
            Rectangle dmgTextLoc = BoundingBox;
            string dmgText = $"{damage:0}";
            dmgTextLoc.Location += Utility.AlignString(dmgText, "General", 1.5f, 1.5f).ToPoint();
            UIManager.Singleton.GetText($"DmgText", "General", dmgTextLoc.X, dmgTextLoc.Y, 0, 0.75f, 0.75f, Color.Red, dmgText).Draw(Game1.Singleton.SpriteBatch);
        }
    }
}
