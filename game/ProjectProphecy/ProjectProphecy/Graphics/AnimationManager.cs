using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using System.Numerics;
using ProjectProphecy.ns_Utility;
using System.Collections.Immutable;

namespace ProjectProphecy.ns_Graphics
{
    /// <summary>
    /// Manager class for all IDrawable objects in a single room.
    /// </summary>
    public class AnimationManager
    {
        // --- Fields ---
        // Singleton of the manager class
        private static readonly AnimationManager manager = new AnimationManager();
        // TODO: All IDrawables in all rooms - display for static items is also handled by AnimationManager
        private readonly Dictionary<string, IDrawable[]> items;
        // Loaded sprite sheets - single images containing multiple animations. Dictionary keys case-insensitive.
        private readonly Dictionary<string, Texture2D> loadedSpriteSheets = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        // Loaded animations - all animations from different sprite sheets. Secondary dictionary keys are case-insensitive.
        private readonly Dictionary<Texture2D, Dictionary<string, Animation>> loadedAnimations = new Dictionary<Texture2D, Dictionary<string, Animation>>();
        // Where all the sprite sheets and configs are stored
        private readonly string root = @"..\..\..\Content\Animations\";


        // --- Properties ---
        /// <summary>
        /// Use this property to call manager functions.
        /// </summary>
        public static AnimationManager Singleton
        {
            get => manager;
        }

        // --- Constructors ---
        static AnimationManager()
        {
            manager.LoadAllAnimations();
        }

        // --- Methods ---
        /// <summary>
        /// Update a single room's all items using the room's name.
        /// TODO: Unimplemented in game
        /// </summary>
        /// <param name="key"></param>
        public void Update(string key)
        {
            foreach (IDrawable item in items[key])
            {
                Update(item);
            }
        }

        /// <summary>
        /// Updates and displayes an IDrawable item on the screen
        /// </summary>
        /// <param name="key"></param>
        public void Update(IDrawable item)
        {
            // If the item is animatable, updates its current animation in order to display the proper frame
            if (item is IAnimatable)
            {
                (item as IAnimatable).Update();
            }
            // Displays the item if it's an Ientity
            if (item is IEntity)
            {
                IEntity entity = item as IEntity;
                item.Display(entity.Location.X, entity.Location.Y, true);
                // Further, if it's LivingEntiy
                if (item is LivingEntity && !(item is Player) && !(item is Projectile))
                {
                    LivingEntity livingEntity = item as LivingEntity;
                    // Draws health bar
                    // Computes approriate location for the health bar (above head)
                    Rectangle hpBarLoc = new Rectangle(livingEntity.BoundingBox.Center.X, livingEntity.BoundingBox.Top, (int)(40 * livingEntity.Scale), (int)(3 * livingEntity.Scale));
                    // Though the bar's center can indeed be aligned without the offset,
                    // what we actually want is see the "visible" health block in the middle
                    int xOffset = (int)((1 - livingEntity.Health / livingEntity.MaxHealth) * hpBarLoc.Width / 2);
                    int yOffset = (int)(8 - livingEntity.BoundingBox.Height / 2.0);
                    hpBarLoc.Location = Utility.AlignCenters(
                        livingEntity.BoundingBox, hpBarLoc).ToPoint()
                        + new Point(xOffset, yOffset);
                    UIManager.Singleton.GetBar($"HP", hpBarLoc.X, hpBarLoc.Y, 0, hpBarLoc.Width, hpBarLoc.Height, livingEntity.Health, livingEntity.MaxHealth).Draw(Game1.Singleton.SpriteBatch);
                    // Computes approriate location for the health lable
                    Rectangle hpTextLoc = new Rectangle(livingEntity.BoundingBox.Center.X, livingEntity.BoundingBox.Top - 10, (int)(40 * livingEntity.Scale), (int)(3 * livingEntity.Scale));
                    string hpText = $"{livingEntity.Health}/{livingEntity.MaxHealth}";
                    hpTextLoc.Location += Utility.AlignString(hpText, "General", 0.5f, 0.5f).ToPoint();
                    UIManager.Singleton.GetText($"HPText", "General", hpTextLoc.X, hpTextLoc.Y, 0, 0.75f, 0.75f, hpText).Draw(Game1.Singleton.SpriteBatch);
                }
            }
            // TODO
            else
            {
            }
        }

