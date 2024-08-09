using Microsoft.Xna.Framework;
using ProjectProphecy.ns_Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ProjectProphecy.ns_Utility;


namespace ProjectProphecy.Map
{
    public class RoomManager
    {
        // --- Fields ---
        // Singleton of the manager class
        private static readonly Lazy<RoomManager> manager = new Lazy<RoomManager>(() => new RoomManager());
        // All rooms
        private Dictionary<string, Room> rooms = new Dictionary<string, Room>(StringComparer.OrdinalIgnoreCase);
        // Current room
        private Room currentRoom;
        // Where all the room configs are stored
        private readonly string root = @"..\..\..\Content\Rooms\";

        // --- Properties ---
        /// <summary>
        /// Use this property to call manager functions.
        /// </summary>
        public static RoomManager Singleton
        {
            get => manager.Value;
        }

        public Dictionary<string, Room> Rooms
        {
            get => rooms;
        }

        public Room CurrentRoom
        {
            get => currentRoom;
            set => currentRoom = value;
        }

        // --- Methods ---
        /// <summary>
        /// Called on game restart (soft)
        /// </summary>
        public void Reset()
        {
            currentRoom = null;
            rooms.Clear();
        }

        /// <summary>
        /// Generates a room using its corresponding config file in the game world
        /// </summary>
        /// <param name="name">Unique name of the room</param>
        /// <returns>If the room was successfully generated</returns>
        public bool GenerateRoom(string name)
        {
            Logger.Log($"Try generating room {name}...");
            if (rooms.ContainsKey(name))
            {
                string message = "A room with the same name exists! Please check your config files for conflicts :)";
                Logger.Log(message);
                throw new MajorIssueException(message);
            }
            try
            {
                ReadRoomConfig(name);
                // TODO: print room info when generation is finished.
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error occurred during generation for room {name}. {ex.Message}\n{ex.StackTrace}");
                rooms.Remove(name);
                return false;
            }
        }

