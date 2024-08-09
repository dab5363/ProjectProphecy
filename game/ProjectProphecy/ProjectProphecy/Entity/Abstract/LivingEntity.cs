using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Logics;
using ProjectProphecy.ns_Skill;
using ProjectProphecy.ns_Event;
using System;
using System.Collections.Generic;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using ProjectProphecy.Map;
using System.Threading;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;
using ProjectProphecy.ns_Utility;
using IDrawable = ProjectProphecy.ns_Graphics.IDrawable;
using ProjectProphecy.Graphics.UI;
using ProjectProphecy.Entity;
using ProjectProphecy.Entity.Stats;
using System.Linq;

namespace ProjectProphecy.ns_Entity
{
    /// <summary>
    /// Represents a living entity that can move, animate, attack, be attacked, has actions, stats and skills, etc.
    /// </summary>
    public abstract class LivingEntity : IMoveable, IAnimatable, IDamageable
    {
        // --- Fields ---
        // Private is right here - let child classes use the properties - they have preset logics.
        protected bool isFaceLeft;
        protected Rectangle boundingBox;
        protected Rectangle previousBoundingBox;
        protected Vector2 direction;
        protected float speed;
        protected float rotation;
        protected float scale;
        protected double health;
        protected double maxHealth;
        protected float baseSpeed;
        protected bool isValid;

        // --- Properties ---
        public string Name { get; set; }

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

        /// <summary>
        /// Direction of motion. Applies normalization when set to ensure it's a unit vector.
        /// </summary>
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

        #region IDrawable super IAnimatable
        /// <summary>
        /// Asset for IDrawable, sprite sheet for IAnimatable
        /// </summary>
        public Texture2D Asset
        {
            get; set;
        }

        /// <summary>
        /// Where the current frame is in the sprite sheet
        /// </summary>
        public Rectangle Section
        {
            get => (this as IAnimatable).CurrentFrameSection;
            set
            {
                // TODO: this setter has no use
            }
        }
        public float Rotation
        {
            get => rotation;
            set
            {
                MathHelper.ToRadians(rotation);
            }
        }
        public float Scale
        {
            get => scale;
            set
            {
                scale = value;
                boundingBox.Size = new Vector2(boundingBox.Width * value, boundingBox.Height * value).ToPoint();
            }
        }

        public bool IsVisible
        {
            get; set;
        } = true;
        #endregion

        #region IAnimatable
        public bool IsFaceLeft
        {
            get => isFaceLeft;
            set
            {
                if (!FixedFacing)
                {
                    isFaceLeft = value;
                }
            }
        }
        public bool FixedFacing { get; set; }
        public Dictionary<string, Animation> Animations { get; set; }
        public Animation DefaultAnimation { get; set; }
        public Animation CurrentAnimation { get; set; }
        public bool FixedAnimation { get; set; }
        public DateTime AnimationStartTime { get; set; }
        public double TimeCounter { get; set; }
        public int CurrentFrame { get; set; }
        public double FPS { get; set; }
        #endregion

        #region IDamageable
        /// <summary>
        /// One of the initial stats that will be moved into Stats
        /// </summary>
        public double Health
        {
            get => health;
            set => health = value;
        }
        /// <summary>
        /// One of the initial stats that will be moved into Stats
        /// </summary>
        public double MaxHealth
        {
            get => maxHealth;
            set => maxHealth = value;
        }
        #endregion

        #region Own
        /// <summary>
        /// State representing the entity's current action.
        /// </summary>
        public ActionState ActionState
        {
            get; set;
        } = null;

        /// <summary>
        /// If the living entity is dead.
        /// </summary>
        public bool IsDead
        {
            get => Health <= 0;
        }

        /// <summary>
        /// Returns false if the entity is dying, has died or been despawned for some other reason.
        /// </summary>
        public bool IsValid
        {
            get => isValid;
            set => isValid = value;
        }

        // TODO: Will not be in this form in late development, replaced by EntityStats type.
        public Dictionary<EntityStats, double> Stats
        {
            get;
            set;
        } = new Dictionary<EntityStats, double>();

        /// <summary>
        /// One of the initial stats that will be moved into Stats
        /// </summary>
        public float BaseSpeed
        {
            get => baseSpeed;
        }

        /// <summary>
        /// Skills the living entity is holding.
        /// </summary>
        public Dictionary<string, EntitySkill> Skills
        {
            get;
        } = new Dictionary<string, EntitySkill>(StringComparer.OrdinalIgnoreCase);

