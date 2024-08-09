using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using ProjectProphecy.ns_Utility;

namespace ProjectProphecy.Map
{
    /// <summary>
    /// All available tile types
    /// </summary>
    public enum Tiles
    {
        Wall,
        Floor,
        Left,
        Right,
        Top,
        Bottom
    }

    /// <summary>
    /// Data for a certain type of tile. All tile types' corresponding datas
    /// are stored in the static dictionary of this class since File IO takes 
    /// time and makes less sense when few configurations are needed
    /// </summary>
    public class TileData
    {
        // --- Fields ---
        private string name;
        private Point size;
        private bool passable;
        // All pre-defined TileDatas; Used hard coding instead of a File IO to read configurations
        private static Dictionary<string, TileData> datas;

        static TileData()
        {
            datas = new Dictionary<string, TileData>();
            // TODO: Custom tile size in each room??
            Point size = new Point(32, 32);
            AddData(Tiles.Wall, size, false);
            AddData(Tiles.Floor, size);
            AddData(Tiles.Left, size);
            AddData(Tiles.Right, size);
            AddData(Tiles.Top, size);
            AddData(Tiles.Bottom, size);
        }

        // --- Properties ---
        public string Name
        {
            get => name;
        }

        public Point Size
        {
            get => size;
        }

        public bool Passable
        {
            get => passable;
        }

        // --- Constructors ---
        private TileData(string name, Point size, bool passable)
        {
            this.name = name;
            this.size = size;
            this.passable = passable;
        }

        // --- Methods ---
        public static TileData GetData(string name)
        {
            if (!datas.ContainsKey(name))
            {
                throw new MajorIssueException($"Can't create tile - TileData for tile {name} not found!");
            }
            return datas[name];
        }

        private static void AddData(Tiles tile, Point size, bool passable = true)
        {
            string tileName = tile.ToString();
            if (datas.ContainsKey(tileName))
            {
                Logger.Log($"Warning: already set TileData for tile {tileName}!", Logger.DebugLevel.Error);
                return;
            }
            datas.Add(tileName, new TileData(tileName, size, passable));
        }
    }
}
