using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Event;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Skill
{
    /// <summary>
    /// Just a test skill class for learning delegates
    /// </summary>
    public class TestSkill : Skill
    {
        public override string Name => "TestSkill";

        public override void Load()
        {
            onCast = (caster, targets, level, EventMetadata) =>
            {
                Logger.Log($"Skill Test: {caster.Name}");
                return true;
            };
            base.Load();
        }
    }
}