        /// <summary>
        /// Draws all living entities in the current room
        /// </summary>
        /// <param name="roomName"></param>
        public void UpdateAll()
        {
            EntityManager entityManager = Game1.Singleton.Entity;
            string currentRoom = Game1.Singleton.CurrentRoom.Name;
            List<LivingEntity> entities = entityManager.GetLivingEntities(currentRoom).ToList();
            for (int i = 0; i < entities.Count; i++)
            {
                LivingEntity entity = entities[i];
                // Player should be executed last - I know this is kinda bad means, but convenient.
                // Literally no time to figure out a certain "entity drawing priority" system.
                if (entity is Player && i < entities.Count - 1)
                {
                    // Swaps the player and last entity
                    entities[i] = entities[^1];
                    entities[^1] = entity;
                    entity = entities[i];
                }
                Update(entity);
            }
        }

        /// <summary>
        /// Loads all animations in all sprite sheets in the Animations directory
        /// </summary>
        public void LoadAllAnimations()
        {
            ContentManager content = Game1.Singleton.Content;
            // DirectoryInfo dir = new DirectoryInfo(content.RootDirectory + "\\Animations"); // XNA asset folder - not the right directory
            DirectoryInfo dir = new DirectoryInfo(root);
            // Assuming only image and config files exist, excludes all config files so the rest are image files
            FileInfo[] spriteSheets = dir.GetFiles().Where(file => file.Extension != ".yml" && file.Extension != ".txt").ToArray();
            Logger.Log($"{spriteSheets.Length} sprite sheets found. Trying to load...");
            // For each sheet, loads their animations
            foreach (FileInfo spriteSheet in spriteSheets)
            {
                string name = spriteSheet.Name.Substring(0, spriteSheet.Name.Length - spriteSheet.Extension.Length);
                Logger.Log($"Processing sheet: {spriteSheet.Name}");
                LoadAnimation(name, name);
            }
        }

