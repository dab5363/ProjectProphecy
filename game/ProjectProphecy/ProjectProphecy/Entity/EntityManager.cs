using ProjectProphecy.Map;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;

namespace ProjectProphecy.ns_Entity
{
    /// <summary>
    /// Manager class for all LivingEntity objects in all rooms.
    /// </summary>
    public class EntityManager
    {
        // --- Fields ---
        // Singleton of the manager class
        // Intro on this Singleton pattern can be found here (6th): https://csharpindepth.com/articles/singleton
        private static readonly Lazy<EntityManager> manager = new Lazy<EntityManager>(() => new EntityManager());
        // All LivingEntities in all rooms
        private readonly Dictionary<string, ConcurrentHashSet<LivingEntity>> entities = new Dictionary<string, ConcurrentHashSet<LivingEntity>>(StringComparer.OrdinalIgnoreCase);

        // --- Properties ---
        /// <summary>
        /// Use this property to call manager functions.
        /// </summary>
        public static EntityManager Singleton
        {
            get => manager.Value;
        }

        // --- Methods ---
        /// <summary>
        /// Called on game restart (soft)
        /// </summary>
        public void Reset()
        {
            entities.Clear();
        }

        /// <summary>
        /// Gets all living entities in the specified room
        /// </summary>
        /// <param name="room">Room name</param>
        /// <returns>All living entities in the room</returns>
        public List<LivingEntity> GetLivingEntities(string room)
        {
            // Registers an empty room if the room hasn't been registered.
            if (!entities.ContainsKey(room))
            {
                entities.Add(room, new ConcurrentHashSet<LivingEntity>());
            }
            return entities[room].MutableCopy().ToList();
        }

        /// <summary>
        /// Spawns an entity to the given room. Current room by default.
        /// TODO: unimplemented - should spawn an entity with its EntityData
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool SpawnEntity(LivingEntity entity)
        {
            return Register(entity);
        }

        /// <summary>
        /// Directly removes an entity from its current room.
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveEntity(LivingEntity entity, bool permanent = true)
        {
            // No room with entity.Room.Name
            if (!entities.ContainsKey(entity.Room.Name))
            {
                Logger.Log($"Can't remove entity. no room registered with {entity.Name}'s current room's name");
                return;
            }
            // Entity not in room's HashSet
            if (!entities[entity.Room.Name].Contains(entity))
            {
                Logger.Log($"Entity {entity.Name} to remove is not in its current room's entity HashSet");
                return;
            }

            if (permanent)
            {
                (entity as IDrawable).IsVisible = false;
                entity.IsValid = false;
            }
            entities[entity.Room.Name].Remove(entity);
        }

        /// <summary>
        /// Simply kills an entity by calling entity.Die().
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool KillEntity(LivingEntity entity)
        {
            entity.Die();
            return true;
        }

        /// <summary>
        /// Updates all living entities in the current room;
        /// </summary>
        public void UpdateAll()
        {
            Update(RoomManager.Singleton.CurrentRoom.Name);
        }

        /// <summary>
        /// Updates all living entities in the specified room.
        /// </summary>
        /// <param name="room"></param>
        public void Update(string room)
        {
            foreach (LivingEntity entity in GetLivingEntities(room))
            {
                Update(entity);
            }
        }

        /// <summary>
        /// Manually Updates an LivingEntity.
        /// </summary>
        /// <param name="key"></param>
        public void Update(LivingEntity entity)
        {
            entity.Update();
        }

        /// <summary>
        /// Registers an entity into the specified room.
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="entity"></param>
        public bool Register(string roomName, LivingEntity entity)
        {
            // No room with specified name
            if (!RoomManager.Singleton.Rooms.ContainsKey(roomName))
            {
                return false;
            }

            // Room not initialized in the dictionary
            if (!entities.ContainsKey(roomName))
            {
                // Adds a new entry to the dictionary starting with only the entity in the HashSet.
                entities.Add(roomName, new ConcurrentHashSet<LivingEntity>(entity));
                // Updates the entity's current room
                entity.Room = RoomManager.Singleton.Rooms[roomName];
                return true;
            }

            // Already registered
            if (entities[roomName].Contains(entity))
            {
                return false;
            }

            // Adds the entity to the room's entity HashSet.
            entities[roomName].Add(entity);
            entity.Room = RoomManager.Singleton.Rooms[roomName];
            return true;
        }

