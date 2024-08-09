using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectProphecy.Entity;
using ProjectProphecy.Entity.Stats;
using ProjectProphecy.Map;
using ProjectProphecy.ns_Controls;
using ProjectProphecy.ns_Entity;
using ProjectProphecy.ns_Graphics;
using ProjectProphecy.ns_Utility;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Button = ProjectProphecy.ns_Controls.Button;
using Keys = Microsoft.Xna.Framework.Input.Keys;

// IGME 106 - GDAPS 2 - Erika Mesh
// Team I - Hello World
// Monogame project - Project Prophecy
// Anthony Sakis
// David Benson
// Joey Patrum
// William Mruz
// Zhao Jin

namespace ProjectProphecy
{
    // Major game states available
    enum GameStates
    {
        Title,    // The start menu / title screen.
        Play,     // Main gameplay. The game progresses in this state.
        Pause,    // When the game is paused
        Interact, // When the player is shopping, upgrading skills, checking inventory and so on
        Dialog,    // When the player is talking to somebody
        Defeat,     // When the player has died 
        Victory,    // When the player has completed the game succesfuly
    }

    public class Game1 : Game
    {
        // --- Fields ---
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private HashSet<Task> tasks;

        // Player
        private Player player;
        private int playTimes;

        // Enemy
        private Enemy enemy;
        private Runevark runevark;

        // Window dimensions
        private int windowWidth;            // Width of the game window
        private int windowHeight;           // Height of the game window

        // Current game state
        private GameStates currentState = GameStates.Title;

        // Managers
        public InputManager Input;
        public EntityManager Entity;
        public AnimationManager Animation;
        public UIManager UI;
        public RoomManager Room;

        // Buttons
        private Button newGameButton;
        private Button exitButton;
        private Button resumeButton;
        private Button mainMenuButton;
        private Button mainMenuButtonVic;   // go to main menu from victory screen
        private Button mainMenuButtonDef;   // go to main menu from defeat screen
        private Button exitVic;   // exit from victory screen
        private Button exitDef;   // exit from defeat screen
        private Button playAgainVic;   // play again from victory screen
        private Button playAgainDef;   // play again from defeat screen

        // Relics
        private Relic critsword = new Relic("Critsword");
        private Relic lifespear = new Relic("Lifesoear");
        private Relic glorymace = new Relic("Glorymace");

        // Relic bools
        private bool hasSword = false;
        private bool hasSpear = false;
        private bool hasMace = false;

        // Singleton of Game1; the game.
        private static readonly Lazy<Game1> game = new Lazy<Game1>(() => new Game1());
        private GameTime gameTime;

        // --- Properties ---
        /// <summary>
        /// Readonly property for the Game1 singleton
        /// </summary>
        public static Game1 Singleton
        {
            get => game.Value;
        }

        /// <summary>
        /// Provides the gameTime publicly instead of having to pass it as a parameter.
        /// </summary>
        public GameTime GameTime
        {
            get => gameTime;
        }

        /// <summary>
        /// Provides the spriteBatch publicly instead of having to pass it as a parameter.
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get => spriteBatch;
        }

        /// <summary>
        /// The player entity
        /// </summary>
        public Player Player
        {
            get => player;
        }

        /// <summary>
        /// How many times the player has tried
        /// </summary>
        public int PlayTimes
        {
            get => playTimes;
        }

        /// <summary>
        /// How many times the player has retried - after the first game
        /// </summary>
        public int ReplayTimes
        {
            get => playTimes - 1;
        }

        /// <summary>
        /// Current room the game is in
        /// </summary>
        public Room CurrentRoom
        {
            get => Room.CurrentRoom;
            set => Room.CurrentRoom = value;
        }