        public HashSet<Status> Statuses
        {
            get;
        } = new HashSet<Status>();

        #endregion

        // --- Constructors ---
        /// <summary>
        /// Instantiates a living entity with several initial stats.
        /// Will load the config file instead, using Settings class and future implemented FileIO.
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <param name="baseSpeed"></param>
        /// <param name="direction"></param>
        /// <param name="defaultAnimation"></param>
        /// <param name="health"></param>
        /// <param name="maxHealth"></param>
        /// <param name="name"></param>
        public LivingEntity(
            Rectangle boundingBox, float baseSpeed, Vector2 direction,
            string defaultAnimation,
            double health, double maxHealth,
            string name)
        {
            Name = name;
            BoundingBox = boundingBox;
            speed = baseSpeed;
            this.baseSpeed = baseSpeed;
            Direction = direction;
            Health = health;
            MaxHealth = maxHealth;
            AnimationManager.Singleton.RegisterAnimation(Name, defaultAnimation, this);
            Asset = DefaultAnimation.SpriteSheet;

            // Default values
            scale = 1;
            isValid = true;
        }

        // --- Methods ---
        /// <summary>
        /// Adds an EntitySkill to the Entity's skill list.
        /// </summary>
        /// <param name="skill"> The skill to add to the entity </param>
        /// <returns> If the skill was successfully added. False if the entity already had it. </returns>
        public bool AddSkill(EntitySkill skill)
        {
            return Skills.TryAdd(skill.Data.Name, skill);
        }

        /// <summary>
        /// Checks if the owner has a skill by name. This is not case-sensitive
        /// and does not check to see if the skill is unlocked. It only checks if
        /// the skill is available to upgrade/use.
        /// </summary>
        /// <param name="name"> Name of the skill </param>
        /// <returns> True if has the skill, false otherwise </returns>
        public bool HasSkill(string name)
        {
            return name != null && Skills.ContainsKey(name);
        }

        /// <summary>
        /// Retrieves a skill of the owner by name. This is not case-sensitive.
        /// </summary>
        /// <param name="name"> Name of the skill </param>
        /// <returns> Data for the skill or null if the entity doesn't have the skill </returns>
        public EntitySkill GetSkill(string name)
        {
            if (name == null || !Skills.ContainsKey(name))
            {
                return null;
            }
            return Skills[name];
        }

        /// <summary>
        /// Retrieves the level of a skill for the owner. This is not case-sensitive.
        /// </summary>
        /// <param name="name"> Name of the skill </param>
        /// <returns> Level of the skill or 0 if not found </returns>
        public int GetSkillLevel(string name)
        {
            EntitySkill skill = GetSkill(name);
            return skill == null ? 0 : skill.Level;
        }

