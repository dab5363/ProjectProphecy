using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.Entity
{
    public class Status
    {
        public enum Type
        {
            Invincible
        }

        private LivingEntity entity;
        private Type type;
        private CountdownTimer timer;

        public Status(LivingEntity entity, Type type, int milliseconds)
        {
            this.entity = entity;
            this.type = type;
            timer = new CountdownTimer(milliseconds);
        }

        public Type GetStatusType()
        {
            return type;
        }

        public void Update()
        {
            timer.Update(Game1.Singleton.GameTime);
            if (timer.IsOver)
            {
                // Remove this status from status holder.
                entity.Statuses.Remove(this);
            }
        }
    }
}