        public bool Register(Room room, LivingEntity entity)
        {
            return Register(room.Name, entity);
        }

        /// <summary>
        /// Registers an entity into the current room
        /// </summary>
        /// <param name="entity"></param>
        public bool Register(LivingEntity entity)
        {
            return Register(Game1.Singleton.CurrentRoom.Name, entity);
        }

        /// <summary>
        /// Get the nearest entities of the source entity in the current room.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public List<LivingEntity> GetNearest(LivingEntity source, bool includeSource = false, float distance = 100, bool getAlly = false, bool getEnemy = true)
        {
            List<LivingEntity> targets = new List<LivingEntity>();
            foreach (LivingEntity entity in GetLivingEntities(Game1.Singleton.CurrentRoom.Name))
            {
                // Do not select the source unless includeSource is true
                if (entity == source && !includeSource)
                {
                    continue;
                }
                // If the entity is within certain range, add it to the target list
                // Also needs the source and target's relationship to satisfy the requirements
                if (getAlly && IsAlly(source, entity) || getEnemy && !IsAlly(source, entity))
                {
                    if ((source.BoundingBox.Center - entity.BoundingBox.Center).ToVector2().Length() <= distance)
                    {
                        targets.Add(entity);
                    }
                }
            }
            return targets;
        }

        /// <summary>
        /// Returns all entities that meets the ally/enemy relationship requirements with the source in the current room.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="includeSource"></param>
        /// <param name="getAlly"></param>
        /// <param name="getEnemy"></param>
        /// <returns></returns>
        public List<LivingEntity> GetAll(LivingEntity source, bool includeSource = false, bool getAlly = false, bool getEnemy = true)
        {
            List<LivingEntity> targets = new List<LivingEntity>();
            string room = Game1.Singleton.CurrentRoom.Name;
            // The room hasn't been registered with any entities.
            if (!this.entities.ContainsKey(room))
            {
                return targets;
            }
            List<LivingEntity> entities = GetLivingEntities(room);
            foreach (LivingEntity entity in entities)
            {
                // Do not select the source unless includeSource is true
                if (entity == source && !includeSource)
                {
                    continue;
                }
                // Adds the entity to the target list once the source and itself's relationship satisfies the requirements
                if (getAlly && IsAlly(source, entity) || getEnemy && !IsAlly(source, entity))
                {
                    targets.Add(entity);
                }
            }
            return targets;
        }

        /// <summary>
        /// If the target is an ally or enemy to the subject entity. 
        /// It's a communal relationship, so vice versa.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsAlly(LivingEntity entity, LivingEntity target)
        {
            // If the trigger entity is a projectile, use its caster to determine validity.
            if (entity is Projectile)
            {
                entity = (entity as Projectile).Caster;
                if (entity == null)
                {
                    return false;
                }
            }
            // Well, it may sound cool if we take ourselves as enemies; But one is literally oneself's ally.
            if (entity == target)
            {
                return true;
            }
            // Attack between Player & Enemy
            if (entity is Player && target is Enemy || entity is Enemy && target is Player)
            {
                return false;
            }
            // Attack between Player & NPC
            if (entity is Player && target is NPC || entity is NPC && target is NPC)
            {
                return true;
            }
            // Attack between Enemy & Enemy
            if (entity is Enemy && target is Enemy)
            {
                return true;
            }
            // Attack between Enemy & NPC
            if (entity is Enemy && target is NPC || entity is NPC && target is Enemy)
            {
                return false;
            }
            // In other cases, all living entities are by default allies / friendly to each other.
            return true;
        }
    }
}