        /// <summary>
        /// Gets the current window dimensions.
        /// Be sure you set this instead using the vinilla Monogame methods to change
        ///  the resolution, or the windowWidth and windowHeight won't get updated.
        /// </summary>
        public (int width, int height) WindowDimensions
        {
            get => (windowWidth, windowHeight);
            private set
            {
                // Applies the resolution changes
                graphics.PreferredBackBufferWidth = value.width;
                graphics.PreferredBackBufferHeight = value.height;
                graphics.ApplyChanges();
                // Stores the window dimensions to the fields.
                windowWidth = value.width;
                windowHeight = value.height;
            }
        }

        public Rectangle GameWindow
        {
            get => new Rectangle(0, 0, windowWidth, windowHeight);
        }

        // --- Constructors ---
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            tasks = new HashSet<Task>();
        }

        // --- Methods ---
        protected override void Initialize()
        {
            #region Program properties
            // Sets the game resolution.
            WindowDimensions = (1920, 1080);
            Logger.Log($"Three ways to get current window dimensions: ");
            Logger.Log($"graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height {graphics.GraphicsDevice.Viewport.Width}x{graphics.GraphicsDevice.Viewport.Height} ");
            Logger.Log($"graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight {graphics.PreferredBackBufferWidth}x{graphics.PreferredBackBufferHeight} ");
            Logger.Log($"graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height {graphics.GraphicsDevice.Viewport.Width}x{graphics.GraphicsDevice.Viewport.Height}");
            #endregion

            // Initializes the managers
            Input = InputManager.Singleton;
            Entity = EntityManager.Singleton;


            // TODO: Frame rate test
            IsFixedTimeStep = true; //false;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d); //60);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Initializes the managers.
            Animation = AnimationManager.Singleton;
            UI = UIManager.Singleton;
            Room = RoomManager.Singleton;

            // Changes the cursor to original icon
            Texture2D cursor = UI.GetTexture("Cursor");
            Mouse.SetCursor(MouseCursor.FromTexture2D(cursor, cursor.Width / 2, cursor.Height / 2));

            // Can be removed:
            // No need - already does once in Play button
            //// Execute initial loading of resettable elements
            //ResetGame();

            // Buttons test code for now
            newGameButton = UI.RegisterButton(
                "NewGame",
                new Vector2(windowWidth / 2, windowHeight * 0.73f),
                "New Game",
                "MenuButton",
                UI.GetFont("LabelFont"),
                new Color(47, 200, 227))
                .RegisterEvent(Button.Events.OnClick, Button_StartNewGame);
            exitButton = UI.RegisterButton(
                "Exit",
                new Vector2(windowWidth / 2, windowHeight * 0.88f),
                "Exit",
                "MenuButton",
                UI.GetFont("LabelFont"),
                new Color(47, 200, 227))
                .RegisterEvent(Button.Events.OnClick, (sender, e) => Exit());
            resumeButton = UI.RegisterButton(
                "Resume",
                new Vector2(windowWidth / 2, windowHeight * 0.55f),
                "Resume",
                "PauseButton",
                UI.GetFont("LabelFont"),
                new Color(12, 18, 47))
                .RegisterEvent(Button.Events.OnClick, (sender, e) => currentState = GameStates.Play);
            mainMenuButton = UI.RegisterButton(
                "MainMenu",
                new Vector2(windowWidth / 2, windowHeight * 0.7f),
                "Main Menu",
                "PauseButton",
                UI.GetFont("LabelFont"),
                new Color(12, 18, 47))
                .RegisterEvent(Button.Events.OnClick, (sender, e) => currentState = GameStates.Title);
            mainMenuButtonVic = UI.RegisterButton(
                "MainMenu",
                new Vector2(windowWidth / 2, windowHeight * 0.85f),
                "Main Menu",
                "victorybutton",
                UI.GetFont("LabelFont"),
                new Color(104, 166, 215))
                .RegisterEvent(Button.Events.OnClick, (sender, e) => currentState = GameStates.Title);
            mainMenuButtonDef = UI.RegisterButton(
                "MainMenu",
                new Vector2(windowWidth / 2, windowHeight * 0.7f),
                "Main Menu",
                "retrybutton",
                UI.GetFont("LabelFont"),
                new Color(12, 18, 47))
                .RegisterEvent(Button.Events.OnClick, (sender, e) => currentState = GameStates.Title);
            exitVic = UI.RegisterButton(
                "MainMenu",
                new Vector2(windowWidth * 0.8f, windowHeight * 0.85f),
                "Exit",
                "victorybutton",
                UI.GetFont("LabelFont"),
                new Color(104, 166, 215))
                .RegisterEvent(Button.Events.OnClick, (sender, e) => Exit());
            exitDef = UI.RegisterButton(
                "MainMenu",
                new Vector2(windowWidth / 2, windowHeight * 0.85f),
                "Exit",
                "retrybutton",
                UI.GetFont("LabelFont"),
                new Color(12, 18, 47))
                .RegisterEvent(Button.Events.OnClick, (sender, e) => Exit());
            playAgainVic = UI.RegisterButton(
                "MainMenu",
                new Vector2(windowWidth * 0.2f, windowHeight * 0.85f),
                "New Adventure",
                "victorybutton",
                UI.GetFont("LabelFont"),
                new Color(104, 166, 215))
                .RegisterEvent(Button.Events.OnClick, Button_StartNewGame);
            playAgainDef = UI.RegisterButton(
                "MainMenu",
                new Vector2(windowWidth / 2, windowHeight * 0.55f),
                "Restart Adventure",
                "retrybutton",
                UI.GetFont("LabelFont"),
                new Color(12, 18, 47))
                .RegisterEvent(Button.Events.OnClick, Button_StartNewGame);
        }

        private void Button_StartNewGame(object sender, EventArgs e)
        {
            ResetGame();
            currentState = GameStates.Play;
        }

        protected override void Update(GameTime gameTime)
        {
            // Updates the gameTime static field so that we don't need to pass it as
            // a parameter into every update method that has time-related process.
            this.gameTime = gameTime;
            // Update our saved device state & get the current device states
            Input.Update();
            // Alt + Enter to switch between window & full screen.
            if (Input.HasPressed(Keys.LeftAlt, 200, false) && Input.HasPressed(Keys.Enter, 200, false))
            {
                // Removes stored key presses
                Input.ConsumeKeyPress(Keys.LeftAlt);
                Input.ConsumeKeyPress(Keys.Enter);
                graphics.ToggleFullScreen(); // Can also use graphics.IsFullScreen = !graphics.IsFullScreen
                Logger.Log($"Changed fullscreen to {graphics.IsFullScreen}");
                if (!graphics.IsFullScreen)
                {
                    // Adjusts the center so the game window is in the middle of the screen.
                    Rectangle computerResolution = new Rectangle(0, 0,
                        Screen.PrimaryScreen.Bounds.Width,
                        Screen.PrimaryScreen.Bounds.Height);
                    Rectangle gameResolution = new Rectangle(0, 0, windowWidth, windowHeight);
                    Point newLoc = Utility.AlignCenters(computerResolution, gameResolution).ToPoint();
                    // Gives a tiny offset or the window has no visual change
                    // if the full(monitor) resolution is exactly the game's resolution.
                    newLoc += new Point(10, 20);
                    Window.Position = newLoc;
                }
                graphics.ApplyChanges();
            }

            // Figure out which helper methods to call based on the current state
            // (these will change the current state if needed)
            switch (currentState)
            {
                case GameStates.Title:
                    ProcessTitle();
                    break;

                case GameStates.Play:
                    ProcessPlay();
                    break;

                case GameStates.Pause:
                    ProcessPause();
                    break;

                case GameStates.Defeat:
                    ProcessDefeat();
                    break;

                case GameStates.Victory:
                    ProcessVictory();
                    break;

                default:
                    break;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Handles what happens when game is on Main Menu. Nothing to do so far. 
        /// </summary>
        /// <param name="kbState"></param>
        private void ProcessTitle()
        {
            // Transitions
            // Exit by key
            if (Input.IsPressed(Keys.Escape))
            {
                Exit();
            }
            // Exit by in-game button
            exitButton.Update(gameTime);

            // Game starts
            newGameButton.Update(gameTime);
        }

        /// <summary>
        /// Handles what happens when game is playing. Nothing to do so far. 
        /// </summary>
        /// <param name="kbState"></param>
        private void ProcessPlay()
        {
            // Game pauses
            if (Input.IsPressed(Keys.Escape))
            {
                currentState = GameStates.Pause;
            }

            // TODO: Temporary room test!
            // Logger.Log($"{currentRoom.Left.Name} + {currentRoom.Right.Name} + {currentRoom.Top.Name} + {currentRoom.Bottom.Name}");
            if (Input.IsPressed(Keys.NumPad4) && CurrentRoom.Left != null)
            {
                Room.SwitchRoom(CurrentRoom.Left);
            }
            else if (Input.IsPressed(Keys.NumPad6) && CurrentRoom.Right != null)
            {
                Room.SwitchRoom(CurrentRoom.Right);
            }
            else if (Input.IsPressed(Keys.NumPad8) && CurrentRoom.Top != null)
            {
                Room.SwitchRoom(CurrentRoom.Top);
            }
            else if (Input.IsPressed(Keys.NumPad2) && CurrentRoom.Bottom != null)
            {
                Room.SwitchRoom(CurrentRoom.Bottom);
            }
            // TEST CODE FOR VICTORY AND DEFEAT SCREENS, CHANGE TO WHEN DEAD OR DEFEAT BOSS
            else if (Input.IsPressed(Keys.Insert) && CurrentRoom.Bottom != null)
            {
                currentState = GameStates.Defeat;
            }
            else if (Input.IsPressed(Keys.Delete) && CurrentRoom.Bottom != null)
            {
                currentState = GameStates.Victory;
            }

            // Updates all LivingEntities in the current room.
            // TODO: should be all IEntities!
            Entity.UpdateAll();

            // Updates the room
            CurrentRoom.Update();

            // Updates all UI components
            UI.UpdateAllComponents();

            // If servion dies go to defeat screen and reset game
            if (Player.Health <= 0)
            {
                Player.Health = 0;
                Player.Speed = 0;
                currentState = GameStates.Defeat;
                ResetGame();
            }
            // If runevark dies, victory screen and reset game
            else if (runevark.Health <= 0)
            {
                currentState = GameStates.Victory;
                ResetGame();
            }
        }

        /// <summary>
        /// Handles what happens when game is paused. Nothing to do so far. 
        /// </summary>
        /// <param name="kbState"></param>
        private void ProcessPause()
        {
            // Updates game according to the current game state.
            // Process -> Transition is better than Transition -> Process because if we handle transitions first,
            // we have to call the corresponding process method if the game has transit into another state

            // Handles transitions
            // Game resumes
            resumeButton.Update(gameTime);
            // Go to Main Menu.
            mainMenuButton.Update(gameTime);

            // Handles state process
            // TODO: nothing so far
        }

        /// <summary>
        /// Handles what happens in victory screen
        /// </summary>
        private void ProcessVictory()
        {
            // Play again button
            playAgainVic.Update(gameTime);

            // exit button from victory screen
            exitVic.Update(gameTime);

            // go to main menu from victory screen
            mainMenuButtonVic.Update(gameTime);
        }

        /// <summary>
        /// handles what happens in defeat screen
        /// </summary>
        private void ProcessDefeat()
        {
            // Play again button
            playAgainDef.Update(gameTime);

            // exit button from defeat screen
            exitDef.Update(gameTime);

            // go to main menu from defeat screen
            mainMenuButtonDef.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            #region Render all sprites
            // Default sortMode is Deferred -shoc in the order of calling. Change it to using layerDepth.
            // SamplerState.PointClamp : do not blend the sprites when scaling.
            // Source: https://community.monogame.net/t/sprite-scaling-problem/9675/3
            spriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack, samplerState: SamplerState.PointClamp);

            // Instructions for the current mode.
            string intro = "";

            // Draws corresponding(exclusive) items to the current mode.
            // Currently only transition instructions and placeholder screens.
            switch (currentState)
            {
                case GameStates.Title:
                    GraphicsDevice.Clear(Color.Black);
                    // Game titile background image
                    Vector2 dimensions = new Vector2(windowWidth, windowHeight);
                    // Every 10+10 seconds, the titleScreen zooms in and out.
                    double sizeMultiplier = Utility.MathHelper.BackAndForth(0.08, 10000, (int)gameTime.TotalGameTime.TotalMilliseconds);
                    Point titleScreenSize = new Point((int)(dimensions.X * (1.05 + sizeMultiplier)), (int)(dimensions.Y * (1.05 + sizeMultiplier)));
                    Rectangle gameWindow = new Rectangle(0, 0, windowWidth, windowHeight);
                    // Resets title screen background image to the screen center.
                    Rectangle titleScreen = new Rectangle(0, 0, titleScreenSize.X, titleScreenSize.Y);
                    titleScreen.Location = Utility.AlignCenters(gameWindow, titleScreen).ToPoint();
                    int maxXOffset = (titleScreenSize.X - windowWidth) / 2;
                    int maxYOffset = (titleScreenSize.Y - windowHeight) / 2;
                    titleScreen.Inflate(maxXOffset, maxYOffset);
                    Vector2 relativeDisplacement = (gameWindow.Center - Input.Mouse.Position).ToVector2();
                    Vector2 mappedDisplacement = new Vector2(
                        (float)Utility.MathHelper.Map(relativeDisplacement.X, -windowWidth / 2, windowWidth / 2, -maxXOffset, maxXOffset),
                        (float)Utility.MathHelper.Map(relativeDisplacement.Y, -windowHeight / 2, windowHeight / 2, -maxYOffset, maxYOffset));
                    titleScreen.Offset(mappedDisplacement);
                    UI.GetFrame("MainMenu", titleScreen.X, titleScreen.Y, 0, titleScreen.Width, titleScreen.Height).Draw(spriteBatch);
                    intro = "Team Hello World";
                    newGameButton.Draw(gameTime, spriteBatch);
                    exitButton.Draw(gameTime, spriteBatch);
                    break;

                case GameStates.Play:
                    GraphicsDevice.Clear(Color.Black);
                    intro = "Press ESC - Game pauses | Left Click - attack | Left Shift - dash";
                    // Room background
                    // UI.GetFrame("DefaultRoomBackground", 0, 0, 0, 1920, 1080).Draw(spriteBatch);
                    CurrentRoom.Draw();

                    /// UI elements over the background
                    // Starting point for UI depths
                    int z = 1000000;
                    // Status frame background
                    UI.GetFrame("StatusBg", 20, 30, z++, 360, 109).Draw(spriteBatch);
                    // Health bar & Mana bar
                    UI.GetBar("HP", 58, 46, z++, 300, 36, player.Health, player.MaxHealth).Draw(spriteBatch);
                    UI.GetBar("MP", 58, 93, z++, 237, 19, player.Mana, player.MaxMana).Draw(spriteBatch);
                    // Dash skill cooldown timer
                    UI.GetBar("Timer", 58, 119, z++, 45, 8, player.Skills["Dash"].Cooldown, player.Skills["Dash"].Data.Cooldown).Draw(spriteBatch);
                    // The status frame
                    UI.GetFrame("Status", 20, 30, z++, 360, 109).Draw(spriteBatch);
                    // Health text & Mana text
                    UI.GetText("HPText", "General", 230, 30, z++, 0.7f, 0.7f, $"{player.Health:0}/{player.MaxHealth:0}").Draw(spriteBatch);
                    UI.GetText("MPText", "General", 230, 110, z++, 0.7f, 0.7f, $"{player.Mana:0}/{player.MaxMana:0}").Draw(spriteBatch);
                    // Rune slot
                    UI.GetFrame("RuneSlot", 20, 916, z++, 120, 120).Draw(spriteBatch);

                    // Updates all living entities in the room.
                    // TODO: Should be all IEntities.
                    Animation.UpdateAll();

                    // Draws all other UI components
                    UI.DrawAllComponents();
                    break;

                case GameStates.Pause:
                    GraphicsDevice.Clear(Color.Black);
                    // Pause menu background image
                    UI.GetFrame("PauseMenu", 0, 0, 0, 1920, 1080).Draw(spriteBatch);
                    intro = "";
                    resumeButton.Draw(gameTime, spriteBatch);
                    mainMenuButton.Draw(gameTime, spriteBatch);
                    break;
                case GameStates.Defeat:
                    GraphicsDevice.Clear(Color.Black);
                    UI.GetFrame("defeatscreen blank", 0, 0, 0, 1920, 1080).Draw(spriteBatch);

                    exitDef.Draw(gameTime, spriteBatch);
                    mainMenuButtonDef.Draw(gameTime, spriteBatch);
                    playAgainDef.Draw(gameTime, spriteBatch);
                    break;

                case GameStates.Victory:
                    GraphicsDevice.Clear(Color.Black);
                    UI.GetFrame("victoryscreenblank", 0, 0, 0, 1920, 1080).Draw(spriteBatch);

                    exitVic.Draw(gameTime, spriteBatch);
                    mainMenuButtonVic.Draw(gameTime, spriteBatch);
                    playAgainVic.Draw(gameTime, spriteBatch);
                    break;

                default:
                    break;
            }

            // Display the instructions for current mode.
            SpriteFont font = UI.GetFont("LabelFont");
            spriteBatch.DrawString_(
                font,
                intro,
                new Vector2(20, windowHeight - font.MeasureString(intro).Y), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
            #endregion
        }

        /// <summary>
        /// Initializes or resets necessary components when the game starts or restarts
        /// </summary>
        private void ResetGame()
        {
            ClearTasks();
            // Every time a new run is started, adds up play times which affects player's some ability.
            playTimes++;

            Entity.Reset();
            Room.Reset();

            // Test code for room system
            if (CurrentRoom == null)
            {
                Room.GenerateRoom("MainHallV1");
            }
            //CurrentRoom = Room.Rooms["MainHallV1"];
            CurrentRoom = Room.Rooms["BossRoom"];

            // Test code for player
            player = new Player(new Rectangle(0, 0, 22, 36), 7.5f, new Vector2(1, 1), "Stand", 100, 100, "Servion") { Scale = 4 };
            Logger.Log($"The play has {ReplayTimes} replay times!");
            player.Stats.Add(EntityStats.Attack, 20 + ReplayTimes * 5);
            Entity.Register(CurrentRoom, player);
            player.Location = Utility.AlignCenters(new Rectangle(0, 0, windowWidth, windowHeight), player.BoundingBox).ToPoint();
            player.Location += new Point(0, 350);
            // Test code for enemy
            // enemy = new Enemy(new Rectangle(200, 200, 22, 36), 5f, new Vector2(1, 1), "Stand", 30, 30, "OldMan") { Scale = 3.5f };
            // Entity.Register(CurrentRoom.Name, enemy);
            // Test code for Runevark
            runevark = new Runevark(new Rectangle(600, 200, 30, 72), 0, new Vector2(1, 1), "Stand", 500, 500, "Runevark") { Scale = 6 };
            //Entity.Register(CurrentRoom, runevark);
            Entity.Register("BossRoom", runevark);
            runevark.Location = Utility.AlignCenters(runevark.Room.Self, runevark.BoundingBox).ToPoint();
            runevark.Location -= new Point(0, 100);
        }

        public void AddTask(Task task)
        {
            tasks.Add(task);
            task.Start();
        }

        private void ClearTasks()
        {
            foreach (Task task in tasks)
            {
                task.Dispose();
            }
            tasks.Clear();
        }
    }
}
