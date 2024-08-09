using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Controls;
using ProjectProphecy.ns_Skill;
using ProjectProphecy.ns_Utility;
using MonoGame.Extended;
using ProjectProphecy.Entity.Stats;

namespace ProjectProphecy.ns_Entity
{
    // An enemy that would chase and attack the player actively. TODO: extends LivingEntity.
    public class Enemy : LivingEntity
    {
        //Circle around enemy for agro, if the player is within this circle then the enemy should move towards the player
        //Once the player is outside of the circle the enemy should stop moving, return to original position?
        //move back and forth in a line?
        //if player is within a certain distance of the enemy for a certain amount of time the player should take damage


        // --- Constructors ---
        public Enemy(Rectangle boundingBox, float baseSpeed, Vector2 direction, string defaultAnimation,
            double health, double maxHealth, string name) :
            base(boundingBox, baseSpeed, direction, defaultAnimation, health, maxHealth, name)
        {
        }

        public override void Update()
        {
            base.Update();
            GameTime gameTime = Game1.Singleton.GameTime;
            Player player = Game1.Singleton.Player;
            CircleF followRange = new CircleF(BoundingBox.Center, 512);
            CircleF attackRange = new CircleF(BoundingBox.Center, 100);
            // Resets direction every frame
            Direction = Vector2.Zero;

            // If the player is in the follow / agro range, but not colliding / touching with the enemy, then make the enemy move towards player. 
            if (followRange.Intersects((BoundingRectangle)player.BoundingBox) && !player.BoundingBox.Intersects(BoundingBox) && CurrentAnimation.Name != "Death")
            {
                Vector2 enemyMove = player.Location.ToVector2() - Location.ToVector2();
                if (enemyMove != Vector2.Zero)
                {
                    enemyMove.Normalize();
                }
                Direction = enemyMove;

                if (Direction.X != 0)
                {
                    IsFaceLeft = Direction.X < 0;
                }
            }
            // If is moving, sets current animation to Move
            if (Direction.Length() != 0)
            {
                (this as IAnimatable).SetAnimation("Move", false, false);
            }
            // If not moving, sets current animation to Stand
            else
            {
                (this as IAnimatable).SetAnimation("Stand", false, false);
            }
            (this as IMoveable).Move();

            // If the player is in the attack range, attempt to attack the player!
            if (attackRange.Intersects((BoundingRectangle)player.BoundingBox) && ActionState != "Attack")
            {
                Stats.TryGetValue(EntityStats.Attack, out double damage);
                if (damage == default)
                {
                    damage = 10;
                }
                Attack(null);
                Damage(new List<LivingEntity> { player }, damage);
                ActionState = new ActionState("Attack", this, 1250).RegisterTransition((subject) => { ActionState = null; });
            }
        }
    }
}