        /// <summary>
        /// Cast a skill the entity has. In order to cast the skill,
        /// the entity must have the skill unlocked, have enough mana (if it's player)
        /// have the skill off cooldown, and have a proper target if applicable.
        /// </summary>
        /// <param name="skill"> Skill to cast </param>
        /// <returns> True if successfully cast the skill, false otherwise </returns>
        public bool Cast(EntitySkill skill)
        {
            // Invalid skill
            if (skill == null) { throw new ArgumentNullException("Skill cannot be null"); }

            int level = skill.Level;

            // On cooldown or not enough mana
            if (!Check(skill, true, true))
            {
                return false;
            }

            // Dead entities can't cast skills.
            if (IsDead)
            {
                return false;
            }

            // Try/catch block to avoid crashes caused by error in skill config to be reported.
            try
            {
                if (skill.Data.Cast(this, new List<LivingEntity> { this }, level, new EventMetadata()))
                {
                    ApplyUse(this, skill, skill.ManaCost);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to cast skill - {skill.Data.Name}: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Casts a skill by the given skill name.
        /// </summary>
        /// <param name="skillName"></param>
        /// <returns></returns>
        public bool Cast(string skillName)
        {
            // Invalid skill name
            if (string.IsNullOrEmpty(skillName))
            {
                throw new ArgumentException("Invalid skill name");
            }
            EntitySkill skill;
            Skills.TryGetValue(skillName, out skill);
            // Invalid skill
            if (skill == null)
            {
                throw new ArgumentNullException("Skill cannot be null");
            }
            else return Cast(skill);
        }

        /// <summary>
        /// Applies cooldown and mana cost when a skill is used.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="skill"></param>
        /// <param name="manaCost"></param>
        /// <returns></returns>
        private bool ApplyUse(LivingEntity entity, EntitySkill skill, double manaCost)
        {
            skill.StartCooldown();
            if (entity is Player)
            {
                (entity as Player).UseMana(manaCost, "skill cast");
            }
            return true;
        }

        /// <summary>
        /// Checks if a skill can be used.
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="cooldown"></param>
        /// <param name="mana"></param>
        /// <returns></returns>
        public bool Check(EntitySkill skill, bool cooldown, bool mana)
        {
            if (skill == null) { return false; }

            SkillStatus status = skill.Status;
            int level = skill.Level;
            double cost = skill.Data.GetManaCost(level);

            // On Cooldown
            if (status == SkillStatus.OnCooldown && cooldown)
            {
                return false;
            }
            // Not enough mana
            else if (status == SkillStatus.MissingMana && mana)
            {
                return false;
            }
            // Skill is ready to be cast
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Search for all targets available to deals damage using given AttackLogic.
        /// Normal attacks.
        /// AttackLogic can be melee poke, melee slash, firing fireballs or shooting arrows….
        /// </summary>
        /// <param name="attack"></param>
        public void Attack(AttackLogic logic)
        {
            // TODO: if off Attack cooldown and not doing anything else (like casting)
            (this as IAnimatable).SetAnimation("Attack", false, true);
            FixedAnimation = true;
            FixedFacing = true;
            // TODO
            // LivingEntity[] targets = logic.getTargets();
        }

        /// <summary>
        /// Deals damage to the targets
        /// </summary>
        /// <param name="targets"></param>
        public void Damage(List<LivingEntity> targets, double damage = 10)
        {
            // Ignore invalid damages
            if (damage <= 0)
            {
                return;
            }
            EntityManager manager = EntityManager.Singleton;
            foreach (LivingEntity target in targets)
            {
                // TODO: Damage calculation here
                // MyAttack - TargetDefense...
                if (!manager.IsAlly(this, target) && !target.HasStatus(Status.Type.Invincible))
                {
                    (target as IDamageable).TakeDamage(damage);
                    (target as IAnimatable).SetAnimation("Hurt", true, false);
                    target.FixedAnimation = true;
                    // Creates a damage indicator
                    new DamageIndicator(this, target, damage);
                }
            }
        }

        /// <summary>
        /// Processes death - triggers death effects, does cleanup.
        /// </summary>
        public void Die()
        {
            isValid = false;
            // No death animation, just die instantly
            if (!Animations.ContainsKey("Death"))
            {
                EntityManager.Singleton.RemoveEntity(this);
            }
            // Has death animation, removal executed right after it's played
            else
            {
                (this as IAnimatable).SetAnimation("Death", false, true);
                FixedAnimation = true;
                // Entity removal is executed in OnAnimationEnd();
            }
        }

        /// <summary>
        /// Updates the player depending on the action state. If the player has no health, let it die.
        /// </summary>
        public virtual void Update()
        {
            // isValid is neccessary here - otherwise it launches multiple threads instead of one when the entity is dying.
            // It also prevents the entity from doing any action (i.e. being updated) since it's considered dying.
            if (!isValid)
            {
                return;
            }
            // Applies death if health <= 0
            if (IsDead)
            {
                Die();
            }
            // Updates statuses
            foreach (Status status in Statuses.ToList())
            {
                status.Update();
            }
            // Performs action according to ActionState
            if (ActionState != null && !ActionState.IsOnDuration)
            {
                ActionState.Update();
            }
        }

        public void OnAnimationEnd(Animation expired)
        {
            // Removes the entity on death
            if (expired.Name.Equals("Death"))
            {
                EntityManager.Singleton.RemoveEntity(this);
            }
        }

        public bool AddStatus(Status.Type type, int milliseconds)
        {
            return AddStatus(new Status(this, type, milliseconds));
        }

        public bool AddStatus(Status status)
        {
            if (HasStatus(status.GetStatusType()))
            {
                return false;
            }
            return Statuses.Add(status);
        }

        public bool RemoveStatus(Status status)
        {
            return Statuses.Remove(status);
        }

        public bool HasStatus(Status.Type type)
        {
            foreach (Status status in Statuses)
            {
                if (status.GetStatusType() == type)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

