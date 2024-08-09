using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Entity
{
    /// <summary>
    /// Has health. Is damageable.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Name of the object
        /// </summary>
        string Name
        {
            get; set;
        }

        /// <summary>
        /// Current health
        /// </summary>
        double Health
        {
            get; set;
        }

        /// <summary>
        /// Max health
        /// </summary>
        double MaxHealth
        {
            get; set;
        }

        /// <summary>
        /// Reduces health and process possible behavior of the damaged.
        /// </summary>
        virtual void TakeDamage(double damage)
        {
            Health -= damage;
            // Logic after being damaged
            // Entity - Counterattack?

            // Logic after health drops below 0
            if (Health <= 0)
            {
                // Entity - Death?
                // Destructible - Break?
            }
        }
    }
}
