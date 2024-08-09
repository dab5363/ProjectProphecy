using Microsoft.Xna.Framework;
using ProjectProphecy.Map;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ProjectProphecy.ns_Entity
{
    /// <summary>
    /// Boundary names of the sides of the screen******NOW ROOM!!!! 
    /// TODO: Add a RoomBoundary enum which is exactly what it does now, and use this ScreenBoundary in Room
    /// </summary>
    public enum ScreenBoundary
    {
        Left,
        Right,
        Top,
        Bottom
    }
    public interface IMoveable : IEntity
    {
        /// <summary>
        /// Current speed. Is DIFFERENT from a LivingEntity(or maybe other classes in the future)'s speed stat.
        /// An entity can have a very high speed (speed stat for original speed) but is moving slowly (debuffed). 
        /// </summary>
        float Speed
        {
            get; set;
        }

        Rectangle PreviousBoundingBox
        {
            get; set;
        }

        /// <summary>
        /// Where the IMoveable was in the last frame.
        /// </summary>
        sealed Point PreviousLocation
        {
            get => PreviousBoundingBox.Location;
        }

        /// <summary>
        /// Direction of motion. Applies normalization when set to ensure it's a unit vector.
        /// </summary>
        Vector2 Direction
        {
            get; set;
        }

        /// <summary>
        /// When the direction is fixed, the direction can't/shouldn't be set.
        /// </summary>
        bool FixedDirection
        {
            get; set;
        }

        /// <summary>
        /// Velocity of motion. Read-only.
        /// </summary>
        sealed Vector2 Velocity
        {
            get => Direction * Speed;
        }

        /// <summary>
        /// If the IMoveable is moving. Checks direction. A zero vector means False.
        /// </summary>
        sealed bool IsMoving
        {
            get => Direction.Length() != 0;
        }

        /// <summary>
        /// Process movement using current Velocity
        /// </summary>
        sealed void Move()
        {
            Rectangle previousBoundingBoxCopy = PreviousBoundingBox;
            PreviousBoundingBox = BoundingBox;
            Point location = BoundingBox.Location;
            location.X += (int)Velocity.X;
            location.Y += (int)Velocity.Y;
            BoundingBox = new Rectangle(location, BoundingBox.Size);
            // Very very basic and cheeky collision detection! Cancels movement if it will collide with anything
            // Literally sadistic implementation
            // Should add another fixlocation for collision
            Room currentRoom = RoomManager.Singleton.CurrentRoom;

            // Currently Buggy
            //foreach (Tile tile in currentRoom.Tiles.Values)
            //{
            //    if (!tile.Passable && BoundingBox.Intersects(tile.BoundingBox))
            //    {
            //        // Simply cancels movement instead of fixing it
            //        BoundingBox = previousBoundingBoxCopy;
            //        return;
            //    }
            //}
            //foreach (LivingEntity entity in EntityManager.Singleton.GetLivingEntities(currentRoom.Name))
            //{
            //    // ignore projectiles
            //    if (entity is Projectile)
            //    {
            //        continue;
            //    }
            //    if (BoundingBox.Intersects(entity.BoundingBox))
            //    {
            //        BoundingBox = previousBoundingBoxCopy;
            //        return;
            //    }
            //}
            // Out of bounds check
            if (IsOutOfBounds())
            {
                FixLocation();
            }
        }

        /// <summary>
        /// Process movement towards a location, using given speed (like when it's displaced by certain skills).
        /// I don't know if we really need this. Because we can always change the entity's current Speed and Dicretion.
        /// </summary>
        sealed void Move(int x, int y, float speed)
        {
            PreviousBoundingBox = BoundingBox;
            Point location = Location;
            Point targetLoc = new Point(x, y);
            // Gets the direction vector from current location to target location
            Vector2 toLoc = (targetLoc - Location).ToVector2();
            toLoc.Normalize();
            location.X += (int)(toLoc.X * speed);
            location.Y += (int)(toLoc.Y * speed);
            if (IsOutOfBounds())
            {
                FixLocation();
            }
        }

        /// <summary>
        /// When the IMovable is out of bounds, fixes its location depending on its previous location.
        /// Use of current direction is an alternative but cannot cover the case of the object teleported while not moving.
        /// </summary>
        /// <returns></returns>
        sealed void FixLocation()
        {
            var (width, height) = Game1.Singleton.WindowDimensions;
            // Gets the direction vector from previous location to current location
            Vector2 toLoc = (Location - PreviousLocation).ToVector2();
            // Can't fix location when no displacement occurred.
            if (toLoc == Vector2.Zero)
            {
                return;
            }
            toLoc.Normalize();

            float tangent = toLoc.Y / toLoc.X;

            // Fixes X coordinate if it's out of bounds
            if (FixLocation_X(width, PreviousLocation, tangent))
            {
                // The fix only ensures X is in the normal range; may be Y orginally out of bounds
                // or Y came out after the fix. Therefore, an additional Y fix is necessary.
                FixLocation_Y(height, PreviousLocation, tangent);
            }
            // If X is not out of bounds, try to fix Y coordinate
            else if (FixLocation_Y(height, PreviousLocation, tangent))
            {
                // Though it is certain here that X originally was not out of bounds,
                // in case X came out after the fix, an X fix is still needed.
                FixLocation_X(width, PreviousLocation, tangent);
            }
        }

        /// <summary>
        /// Fixes X coordinate if it's out of the left or right bound.
        /// </summary>
        /// <param name="width">Width of the world</param>
        /// <param name="loc">(Previous) location to be fixed. Can be current location; just depends on how the method does it</param>
        /// <param name="tangent">Tangent of the angle forced by the direction of motion and distance to the bound</param>
        /// <returns>If X is fixed</returns>
        private bool FixLocation_X(int width, Point loc, float tangent)
        {
            // TODO: Temporary
            Room room = Game1.Singleton.CurrentRoom;
            bool hasFixed = false;
            if (IsOutOfBound(ScreenBoundary.Left) || IsOutOfBound(ScreenBoundary.Right))
            {
                int xOffset =
                    IsOutOfBound(ScreenBoundary.Left) ?
                     room.Self.Left - PreviousBoundingBox.Left : room.Self.Right - PreviousBoundingBox.Right;
                loc.X += xOffset;
                loc.Y += (int)(xOffset * tangent);
                hasFixed = true;
                // Updates current Location
                Location = loc;
            }
            return hasFixed;
        }

        /// <summary>
        /// Fixes Y coordinate if it's out of the top or bottom bound.
        /// </summary>
        /// <param name="height">Height of the world</param>
        /// <param name="loc">(Previous) location to be fixed. Can be current location; just depends on how the method does it</param>
        /// <param name="tangent">Tangent of the angle forced by the direction of motion and distance to the bound</param>
        /// <returns>If Y is fixed</returns>
        private bool FixLocation_Y(int height, Point loc, float tangent)
        {
            // TODO: Temporary
            Room room = Game1.Singleton.CurrentRoom;
            bool hasFixed = false;
            if (IsOutOfBound(ScreenBoundary.Top) || IsOutOfBound(ScreenBoundary.Bottom))
            {
                int yOffset = IsOutOfBound(ScreenBoundary.Top) ?
                    room.Self.Top - PreviousBoundingBox.Top : room.Self.Bottom - PreviousBoundingBox.Bottom;
                loc.Y += yOffset;
                loc.X += (int)(yOffset / tangent);
                hasFixed = true;
                // Updates current Location
                Location = loc;
            }
            return hasFixed;
        }

        /// <summary>
        /// If the IMoveable object is out of the screen.
        /// </summary>
        /// <returns></returns>
        sealed bool IsOutOfBounds()
        {
            return IsOutOfBound(ScreenBoundary.Left)
                || IsOutOfBound(ScreenBoundary.Right)
                || IsOutOfBound(ScreenBoundary.Top)
                || IsOutOfBound(ScreenBoundary.Bottom);
        }

        /// <summary>
        /// If the IMoveable object is out of the given screen boundary.
        /// </summary>
        /// <param name="bound"></param>
        /// <returns></returns>
        sealed bool IsOutOfBound(ScreenBoundary bound)
        {
            var (width, height) = Game1.Singleton.WindowDimensions;
            //switch (bound)
            //{
            //    case ScreenBoundary.Left:
            //        return BoundingBox.Left < 0;
            //    case ScreenBoundary.Right:
            //        return BoundingBox.Right > width;
            //    case ScreenBoundary.Top:
            //        return BoundingBox.Top < 0;
            //    case ScreenBoundary.Bottom:
            //        return BoundingBox.Bottom > height;
            //}
            Room room = Game1.Singleton.CurrentRoom;
            switch (bound)
            {
                case ScreenBoundary.Left:
                    return BoundingBox.Left < room.Self.Left;
                case ScreenBoundary.Right:
                    return BoundingBox.Right > room.Self.Right;
                case ScreenBoundary.Top:
                    return BoundingBox.Top < room.Self.Top;
                case ScreenBoundary.Bottom:
                    return BoundingBox.Bottom > room.Self.Bottom;
            }
            return false;
        }

        /// <summary>
        /// If the given location is out of the screen.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        static bool IsOutOfBounds(Point location)
        {
            return IsOutOfBound(location, ScreenBoundary.Left)
                || IsOutOfBound(location, ScreenBoundary.Right)
                || IsOutOfBound(location, ScreenBoundary.Top)
                || IsOutOfBound(location, ScreenBoundary.Bottom);
        }

        /// <summary>
        /// If the given location is out of the specified screen boundary
        /// </summary>
        /// <param name="location"></param>
        /// <param name="bound"></param>
        /// <returns></returns>
        static bool IsOutOfBound(Point location, ScreenBoundary bound)
        {
            var (width, height) = Game1.Singleton.WindowDimensions;
            //switch (bound)
            //{
            //    case ScreenBoundary.Left:
            //        return location.X < 0;
            //    case ScreenBoundary.Right:
            //        return location.X > width;
            //    case ScreenBoundary.Top:
            //        return location.Y < 0;
            //    case ScreenBoundary.Bottom:
            //        return location.Y > height;
            //}
            Room room = Game1.Singleton.CurrentRoom;
            switch (bound)
            {
                case ScreenBoundary.Left:
                    return location.X < room.Self.Left;
                case ScreenBoundary.Right:
                    return location.X > room.Self.Right;
                case ScreenBoundary.Top:
                    return location.Y < room.Self.Top;
                case ScreenBoundary.Bottom:
                    return location.Y > room.Self.Bottom;
            }
            return false;
        }

        /// <summary>
        /// If the IMoveable is colliding with any entity, returns a tuple with the result and colliding entities.
        /// </summary>
        /// <returns></returns>
        (bool result, IEntity[] entities) CheckCollision()
        {
            // TODO
            return (true, null);
        }

        /// <summary>
        /// Exactly what the name means.
        /// </summary>
        /// <returns></returns>
        sealed bool IsCollidingWithAnyEntity()
        {
            return CheckCollision().result;
        }
    }
}

