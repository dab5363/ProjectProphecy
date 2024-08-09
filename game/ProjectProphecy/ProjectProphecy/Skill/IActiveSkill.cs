using ProjectProphecy.ns_Event;
using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Skill;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Skill
{
    /// <summary>
    /// An active skill. Can be cast.
    /// </summary>
    public interface IActiveSkill
    {
        /// <summary>
        /// Let somebody cast the skill.
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="targets"></param>
        /// <param name="level"></param>
        /// <param name="meta"></param>
        /// <returns></returns>
        bool Cast(LivingEntity caster, List<LivingEntity> targets, int level, EventMetadata meta);
    }
}
