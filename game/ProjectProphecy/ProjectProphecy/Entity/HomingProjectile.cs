using Microsoft.Xna.Framework;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Entity
{
    /// <summary>
    /// Projectile that tracks a certain target (i.e. a missile)
    /// </summary>
    public class HomingProjectile : Projectile
    {
        // --- Fields ---
        // Sets the "turning-rate" of the missile. Lower values make the missile turn around faster.
        // Use big numbers (10-100) when trying to make your missiles turn slowly.
        // 0 makes the projectile straight towards its target.
        private float inertia;

        // --- Constructors ---
        // Creates a homing projectile with a starting direction and no initial target.
        public HomingProjectile(
            Rectangle boundingBox, float baseSpeed, Vector2 direction,
            string defaultAnimation,
            string name,
            LivingEntity caster, double health = 1, double maxHealth = 1, float acceleration = 0, float radius = -1, float inertia = 1.5f)
            : base(boundingBox, baseSpeed, direction, defaultAnimation, name, caster, health, maxHealth, acceleration, radius)
        {
            this.inertia = inertia;
        }

        // Creates a homing projectile with a target set already.
        public HomingProjectile(
            Rectangle boundingBox, float baseSpeed, LivingEntity target,
            string defaultAnimation,
            string name,
            LivingEntity caster, double health = 1, double maxHealth = 1, float acceleration = 0, float radius = -1, float inertia = 1.5f)
            : this(boundingBox, baseSpeed, default(Vector2), defaultAnimation, name, caster, health, maxHealth, acceleration, radius, inertia)
        {
            this.target = target;
            TrackTarget();
        }

        /// <summary>
        /// Updates the current target tracked
        /// </summary>
        /// <param name="target"></param>
        public override void SetTarget(LivingEntity target)
        {
            this.target = target;
        }

        public override void Update()
        {
            base.Update();
            if (target != null)
            {
                TrackTarget();
            }

        }

        /// <summary>
        /// Updates the direction every frame to follow the target
        /// </summary>
        private void TrackTarget()
        {
            Vector2 newDirection = Direction * inertia + (target.Location - Location).ToVector2() * ((float)Game1.Singleton.GameTime.ElapsedGameTime.TotalMilliseconds / 1000);
            if (newDirection != Vector2.Zero)
            {
                newDirection.Normalize();
            }
            Direction = newDirection;
        }
    }
}