        /// <summary>
        /// Loads all animations in a sprite sheet into AnimationManager using given names for the sprite sheet image and config file.
        /// Generally, both names should be the same. If no config file is found, uses default animation settings.
        /// Also, both file paths have the same root directory Content\Animations
        /// Supported config file formats: .yml & .txt
        /// </summary>
        /// <param name="sheetName"> just the asset's name in Content.mcdb</param>
        /// <param name="configName"> just the file name, no extesion included</param>
        public void LoadAnimation(string sheetName, string configName)
        {
            // Step 1: Loads the sprite sheet for the animation
            ContentManager content = Game1.Singleton.Content;
            Texture2D spriteSheet = null;
            try
            {
                spriteSheet = content.Load<Texture2D>(@$"Animations\{sheetName}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading Texture2D {sheetName}: {ex.Message} Is the content registered in Content.mgcb?");
                // Sprite sheet is not found - no need to proceed.
                return;
            }

            // Step 2: Uses FileIO to initialize the animation.
            // If no config file is found, turn to the alternative approach instead, with default animation settings.
            // Config files can be either a yml or a txt file, stored under ProjectProphecy\Content\Animations
            string pathYML = root + configName + ".yml";
            string pathTXT = root + configName + ".txt";
            // Try to find and read the file.
            StreamReader reader = null;
            string line = null;                          // Current line in config
            int lineIndex = 1;                           // Index of the current line 
            bool hasConfig = false;                      // Process is different depending on the exsistence or completion of config.
            Dictionary<int, string[]> rowConfigs = null; // Configs for each row in the sprite sheet which represents a single animation.
            // Default settings
            (int width, int height) size = (32, 32);     // Size of a single sprite
            const double fps_default = 10;               // Default fps

            // Try to load the config file. The animation can load without but definitely better with a config file.
            try
            {
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
                    reader = new StreamReader(path);
                    string[] output;                              // Elements of the current line
                    rowConfigs = new Dictionary<int, string[]>(); // Used ONLY when the config exists
                    int loadedRows = 0;                           // Loaded animations

                    // Line #1 specifies size (width, height) of a single sprite to divide the sheet into subsections (rows, columns)
                    if ((line = reader.ReadLine()) != null)
                    {
                        output = line.Replace(" ", string.Empty).Split(",");
                        size = (int.Parse(output[0]), int.Parse(output[1]));
                        lineIndex++;
                    }
                    // Each line following specifies an animation inside: Row, Name, Frames, FPS, Duration (non-positive means forever)
                    // ex) 1, attack, 5, 10, 0.5
                    while ((line = reader.ReadLine()) != null)
                    {
                        output = line.Replace(" ", string.Empty).Split(",");
                        int row = int.Parse(output[0]);
                        // Parses parameters without storing them. If something is wrong, it will be caught.
                        try
                        {
                            int.Parse(output[2]);    // Frames
                            double.Parse(output[3]); // FPS
                            double.Parse(output[4]); // Duration

                            // Saves output to rowConfigs after making sure no error would occur
                            rowConfigs[row] = output;
                            loadedRows++;
                        }
                        catch (Exception ex)
                        {
                            string message;
                            if (ex is FormatException)
                            {
                                message = "Wrong syntax";
                            }
                            else if (ex is IndexOutOfRangeException)
                            {
                                message = "Missing parameters";
                            }
                            else
                            {
                                message = ex.Message;
                            }
                            Logger.Log($"Error reading config: {message} at line {lineIndex}: {line}\n{ex.StackTrace}");
                        }
                        lineIndex++;
                    }

                    int totalRows = spriteSheet.Height / size.height;
                    if (loadedRows < totalRows)
                    {
                        Logger.Log($"Animation config {configName} loaded - insufficiant animation data. {loadedRows}/{totalRows} rows defined.");
                    }
                    else
                    {
                        Logger.Log($"Animation config {configName} loaded successfully!");
                    }
                }
                // Sends a notification if no save file was found or it's empty.
                else
                {
                    Logger.Log($"No valid data found for config {configName}. Default process applies.");
                }
            }
            // Error message. Most probably caused by Wrong data syntax.
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    Logger.Log($"Error reading config: {ex.Message} at line {lineIndex}: {line}\n{ex.StackTrace}");
                }
                else
                {
                    Logger.Log($"Error reading config: {ex.Message}\n{ex.StackTrace}");
                }
                // Config loading fails - continue, using default settings. NO return here!
            }
            if (reader != null)
            {
                reader.Close();
            }

            // Loads animations
            Queue<Animation> tempLoadedAnimations = new Queue<Animation>();
            try
            {
                // Loops through each row
                int currentRow = 0;
                for (int y = 0; y + size.height <= spriteSheet.Height; y += size.height, currentRow++)
                {
                    string[] settings = null;                     // Unchecked
                    string name = "";                             // Undetermined
                    int frames = 0;                               // Undetermined
                    double FPS = 0;                               // Undertermined
                    double duration = 0;                          // Undetermined
                    int columns = spriteSheet.Width / size.width; // int division (DIV)
                    List<Rectangle> subsections = new List<Rectangle>();
                    // Try to get settings from rowConfigs
                    if (rowConfigs != null && rowConfigs.ContainsKey(currentRow))
                    {
                        settings = rowConfigs[currentRow];
                    }
                    // If row config exists, parse settings within
                    if (settings != null)
                    {
                        // Parses settings
                        name = settings[1];
                        frames = int.Parse(settings[2]);
                        FPS = double.Parse(settings[3]);
                        duration = double.Parse(settings[4]);
                    }
                    // Analyze the sprite sheet otherwise 
                    else
                    {
                        name = "Undefined";
                        // Does a rough check to count subsections/frames - checks every diagonal pixel
                        // of the current sprite, if each is WHITE, it's a blank frame - loading for the
                        // current animation ends; go load the next one!
                        // Loops through as many frames as possible
                        for (int x = 0; x + size.width <= spriteSheet.Width; x += size.width)
                        {
                            Color[,] spriteColorArray = spriteSheet.GetPixels2(new Rectangle(x, y, size.width, size.height));

                            // Gets all diagonal pixels
                            bool isWidthShorter = size.width <= size.height;
                            double tangent = (double)size.height / size.width;
                            bool isBlankFrame = true;
                            // Looping through diagonal pixels.
                            if (isWidthShorter)
                            {
                                for (int pixelY = 0; pixelY < size.height; pixelY++)
                                {
                                    // P(x,y) => x = y / tan
                                    int pixelX = (int)(pixelY / tangent);
                                    #region This part is the same
                                    if (spriteColorArray[pixelX, pixelY] != Color.White
                                        || // Plus the symmetric diagonal pixel on the other side
                                        spriteColorArray[pixelX, size.height - 1 - pixelY] != Color.White)
                                    {
                                        isBlankFrame = false;
                                        frames++;
                                        // Breaks the inner loop, letting outer loop go to the next frame   
                                        break;
                                    }
                                    #endregion
                                }
                            }
                            else
                            {
                                for (int pixelX = 0; pixelX < size.width; pixelX++)
                                {
                                    // P(x,y) => y = x * tan
                                    int pixelY = (int)(pixelX * tangent);
                                    #region This part is the same
                                    if (spriteColorArray[pixelX, pixelY] != Color.White
                                        || // Plus the symmetric diagonal pixel on the other side
                                        spriteColorArray[pixelX, size.height - 1 - pixelY] != Color.White)
                                    {
                                        isBlankFrame = false;
                                        frames++;
                                        // Breaks the inner loop, letting outer loop go to the next frame
                                        break;
                                    }
                                    #endregion
                                }
                            }
                            // Blank frame means end of the animation. Proceed to the next row to load another animation
                            if (isBlankFrame)
                            {
                                continue;
                            }
                        }
                        FPS = fps_default;
                        duration = frames / FPS; // Default duraiton is the time it takes to play once
                    }
                    // Renames the reserved animation rows
                    switch (currentRow)
                    {
                        case 0:
                            name = "Stand";
                            break;
                        case 1:
                            name = "Move";
                            break;
                        case 2:
                            name = "Attack";
                            break;
                        default:
                            break;
                    }
                    if (frames > columns)
                    {
                        Logger.Log($"Warning: Config file {configName} has defined more frames for animation {name} than the sprite sheet {sheetName} can hold on row {currentRow}.");
                    }
                    // Loops through the required number of columns, determined by frames
                    for (int x = 0; x + size.width <= frames * size.width; x += size.width)
                    {
                        // Gets the subsection for sprite
                        subsections.Add(new Rectangle(x, y, size.width, size.height));
                    }
                    tempLoadedAnimations.Enqueue(new Animation(size, spriteSheet, subsections.ToArray(), currentRow, name, frames, FPS, duration));
                }
                // Successfully loaded.
                // Stores loaded sprite sheet for further use.
                loadedSpriteSheets[sheetName] = spriteSheet;
                loadedAnimations[spriteSheet] = new Dictionary<string, Animation>(StringComparer.OrdinalIgnoreCase);
                // Stores loaded animations for further use.
                // Count of rowConfigs is inaccurate representing number of configured animations - rowConfigs may include config for non-existent rows
                int configuredAnimations = 0;
                int totalAnimations = tempLoadedAnimations.Count;
                while (tempLoadedAnimations.Count > 0)
                {
                    Animation animation = tempLoadedAnimations.Dequeue();
                    loadedAnimations[spriteSheet][animation.Row.ToString()] = animation;
                    loadedAnimations[spriteSheet][animation.Name] = animation;
                    // Distinguish when default loading process is applied to this animation.
                    // Has row config
                    if (hasConfig && rowConfigs.ContainsKey(animation.Row))
                    {
                        configuredAnimations++;
                        Logger.Log($"{animation.Row} - {animation.Name} loaded. {animation}");
                    }
                    // Default applied
                    else
                    {
                        Logger.Log($"{animation.Row}(D) - {animation.Name} loaded. {animation}");
                    }
                }
                if (hasConfig)
                {
                    if (configuredAnimations == totalAnimations)
                    {
                        Logger.Log($"Sprite Sheet {sheetName} is perfectly loaded - with a config and without any default process");
                    }
                    else
                    {
                        Logger.Log($"Sprite Sheet {sheetName} is loaded with a config, but the data is insufficient - {configuredAnimations}/{totalAnimations} animations defined.");
                    }
                }
                else
                {
                    Logger.Log($"Sprite Sheet {sheetName} is loaded through default process");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error initializing animation: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Registers the character animations for given targets (of the same type)
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="targets"></param>
        public void RegisterAnimation(string sheetName, string defaultAnimation, params IAnimatable[] targets)
        {
            try
            {
                Texture2D spriteSheet = loadedSpriteSheets[sheetName];
                foreach (IAnimatable target in targets)
                {
                    Dictionary<string, Animation> animations = loadedAnimations[spriteSheet];
                    target.Animations = animations;
                    target.DefaultAnimation = animations[defaultAnimation];
                    target.SetAnimation(target.DefaultAnimation);
                }
            }
            catch (Exception ex)
            {
                string message = $"Error registering animation \"{sheetName}\" to IAnimatable objects";
                Logger.Log(message + $": {ex.Message}\n{ex.StackTrace}");
                throw new MajorIssueException(message + $" - {ex.Message}");
            }
        }
    }
}
