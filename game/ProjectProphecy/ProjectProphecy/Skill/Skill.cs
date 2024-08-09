using ProjectProphecy.ns_Event;
using ProjectProphecy.ns_Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using ProjectProphecy.ns_IO;
using ProjectProphecy.ns_Utility;

namespace ProjectProphecy.ns_Skill
{
    /// <summary>
    /// A skill that can be either active, passive or both and used by any entity.
    /// The mana/stamina cost only works for the player. For NPCs and enemies, only
    /// the cooldown counts.
    /// Represents the template of a skill.
    /// </summary>
    public abstract class Skill : IActiveSkill, IPassiveSkill
    {
        // --- Fields ---
        private int maxLevel;
        protected EntityCastSkillEvent.Handler onCast;
        protected readonly Settings settings = new Settings();

        // --- Properties ---
        /// <summary>
        /// Name of the skill.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Unique ID to distinguish the skill.
        /// </summary>
        public UniqueId UUID
        {
            get;
        } = new UniqueId();

        public double Cooldown
        {
            get; set;
        }

        public int MaxLevel
        {
            get => MaxLevel;
        }

        // --- Constructors ---
        public Skill()
        {
            Load();
            // TODO: File IO
        }

        // --- Methods ---
        public double GetManaCost(int level)
        {
            return 10;
        }

        private bool Trigger(LivingEntity user, List<LivingEntity> targets, int level, EventMetadata meta, Event e)
        {
            return true;
        }

        public bool Cast(LivingEntity caster, List<LivingEntity> targets, int level, EventMetadata meta)
        {
            return (bool)EntityCastSkillEvent.Handlers[UUID]?.Invoke(caster, level, targets, meta);
        }

        public void Update(LivingEntity user, int prevLevel, int newLevel)
        {
            throw new NotImplementedException();
        }

        public void Initialize(LivingEntity user, int level)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the skill data to the configuration, overwriting all previous data
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Loads the function module into the skill.
        /// Since the module class has not been implemented and is tough work,
        /// for now, just add child classes and skill code in this function.
        /// </summary>
        public virtual void Load()
        {
            if (onCast == null)
            {
                Logger.Log($"Skill {Name} has no function module to execute. Skills need to override Load() with necessary functionality");
                throw new NotImplementedException($"Skill {Name} has no function module to execute. Skills need to override Load() with necessary functionality");
            }
            EntityCastSkillEvent.Handlers[UUID] = onCast;
        }

        public void StopEffects(LivingEntity user)
        {
            throw new NotImplementedException();
        }
    }
}
