using ProjectProphecy.ns_Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Entity
{
    public delegate void ImplicitTransitionHandler(LivingEntity entity);
    public delegate void ExplicitStateTransitionHandler(LivingEntity entity, ActionState nextState);

    /// <summary>
    /// Represents the action a living entity is doing.
    /// </summary>
    public class ActionState
    {
        // --- Fields ---
        private LivingEntity subject;                                     // Subject of the action
        private long duration;                                            // How long the action would last
        private ImplicitTransitionHandler implicitTransitionHandler;      // Does not have a nextState when called, but one in the method block
        private ExplicitStateTransitionHandler explicitTransitionHandler; // Specifically determines what the next state should be.

        // --- Properties ---
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Whether the action is still being done or in effect.
        /// </summary>
        public bool IsOnDuration
        {
            get
            {
                return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() <= duration;
            }
        }
        // --- Operators ---
        public static bool operator ==(ActionState left, string right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Name.Equals(right);
        }

        public static bool operator !=(ActionState left, string right)
        {
            return !(left == right);
        }

        // --- Constructors ---
        /// <summary>
        /// Creates and assigns a new action state for the subject
        /// </summary>
        /// <param name="name"></param>
        /// <param name="subject"></param>
        /// <param name="duration"></param>
        public ActionState(string name, LivingEntity subject, long duration)
        {
            Name = name;
            this.subject = subject;
            this.duration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + duration;
            subject.ActionState = this;
        }

        // --- Methods ---
        /// <summary>
        /// Updates the ActionState with a implicit transition handler.
        /// </summary>
        public void Update()
        {
            implicitTransitionHandler?.Invoke(subject);
        }

        /// <summary>
        /// Updates the ActionState with a explicit transition handler.
        /// </summary>
        /// <param name="nextState"></param>
        public void Update(ActionState nextState)
        {
            if (!IsOnDuration)
            {
                explicitTransitionHandler?.Invoke(subject, nextState);
            }
        }

        /// <summary>
        /// Registers an implicit transition handler into the handler list.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public ActionState RegisterTransition(ImplicitTransitionHandler handler)
        {
            implicitTransitionHandler += handler;
            return this;
        }
        /// <summary>
        /// Registers an explicit transition handler into the handler list.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public ActionState RegisterTransition(ExplicitStateTransitionHandler handler)
        {
            explicitTransitionHandler += handler;
            return this;
        }

        /// <summary>
        /// Converts to string, showing name, subject's name and if is taking effect
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name}, Owner: {subject.Name}, OnDuration: {IsOnDuration}";
        }
    }
}
