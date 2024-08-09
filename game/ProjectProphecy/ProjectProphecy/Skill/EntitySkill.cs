using ProjectProphecy.ns_Event;
using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Skill;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectProphecy.ns_Skill
{
    /// <summary>
    /// Different statuses of whether a skill can be cast
    /// </summary>
    public enum SkillStatus
    {
        OnCooldown,
        MissingMana,
        Ready
    }
    /// <summary>
    /// An available skill that is held by an entity. Includes details on skill use.
    /// Represents an entity's mastery of a skill.
    /// </summary>
    public class EntitySkill
    {
        // --- Fields ---
        private Skill skill;
        private LivingEntity entity;
        private long cooldown;
        private int level;

        // --- Properties ---
        /// <summary>
        /// If the skill is usable. some entities may have a skill set with random ones usable.
        /// Mainly for the player skill. Imagine a skill tree with unlocked skills.
        /// </summary>
        public bool IsUnlocked
        {
            get => level > 0;
        }

        /// <summary>
        /// Data of the skill. Read-only.
        /// </summary>
        public Skill Data
        {
            get => skill;
        }

        /// <summary>
        /// Owner of the skill.
        /// </summary>
        public LivingEntity Owner
        {
            get => entity;
        }

        /// <summary>
        /// Current skill level
        /// </summary>
        public int Level
        {
            get => level;
            set => level = value;
        }

        /// <summary>
        /// How much mana is required and reduced to cast the skill
        /// </summary>
        public double ManaCost
        {
            get => skill.GetManaCost(level);
        }

        /// <summary>
        /// Whether the skill is on cooldown so that it cannot be cast.
        /// </summary>
        public bool IsOnCooldown
        {
            get => cooldown > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Whether the skill has reached its max level.
        /// </summary>
        public bool IsMaxed
        {
            get => level >= skill.MaxLevel;
        }

        /// <summary>
        /// Gets the skill cooldown in milliseconds.
        /// </summary>
        public double Cooldown
        {
            get
            {
                if (IsOnCooldown)
                {
                    return (cooldown - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) / 1000.0;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets the cast validity of the skill.
        /// </summary>
        public SkillStatus Status
        {
            get
            {
                // See if it is on cooldown
                if (IsOnCooldown)
                {
                    return SkillStatus.OnCooldown;
                }

                // If caster is the player, check if it has enough mana
                if (entity is Player)
                {
                    Player player = (Player)entity;
                    if (player.Mana < ManaCost)
                    {
                        return SkillStatus.MissingMana;
                    }
                }

                // The skill is available when both off cooldown and when there's enough mana
                return SkillStatus.Ready;
            }
        }
        // --- Constructors ---
        /// <summary>
        /// Constructs a new EntitySkill. You can get instances through an IEntity object.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="skill"></param>
        public EntitySkill(LivingEntity entity, Skill skill)
        {
            this.entity = entity;
            this.skill = skill;
        }

        // --- Methods ---
        /// <summary>
        /// Adds levels to the skill. This will not update passive
        /// effects.To level up/down the skill properly, use the
        /// upgrade and downgrade methods in LivingEntity.
        /// </summary>
        /// <param name="amount"></param>
        public void AddLevels(int amount)
        {
            level = Math.Min(level + amount, skill.MaxLevel);
        }
        /// <summary>
        /// Starts the cooldown of the skill. Uses Unix timestamps for current and end time,
        /// so there's no need of a cooldown timer. Even if a timer can do something when
        /// getting updated, an update method also does the trick and more readable.
        /// </summary>
        public void StartCooldown()
        {
            long cd = (long)(skill.Cooldown * 1000);
            cooldown = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + cd;
        }

        /// <summary>
        /// Refreshes the cooldown of the skill, allowing the entity to cast the skill again.
        /// </summary>
        public void RefreshCooldown()
        {
            cooldown = 0;
        }
    }
}
