using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Skill;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace ProjectProphecy.ns_Skill
{
    /// <summary>
    /// A passive skill. Is passive in two aspects: has stat bonuses and effect when triggered by specified events.
    /// </summary>
    public interface IPassiveSkill
    {
        /// <summary>
        /// Updates the bonuses given by the skill.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="prevLevel"></param>
        /// <param name="newLevel"></param>
        void Update(LivingEntity user, int prevLevel, int newLevel);

        /// <summary>
        /// Registers the skill owner into event handlers so that when a event is handled the check includes the owner 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="level"></param>
        void Initialize(LivingEntity user, int level);

        /// <summary>
        /// Stops the effects when the owner loses/unlearns the skill.
        /// This could entail stopping tasks you use for the skill, resetting
        /// health or other stats, or other lasting effects you use.
        /// </summary>
        /// <param name="user"></param>
        /// <param name=""></param>
        void StopEffects(LivingEntity user);
    }
}