        public Room ConnectRoom(Room room, string name)
        {
            if (rooms.ContainsKey(name))
            {
                Logger.Log($"Room {name} already exists! Now Connected to {room.Name}.");
                return rooms[name];
            }
            else
            {
                Logger.Log($"Room {name} hasn't been created. Now genearting...");
                if (GenerateRoom(name))
                {
                    return rooms[name];
                }
            }
            Logger.Log($"Error occured connecting room {name} to {room.Name}");
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        private Room ReadRoomConfig(string configName)
        {
            string pathYML = root + configName + ".yml";
            string pathTXT = root + configName + ".txt";

            bool hasConfig = false;
            string path = "";

            if (File.Exists(pathYML))
            {
                hasConfig = true;
                path = pathYML;
            }
            else if (File.Exists(pathTXT))
            {
                hasConfig = true;
                path = pathTXT;
            }
            // If config file exists, try loading its data
            if (hasConfig)
            {
                (int width, int height) dimensions; // Size of the room in tiles

                using StreamReader reader = new StreamReader(path);
                string line = null;           // Current line in config
                int lineIndex = 1;            // Index of the current line 
                string[] output;              // Elements of the current line
                Dictionary<Point, Tile> tiles // Tiles in the room
                    = new Dictionary<Point, Tile>();

                // Line #1 specifies room dimensions
                if ((line = reader.ReadLine()) != null)
                {
                    // output = line.Replace(" ", string.Empty).Split(",");
                    output = line.Split(" ");
                    dimensions = (int.Parse(output[0]), int.Parse(output[1]));
                    lineIndex++;
                }
                else
                {
                    throw new ArgumentNullException("Line #1 should define room dimensions");
                }

                // Reads the tiles in sequence: Top Left -> Bottom Right
                for (int h = 0; h < dimensions.height; h++)
                {
                    if ((line = reader.ReadLine()) != null)
                    {
                        // output = line.Replace(" ", string.Empty).Split(",");
                        output = line.Split(" ");
                        try
                        {
                            for (int w = 0; w < dimensions.width; w++)
                            {
                                // Special "tiles" like enemies, items have respective process. Avoid generating them as tiles;
                                // Instead, count them as floor tiles and then spawn them right on the floor.
                                // TODO: Generate enemies
                                Point tileLoc = new Point(w, h);
                                string tileName = ReadTile(output[w]);
                                if (tileName.Equals("Enemy"))
                                {
                                    tileName = "Floor";
                                }
                                // tiles.Add(tileLoc, new Tile(tileName, tileLoc));
                                tiles.Add(tileLoc, new Tile(TileData.GetData(tileName), tileLoc));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex is FormatException)
                            {
                                throw new Exception("Wrong syntax");
                            }
                            else if (ex is IndexOutOfRangeException)
                            {
                                throw new Exception("Missing tiles (columns)");
                            }
                            else
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    }
                    else throw new ArgumentNullException($"Not enough rows for defined room dimensions at line {lineIndex}");
                }

                // Initial generation is done
                Room newRoom = new Room(configName, dimensions.width, dimensions.height, tiles);
                // Adds the room to the dictionary
                rooms.Add(configName, newRoom);

                // Reads additional section configurations
                while ((line = reader.ReadLine()) != null)
                {
                    // Reads the possible connected pre-defined rooms.
                    // Section text
                    if (line.ToLower().StartsWith("rooms"))
                    {
                        // Reads the next line as room connection configuration
                        // until no line exists or an empty line as section spacing
                        while (!string.IsNullOrEmpty(line = reader.ReadLine().ToLower()))
                        {
                            string roomName = null;
                            if (line.StartsWith("left: "))
                            {
                                roomName = line[6..];
                                newRoom.Left = ConnectRoom(newRoom, roomName);
                            }
                            else if (line.StartsWith("right: "))
                            {
                                roomName = line[7..];
                                newRoom.Right = ConnectRoom(newRoom, roomName);
                            }
                            else if (line.StartsWith("top: "))
                            {
                                roomName = line[5..];
                                newRoom.Top = ConnectRoom(newRoom, roomName);
                            }
                            else if (line.StartsWith("bottom: "))
                            {
                                roomName = line[8..];
                                newRoom.Bottom = ConnectRoom(newRoom, roomName);
                            }
                        }
                        // Attaches door tiles to rooms
                        foreach (Tile tile in newRoom.Tiles.Values)
                        {
                            if (tile is Door)
                            {
                                Door door = tile as Door;
                                switch (tile.Name.ToLower())
                                {
                                    case "left":
                                        door.ConnectedRoom = newRoom.Left;
                                        break;
                                    case "right":
                                        door.ConnectedRoom = newRoom.Right;
                                        break;
                                    case "top":
                                        door.ConnectedRoom = newRoom.Top;
                                        break;
                                    case "bottom":
                                        door.ConnectedRoom = newRoom.Bottom;
                                        break;
                                }
                            }
                        }
                    }

                    // TODO: Reads enemies
                    else if (line.ToLower().StartsWith("enemies"))
                    {

                    }
                }
                return newRoom;
            }
            // Abandon room generation since no valid data is found
            else
            {
                throw new Exception("Cannot find config file.");
            }
        }

        /// <summary>
        /// Converts tile from the placeholder to its real name
        /// </summary>
        /// <param name="placeholder"></param>
        /// <returns></returns>
        private string ReadTile(string placeholder)
        {
            switch (placeholder)
            {
                case "X":
                    return "Wall";
                case "~":
                    return "Floor";
                case "E":
                    return "Enemy";
                case "L":
                    return "Left";
                case "R":
                    return "Right";
                case "T":
                    return "Top";
                case "B":
                    return "Bottom";
                default:
                    return "Floor";
            }
        }

        public void SwitchRoom(Room next)
        {
            // Remove the player from current room
            EntityManager.Singleton.RemoveEntity(Game1.Singleton.Player,false);
            // Change the current room
            currentRoom = next;
            // Add the player to the new current room
            EntityManager.Singleton.Register(Game1.Singleton.Player);
        }
    }
}
