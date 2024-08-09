using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectProphecy.ns_Controls;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Skill;
using ProjectProphecy.ns_Utility;
using ProjectProphecy.ns_Entity;
using System.Threading;
using MonoGame.Extended;
using System.Threading.Tasks;

namespace ProjectProphecy.Entity
{
    public class Runevark : Enemy
    {
        private bool hasCast;
        private List<Projectile> attackList;

        // --- Constructors
        public Runevark(Rectangle boundingBox, float baseSpeed, Vector2 direction, string defaultAnimation,
            double health, double maxHealth, string name) :
            base(boundingBox, baseSpeed, direction, defaultAnimation, health, maxHealth, name)
        {
            hasCast = false;
            attackList = new List<Projectile>();
        }

        //Projectile from livingentity
        //projectile object in here
        //setanimation on the projectile
        //give a direction for the projectile to follow
        public void LightningSpearCast()
        {
            // Casts new spells
            Game1.Singleton.AddTask(new Task(async () =>
            {
                for (int i = 0; i < 2; i++)
                {
                    Point center = BoundingBox.Center;
                    HomingProjectile leftLightningSpear = new HomingProjectile(
                        new Rectangle(center.X - 256, center.Y - 32, 8, 32),
                        4,
                        Vector2.Zero,
                        "Stand",
                        "LightningSpear",
                        this,
                        inertia: 20f, acceleration: 0.02f)
                    { Scale = 4, MaxDuration = 3 };
                    leftLightningSpear.Damage = 2;
                    leftLightningSpear.OnEndEvent += () => { attackList.Remove(leftLightningSpear); };
                    attackList.Add(leftLightningSpear);
                    EntityManager.Singleton.Register(Room, leftLightningSpear);

                    HomingProjectile rightLightningSpear = new HomingProjectile(
                        new Rectangle(center.X + 256, center.Y - 32, 8, 32),
                        4,
                        Vector2.Zero,
                        "Stand",
                        "LightningSpear",
                        this,
                        inertia: 20f, acceleration: 0.02f)
                    { Scale = 4, MaxDuration = 5 };
                    rightLightningSpear.Damage = 2;
                    rightLightningSpear.OnEndEvent += () => { attackList.Remove(rightLightningSpear); };
                    attackList.Add(rightLightningSpear);
                    EntityManager.Singleton.Register(Room, rightLightningSpear);

                    await Task.Delay(500);
                    if (leftLightningSpear != null)
                    {
                        leftLightningSpear.SetTarget(Game1.Singleton.Player);
                    }
                    if (rightLightningSpear != null)
                    {
                        rightLightningSpear.SetTarget(Game1.Singleton.Player);
                    }
                }
            }));
        }

        public void FireballCast()
        {
            Player player = Game1.Singleton.Player;
            // Computes the direction from Runevark to player
            Vector2 displacement = (player.Location - Location).ToVector2();
            Random rnd = new Random();
            int fireballNum = rnd.Next(12, 18);
            float rotationPerStep = (MathHelper.ToRadians(270 / (fireballNum - 1)));
            displacement = displacement.Rotate(MathHelper.ToRadians(-135));
            int interval = 2000 / fireballNum;
            Game1.Singleton.AddTask(new Task(async () =>
            {
                for (int i = 0; i < fireballNum; i++)
                {
                    Projectile fireball = new Projectile(
                            new Rectangle(BoundingBox.Center + (displacement * 0.1f).ToPoint(), new Point(20, 28)),
                            0.5f,
                            displacement.NormalizedCopy(),
                            "Stand",
                            "Fireball",
                            this,
                            radius: 60, acceleration: 0.2f)

                    { MaxDuration = 2, Scale = 3.5f };
                    fireball.Damage = 4;
                    fireball.OnEndEvent += () => { attackList.Remove(fireball); };
                    attackList.Add(fireball);
                    EntityManager.Singleton.Register(Room, fireball);
                    displacement = displacement.Rotate(rotationPerStep);
                    await Task.Delay(interval);
                }
            }));
        }

        public void ShockWaveCast()
        {
            Player player = Game1.Singleton.Player;
            // Computes the direction from Runevark to player
            Game1.Singleton.AddTask(new Task(async () =>
           {
               Vector2 displacement = new Vector2(200, 200);
               int shockwaves = 9;
               float distanceMultiplier = 1f;
               // 3 times of shockwave casts
               for (int times = 0; times < 3; times++)
               {
                   // Each time n*9 shockwaves
                   float rotationPerStep = MathHelper.ToRadians(360 / (shockwaves - 1));
                   for (int i = 0; i < shockwaves; i++)
                   {
                       Rectangle shockwaveRect = new Rectangle(BoundingBox.Location, new Point(0, 0));
                       shockwaveRect.Location = Utility.AlignCenters(BoundingBox, shockwaveRect).ToPoint();
                       shockwaveRect.Location += (displacement * distanceMultiplier).ToPoint();

                       Projectile shockwave = new Projectile(
                               shockwaveRect,
                               0,
                               Vector2.Zero,
                               "Stand",
                               "Shockwave",
                               this,
                               radius: 60)
                       { MaxDuration = 1.5, Scale = 4f };

                       shockwave.OnEndEvent += () => { attackList.Remove(shockwave); };
                       shockwave.Damage = 3;
                       attackList.Add(shockwave);
                       EntityManager.Singleton.Register(Room, shockwave);
                       displacement = displacement.Rotate(rotationPerStep);
                   }
                   await Task.Delay(750);
                   shockwaves += shockwaves;
                   distanceMultiplier += 0.4f;
               }
           }));
        }

        public void CastUpdate()
        {
            if (attackList.Count == 0)
            {
                hasCast = false;
            }
        }

        public override void Update()
        {
            // Runevark  is special - does not need and should not use Update in Enemy.
            // base.Update();
            Player player = Game1.Singleton.Player;
            if (!hasCast)
            {
                Random rng = new Random();
                switch (rng.Next(3))
                {
                    case 0:
                        ShockWaveCast();
                        break;
                    case 1:
                        LightningSpearCast();
                        break;
                    case 2:
                        FireballCast();
                        break;
                }
                hasCast = true;
            }
            else
            {
                CastUpdate();
            }
        }
    }
}
