using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectProphecy.ns_Controls;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Skill;
using ProjectProphecy.ns_Utility;
using ProjectProphecy.ns_Entity;
using MonoGame.Extended;

namespace ProjectProphecy.ns_Entity
{
    public class Projectile : LivingEntity
    {
        public delegate void OnEnd();
        public delegate void OnHit();

        // --- Fields ---
        protected LivingEntity caster;    // Caster of the projectile
        protected LivingEntity target;    // Target of the projectile
        protected float acceleration;     // Acceleration of projectile. Only effects speed, not direction
        protected float radius;           // Enemy within radius will get hit
        protected CircleF collisionArea;  // Area within which other entities would trigger the projectile
        protected double maxDuration      // How long the projectile can last before it hits anything or expires
                                 = 5;
        protected double currentDuration; // How long the projectile has lasted
        protected float maxRange          // How far the projectile can travel before it hits anything or expires
                                 = 1000;
        protected float currentRange;     // How far the projectile has travelled
        protected double damage = 5;     // Damage of the projectile
        public event OnHit OnHitEvent;    // Event executed when the projectile hits something
        public event OnEnd OnEndEvent;    // Event executed when the projectile expires

        // --- Properties ---
        public LivingEntity Caster
        {
            get => caster;
        }

        /// <summary>
        /// Do use this to get Collision Area because it updates the center every time.
        /// </summary>
        public CircleF CollisionArea
        {
            get
            {
                collisionArea.Center = boundingBox.Center;
                return collisionArea;
            }

            private set => collisionArea = boundingBox;
        }

        public double MaxDuration
        {
            get => maxDuration;
            set => maxDuration = value;
        }

        public float MaxRange
        {
            get => maxRange;
            set => maxRange = value;
        }

        public double Damage
        {
            get => damage;
            set => damage = value;
        }

        // --- Constructors ---
        // Creates a projectile with a specified initial direction.
        public Projectile(
            Rectangle boundingBox, float baseSpeed, Vector2 direction,
            string defaultAnimation,
            string name,
            LivingEntity caster, double health = 1, double maxHealth = 1, float acceleration = 0, float radius = -1)
            : base(boundingBox, baseSpeed, direction, defaultAnimation, health, maxHealth, name)
        {
            this.caster = caster;
            this.acceleration = acceleration;
            // Default values
            // Uses half bounding box diagonal as default radius
            if (radius <= 0)
            {
                this.radius = collisionArea.Radius;
            }
        }

        // Creates a projectile initially towards given target.
        public Projectile(
            Rectangle boundingBox, float baseSpeed, LivingEntity target,
            string defaultAnimation,
            string name,
            LivingEntity caster, double health = 1, double maxHealth = 1, float acceleration = 0, float radius = -1)
            : this(boundingBox, baseSpeed, default(Vector2), defaultAnimation, name, caster, health, maxHealth, acceleration, radius)
        {
            SetTarget(target);
        }

        // --- Methods ---
        /// <summary>
        /// Updates the direction of motion towards the given target (only once)
        /// </summary>
        /// <param name="target"></param>
        public virtual void SetTarget(LivingEntity target)
        {
            this.target = target;
            Direction = (target.Location - Location).ToVector2();
        }

        public override void Update()
        {
            if (!isValid)
            {
                return;
            }
            // Projectiles don't need a action state. No need to call base() here.

            // Moves the projectile. If it touches the wall, ends the projectile
            if (!Move())
            {
                OnEndEvent?.Invoke();
                Die();
                return;
            }

            // Accumulates range and duration
            currentRange += (BoundingBox.Location - PreviousBoundingBox.Location).ToVector2().Length();
            currentDuration += Game1.Singleton.GameTime.ElapsedGameTime.TotalSeconds;
            // Checks validity
            if (currentDuration >= maxDuration || currentRange >= maxRange)
            {
                OnEndEvent?.Invoke();
                Die();
            }
            // If still valid, checks collision
            else
            {
                bool hasHit = false;
                List<LivingEntity> targets = EntityManager.Singleton.GetAll(this);
                for (int i = 0; i < targets.Count; i++)
                {
                    LivingEntity enemy = targets[i];
                    if (!CollidesWith(enemy))
                    {
                        targets.RemoveAt(i++);
                    }
                    else
                    {
                        hasHit = true;
                    }
                }
                if (hasHit)
                {
                    Damage(targets, damage);
                    OnHitEvent?.Invoke();
                    OnEndEvent?.Invoke();
                    Die();
                }
            }
            // Applies acceleration to the current velocity
            Speed += acceleration;
        }

        public bool CollidesWith(LivingEntity target)
        {
            return CollisionArea.Intersects((BoundingRectangle)target.BoundingBox);
        }

        public bool Move()
        {
            IMoveable projectile = this;

            PreviousBoundingBox = BoundingBox;
            Point location = BoundingBox.Location;
            location.X += (int)projectile.Velocity.X;
            location.Y += (int)projectile.Velocity.Y;
            BoundingBox = new Rectangle(location, BoundingBox.Size);
            if (projectile.IsOutOfBounds())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
