using ProjectProphecy.ns_Event;
using ProjectProphecy.ns_Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ProjectProphecy.ns_Event
{
    /// <summary>
    /// Event for when an entity casts a skill.
    /// </summary>
    public class EntityCastSkillEvent : Event
    {
        public delegate bool Handler(LivingEntity caster, int level, List<LivingEntity> targets, EventMetadata meta);

        public static Dictionary<UniqueId, Handler> Handlers = new Dictionary<UniqueId, Handler>(); 
    }
}
