using Microsoft.Xna.Framework;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Event;
using ProjectProphecy.ns_Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ProjectProphecy.Entity;

namespace ProjectProphecy.ns_Skill
{
    /// <summary>
    /// An ability that the player has along with it. Allows to avoid attacks and be invulnerable for a short duration.
    /// </summary>
    public class Dash : Skill
    {
        public override string Name => "Dash";

        private long duration;

        public bool IsOn
        {
            get => duration > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public override void Load()
        {
            Cooldown = 0.5;
            onCast = (caster, targets, level, EventMetadata) =>
            {
                if (caster.Direction == Vector2.Zero)
                {
                    return false;
                }
                long duration = 150;
                this.duration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + duration;
                float speedBefore = caster.Speed;
                caster.Speed = caster.BaseSpeed * 3f;
                // Invincible when dashing
                caster.AddStatus(Status.Type.Invincible, 150);
                // Animation-wise process
                (caster as IAnimatable).SetAnimation("Dash", false, false);
                caster.FixedAnimation = true;
                caster.FixedFacing = true;
                caster.FixedDirection = true;
                return true;
            };
            base.Load();
        }
    }
}
