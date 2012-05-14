using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using MattsMenuLibrary;
using MouseHandlerLibrary;
using MattsButtonLibrary;
using ClickableMenu;

namespace Bubbel_Shot
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Constants

        //default width, default height
        private const int DEFAULT_WIDTH = 800;
        private const int DEFAULT_HEIGHT = 600;

        #endregion

        #region Variables

        //The Screens
        GameScreen activeScreen;


        //'Game' variables
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        int screenWidth, screenHeight;
        Vector2 baseScreenSize = new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT);
        Random random = new Random();

        //Playing field layout
        private int ballWidth = 36;
        private int halfBallWidth = 18;
        private int leftPadding = 2;
        private int horizontalPadding = 2;
        private int rowSpacing = 34;
        private const int collisionTolerance = 31;
        int leftSide = 200;
        int rightSide = 600;


        //Game setup variables
        private const int topInitiallyAlignedRight = 1;
        private const float percentBlank = 0.1f;

        //the menus
        GameMenu mainMenu;
        ButtonMenu pauseMenu;
        ButtonMenu gameOverMenu;

        //The various textures and fonts
        Texture2D pauseMenuImage;
        Texture2D pauseMenuExitButton;
        Texture2D pauseMenuResumeButton;
        Texture2D backgroundTexture;
        Texture2D foregroundTexture;
        //TODO add foreground origins, drawing locations as calculated for playing field here
        Vector2 leftAnchor = new Vector2(0, 0);
        Vector2 rightAnchor = new Vector2(0, 0);

        Texture2D cannonFrame;
        Texture2D cannonBodyFore;
        Texture2D cannonBodyBack;
        Texture2D bubbelTexture;
        Texture2D cursorTexture;
        Texture2D currentModePanel;
        Texture2D mainMenuButtonTexture;
        Texture2D feedTexture;
        Texture2D restartButtonTexture;
        Texture2D gameoverFailTexture;
        Texture2D gameoverSuccessTexture;
        SpriteFont scoreFont;

        //the point on the screen which the cannon spins around
        private Vector2 cannonFulcrum = new Vector2(400, 525);

        private Vector2 zeroOrigin = new Vector2(0, 0);
        private Vector2 cannonFrameOrigin = new Vector2(86, 82);
        private Vector2 cannonBodyOrigin = new Vector2(28, 90);
        private Vector2 bubbelOrigin = new Vector2(18, 18);
        
        //Audio variables
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;

        //Mouse handlers
        MouseHandler targettingMouseHandler;
        MouseHandler leftSideMouseHandler;
        MouseHandler rightSideMouseHandler;
        MouseHandler menuMouseHandler;
        

        CannonData cannonData;
        private float sensitivity = 0.01f;

        //game flow control
        bool gameRunning = false;
        bool shotFired = false;
        bool shotLanded = false;
        bool bubbelsPopping = false;
        bool bubbelsFalling = false;
        bool needToCheckColours = false;
        bool shotReloading = false;

        //score
        Score score;

        //Difficulty settings
        int missedShotsAllowed;

        //Shot data
        Vector2 shotDirection;
        Vector2 shotLocation;
        float shotSpeed = 6.5f;
        Color shotColor;
        Point justAddedBubbel;
        int shotReloadingStage=0;

        //bubbels popping and dropping
        FallingBubbels fallingBubbels;
        FallingParticleEngine fallingParticleEngine;
        PoppingBubbels poppingBubbels;
        PoppingParticleEngine poppingParticleEngine;

        private int popInterval = 4;

        //Playing Field Data
        private List<Color> availableColours;
        int topAlignedRight;
        private Color[,] playingField;


        GameButton mainMenuButton;

        #endregion

        #region Constructor

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            CreateButtons();
            CreateParticleSystems();
            CreateInGameMouse();
            //Create menus
            //CreatePauseMenu(); //PMM
            CreatePauseMenu();
            CreateMainMenu();
            CreateGameOverMenu();
            //CreateGameFinishedMenu();
            CreateMenuMouse();
            this.Deactivated += new EventHandler<System.EventArgs>(Game1_Deactivated);

        }

        void Game1_Deactivated(object sender, EventArgs e)
        {
            PauseGame();
        }

        private void CreateButtons()
        {
            mainMenuButton = new GameButton(this, PauseGame, new Rectangle(30, 510, 130, 50));
            Components.Add(mainMenuButton);
        }

        private void CreateParticleSystems()
        {
            //create bubbel popping particle system
            poppingParticleEngine = new PoppingParticleEngine(this, Content);
            Components.Add(poppingParticleEngine);

            fallingParticleEngine = new FallingParticleEngine(this);
            Components.Add(fallingParticleEngine);
        }

        private void CreateInGameMouse()
        {
            //create targetting mouse
            targettingMouseHandler = new MouseHandler(this);
            Components.Add(targettingMouseHandler);

            leftSideMouseHandler = new MouseHandler(this);
            Components.Add(leftSideMouseHandler);

            rightSideMouseHandler = new MouseHandler(this);
            Components.Add(rightSideMouseHandler);

        }

        private void CreateMainMenu()
        {
            //create main menu
            mainMenu = new GameMenu(this, "Bubbel");

            //set menu items
            mainMenu.AddMenuItem("New classic game", StartNewClassicGame);
            mainMenu.AddMenuItem("Exit", ExitGame);

            //Set background
            mainMenu.SetBackground(null, 210, Color.White);

            Components.Add(mainMenu);
        }

        private void CreatePauseMenu()
        {
            pauseMenu = new ButtonMenu(this);
            
            //Set pause and resume methods
            pauseMenu.SetPauseMethod(PauseGame);
            pauseMenu.SetResumeMethod(ResumeGame);
            pauseMenu.SetMenuHotkey(Keys.Escape);

            //Add menu to the game's components
            Components.Add(pauseMenu);

            //The buttons and menu texture are set after they are loaded in the load method.
        }

        private void CreateGameOverMenu()
        {
            gameOverMenu = new ButtonMenu(this);
            gameOverMenu.SetMenuHotkey(Keys.None);
            //Setup game over menu (accessed automatically when the game ends

            //Add menu to the game's components
            Components.Add(gameOverMenu);
        }

        private void CreateMenuMouse()
        {
            menuMouseHandler = new MouseHandler(this);
            Components.Add(menuMouseHandler);
        }



        #endregion



        #region Initialisation

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Set display parameters
            graphics.PreferredBackBufferWidth = DEFAULT_WIDTH;
            graphics.PreferredBackBufferHeight = DEFAULT_HEIGHT;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            //Set window title
            Window.Title = "Bubbel";

            StartNewClassicGame();


            base.Initialize();

            //set up mice. something means they have to set up after initialized. TODO
            InitialiseMouses();
            
        }

        private void InitialiseMouses()
        {
            //setup targeting mouse
            targettingMouseHandler.SetActiveRegion(new Rectangle(leftSide, 0, rightSide - leftSide, screenHeight));
            targettingMouseHandler.SetCursorMode(CursorMode.Crosshair);
            targettingMouseHandler.AddMouseMovedAction(TargetWithMouse);
            targettingMouseHandler.AddLeftClickAction(FireShotWithMouse);
            targettingMouseHandler.Enable();

            leftSideMouseHandler.SetActiveRegion(new Rectangle(0, 0, leftSide, screenHeight));
            leftSideMouseHandler.Enable();

            rightSideMouseHandler.SetActiveRegion(new Rectangle(rightSide, 0, screenWidth-rightSide, screenHeight));
            rightSideMouseHandler.Enable();

            menuMouseHandler.SetActiveRegion(new Rectangle(0, 0, screenWidth, screenHeight));
        }

        /// <summary>
        /// Generates a random field of balls
        /// TODO - ensure no balls are left floating
        /// </summary>
        private void GeneratePlayingField(int initialRowCount)
        {
            topAlignedRight = topInitiallyAlignedRight;
            playingField = new Color[10,14];

            for (int rowNumber = 0; rowNumber < initialRowCount; rowNumber++)
            {
                GenerateRow(rowNumber);
            }
        }

        private void GenerateRow(int rowNumber)
        {
            for (int j = 0; j < 10; j++)
            {
                int x = random.Next(availableColours.Count);
                playingField[j, rowNumber] = availableColours[x];
                //makes a certain percentage of the slots empty
                if (random.NextDouble() < percentBlank)
                {
                    playingField[j, rowNumber] = Color.Transparent;
                }
            }
        }

        #endregion



        #region Game flow methods

        public void PauseGame()
        {
            pauseMenu.ShowMenu();
            poppingParticleEngine.Pause();
            //set mice correctly
            menuMouseHandler.Enable();
            targettingMouseHandler.Disable();
            leftSideMouseHandler.Disable();
            rightSideMouseHandler.Disable();

            gameRunning = false;
        }

        public void ResumeGame()
        {
            pauseMenu.HideMenu();
            poppingParticleEngine.Resume();
            //set mice correctly
            menuMouseHandler.Disable();
            targettingMouseHandler.Enable();
            leftSideMouseHandler.Enable();
            rightSideMouseHandler.Enable();

            gameRunning = true;
        }

        public void GameOver(bool hasWon)
        {
            pauseMenu.SetMenuHotkey(Keys.None);
            gameRunning = false;
            gameOverMenu.SetBackground(null, 128, Color.White);
            menuMouseHandler.Enable();
            targettingMouseHandler.Disable();
            leftSideMouseHandler.Disable();
            rightSideMouseHandler.Disable();

            if (hasWon)
            {
                gameOverMenu.SetBackground(gameoverSuccessTexture, 255, Color.Green);
                gameOverMenu.ShowMenu();
            }
            else
            {
                gameOverMenu.SetBackground(gameoverFailTexture, 255, Color.Red);
                gameOverMenu.ShowMenu();
            }

        }

        private void StartNewClassicGame()
        {
            score = new Score();

            DifficultySettings settings = new DifficultySettings();
            StartNewLevel(settings);

            ResumeGame();
        }

        private void StartNewLevel(DifficultySettings settings)
        {
            gameRunning = true;
            shotFired = false;
            shotLanded = false;
            bubbelsPopping = false;
            bubbelsFalling = false;
            needToCheckColours = false;

            gameOverMenu.HideMenu();
            pauseMenu.HideMenu();

            //Setup initial available colours
            List<Color> sensibleColours = new List<Color>();
            sensibleColours.Add(Color.HotPink);
            sensibleColours.Add(Color.Yellow);
            sensibleColours.Add(Color.Indigo);
            sensibleColours.Add(Color.Chartreuse);
            sensibleColours.Add(Color.MediumOrchid);
            sensibleColours.Add(Color.DodgerBlue);
            sensibleColours.Add(Color.Crimson);
            sensibleColours.Add(new Color(128,0,64));

            availableColours = new List<Color>();
            for (int i = 0; i < settings.NumberOfDifferentBallColours; i++)
            {
                int sensibleIndex = random.Next(0, sensibleColours.Count-1);
                while (availableColours.Contains(sensibleColours[sensibleIndex]))
                {
                    sensibleIndex = random.Next(0, sensibleColours.Count-1);
                }
                availableColours.Add(sensibleColours[sensibleIndex]);
            }

            missedShotsAllowed = settings.MissedShotsAllowed;

            //generate the playing field
            GeneratePlayingField(settings.InitialRowCount);

            //set up cannon
            cannonData = new CannonData();
            cannonData.currentBallColor = availableColours[random.Next(availableColours.Count)];
            cannonData.nextFiveShotLocation = new Vector2[5];
            cannonData.nextFiveShotLocation[0] = new Vector2(618, 568);
            cannonData.nextFiveShotLocation[1] = new Vector2(658, 568);
            cannonData.nextFiveShotLocation[2] = new Vector2(698, 568);
            cannonData.nextFiveShotLocation[3] = new Vector2(738, 568);
            cannonData.nextFiveShotLocation[4] = new Vector2(778, 568);


            cannonData.nextFiveShots = new List<Color>();
            for (int i = 0; i < 5; i++)
            {
                cannonData.nextFiveShots.Add(availableColours[random.Next(availableColours.Count)]);
            }
        }

        public void ExitGame()
        {
            //may neeed to flush scores to hard disk/network etc
            this.Exit();
        }

        #endregion



        #region Internal Methods

        private void TargetWithMouse(Vector2 target)
        {
            Vector2 fulcrumToMouse = cannonFulcrum - target;
            cannonData.Angle = -1 * (float)Math.Atan(fulcrumToMouse.X / fulcrumToMouse.Y);
            if (cannonData.Angle < -1.1)
                cannonData.Angle = -1.1f;
            if (cannonData.Angle > 1.1)
                cannonData.Angle = 1.1f;
        }

        private void FireShotWithMouse(Vector2 mousePoint)
        {
            if (!shotFired && !shotLanded && !bubbelsPopping && !bubbelsFalling && !shotReloading)
            {
                Vector2 fulcrumToMouse = cannonFulcrum - mousePoint;
                cannonData.Angle = -1 * (float)Math.Atan(fulcrumToMouse.X / fulcrumToMouse.Y);
                if (cannonData.Angle < -1.1)
                    cannonData.Angle = -1.1f;
                if (cannonData.Angle > 1.1)
                    cannonData.Angle = 1.1f;
                FireShot();
            }
        }

        /// <summary>
        /// Fires a shot from the current cannon angle
        /// </summary>
        private void FireShot()
        {
            //calculate the shot direction from the cannon angle
            //-90 gives 0 as the upwards angle
            float shotAngle = cannonData.Angle + MathHelper.ToRadians(-90);
            shotDirection = new Vector2((float)Math.Cos(shotAngle) * shotSpeed, (float)Math.Sin(shotAngle) * shotSpeed);
            //start shot from cannon centre
            shotLocation = cannonFulcrum;
            //set colour of current ball to that of the cannon
            shotColor = cannonData.currentBallColor;
            soundBank.PlayCue("cannon");

            //load next shot into cannon
            LoadNextShot();

            shotFired = true;
        }

        /// <summary>
        /// Loads the next shot from the queue into the cannon
        /// and replenishes the queued shots
        /// </summary>
        private void LoadNextShot()
        {
            shotReloading = true;
            cannonData.currentBallColor = cannonData.nextFiveShots[0];
            cannonData.nextFiveShots.RemoveAt(0);
            cannonData.nextFiveShots.Add(availableColours[random.Next(availableColours.Count)]);
        }

        /// <summary>
        /// Changes the shot direction as if it bounced off a vertical wall
        /// Also plays a bounce sound effect.
        /// </summary>
        private void BounceOffWall()
        {
            shotDirection.X = shotDirection.X * -1;
            //TODO - add a bounce sound effect
            soundBank.PlayCue("bounce");
        }

        /// <summary>
        /// Tests for collisions between the shot and the top of the screen,
        /// and between the shot and the playing field balls.
        /// </summary>
        /// <returns> <code>true</code> if there is a collision</returns>
        private bool ShotCollided()
        {
            Vector2 focusBall;
            Vector2 distance;

            //check for collisions with the top of the screen

            if (shotLocation.Y < 18)
            {
                return true;
            }

            for (int i = 0; i < playingField.GetLength(1); i++)
            {
                int y = i * rowSpacing;
                for (int j = 0; j < playingField.GetLength(0); j++)
                {
                    //For each bubble position in the gameboard that is not null
                    if (playingField[j, i] != Color.Transparent)
                    {
                        focusBall = new Vector2(j * (36 + horizontalPadding) + leftSide + leftPadding + (18 * ((i + topAlignedRight) % 2)), y);
                        focusBall += new Vector2(18, 18);
                        distance = shotLocation - focusBall;
                        if (distance.Length() < collisionTolerance)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Adds the shot bubbel to the field at the nearest location
        /// Recursive. Moves ball backward until it goes into a clear spot.
        /// </summary>
        private void AddShotBubbelToField()
        {
            //add the bubbel to the field at the nearest available spot
            //get the 'y' row
            int i = (int)Math.Floor(shotLocation.Y / rowSpacing);
            //get the 'x' column
            int j = (int)Math.Floor((shotLocation.X - 18 * ((i + topAlignedRight) % 2) - leftPadding - leftSide)) / (36 + horizontalPadding);

            if (i >= playingField.GetLength(1))
                i = playingField.GetLength(1) - 1;
            if (j >= playingField.GetLength(0))
                j = playingField.GetLength(0) - 1;

            if (playingField[j, i] == Color.Transparent)
            {
                playingField[j, i] = shotColor;
                justAddedBubbel = new Point(j, i);
            }
            else
            {
                //move bubbel backwards and try again
                shotLocation = shotLocation - shotDirection;

                if (shotLocation.X < leftSide + 18)
                {
                    shotDirection.X = shotDirection.X * -1;
                }
                else if (shotLocation.X > rightSide - 18)
                {
                    shotDirection.X = shotDirection.X * -1;
                }

                AddShotBubbelToField();
            }
            
        }

        #endregion



        #region Load Methods

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            LoadTextures();
            LoadAudio();
            LoadFonts();



            screenWidth = (int)baseScreenSize.X;
            screenHeight = (int)baseScreenSize.Y;
        }

        private void LoadTextures()
        {
            backgroundTexture = Content.Load<Texture2D>("Background");
            foregroundTexture = Content.Load<Texture2D>("Frame");
            cannonFrame = Content.Load<Texture2D>("CannonFrame");
            cannonBodyFore = Content.Load<Texture2D>("CannonBodyFore");
            cannonBodyBack = Content.Load<Texture2D>("CannonBodyBack");
            bubbelTexture = Content.Load<Texture2D>("Bubbel");
            cursorTexture = Content.Load<Texture2D>("Cursor");
            currentModePanel = Content.Load<Texture2D>("CurrentModePanel");

            pauseMenuImage = Content.Load<Texture2D>("PauseMenuBackground");
            pauseMenuResumeButton = Content.Load<Texture2D>("PauseMenuResumeButton");
            pauseMenuExitButton = Content.Load<Texture2D>("PauseMenuExitButton");

            mainMenuButtonTexture = Content.Load<Texture2D>("MenuButton");

            restartButtonTexture = Content.Load<Texture2D>("GameOverPlayAgainButton");
            gameoverFailTexture = Content.Load<Texture2D>("GameOverFailure");
            gameoverSuccessTexture = Content.Load<Texture2D>("GameOverSuccess");

            feedTexture = Content.Load<Texture2D>("Feed");

            //Set menu items
            pauseMenu.AddMenuItem(new MenuButton("Resume", pauseMenuResumeButton, new Rectangle(330, 250, 140, 50), ResumeGame));
            pauseMenu.AddMenuItem(new MenuButton("Exit", pauseMenuExitButton, new Rectangle(330, 320, 140, 50), ExitGame));


            gameOverMenu.AddMenuItem(new MenuButton("Resume", restartButtonTexture, new Rectangle(330, 250, 140, 50), StartNewClassicGame));
            gameOverMenu.AddMenuItem(new MenuButton("Exit", pauseMenuExitButton, new Rectangle(330, 320, 140, 50), ExitGame));

            //Set pause menu background
            pauseMenu.SetBackground(pauseMenuImage, 255, Color.White);

            mainMenuButton.SetButtonImage(mainMenuButtonTexture);
            
        }

        private void LoadAudio()
        {
            audioEngine = new AudioEngine("Content/bubbelAudio.xgs");
            waveBank = new WaveBank(audioEngine, "Content/bubbelWaveBank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/bubbelSoundBank.xsb");
        }

        private void LoadFonts()
        {
            scoreFont = Content.Load<SpriteFont>("scoreFont");
        }

        #endregion



        #region Unload Methods

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        #endregion



        #region Update Methods

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();


            if (gameRunning)
            {
                ProcessKeyboard();
                UpdateShot();
                PostShotLandingProcessing();
                AnimateBubbelsPopping();
                DropBubbels();
                SpinLoadedShot();
                TestGameOver();
                CheckFaultCount();
                audioEngine.Update();
            }
            if (gameRunning)
            {
                RemoveColoursNotOnField();
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Processes the keyboard for user input
        /// Allows the user to control the cannon using left and right keys and space to fire
        /// </summary>
        private void ProcessKeyboard()
        {
            KeyboardState kbState = Keyboard.GetState();
            if (kbState.IsKeyDown(Keys.Left))
            {
                cannonData.Angle -= sensitivity;
                if (cannonData.Angle < -1.1)
                    cannonData.Angle = -1.1f;
            }
            if (kbState.IsKeyDown(Keys.Right))
            {
                cannonData.Angle += sensitivity;
                if (cannonData.Angle > 1.1)
                    cannonData.Angle = 1.1f;
            }
            if (kbState.IsKeyDown(Keys.Space) && !shotFired && !shotLanded && !bubbelsPopping && !bubbelsFalling)
            {
                FireShot();
            }
        }

        /// <summary>
        /// when shot is fired, update shot updates the shot's location,
        /// bouncing it off walls and checking for collisions with the ball field
        /// </summary>
        private void UpdateShot()
        {
            if (shotFired)
            {
                shotLocation = shotDirection + shotLocation;
                //if shot is to the left of the play area, invert x
                if (shotLocation.X < leftSide + 18)
                {
                    BounceOffWall();
                }
                //if shot is to the right of the play area, invert x
                else if (shotLocation.X > rightSide - 18)
                {
                    BounceOffWall();
                }

                if (ShotCollided())
                {
                    AddShotBubbelToField();
                    shotFired = false;
                    shotLanded = true;
                }
            }
        }


        /// <summary>
        /// Does work after a shot lands, calculates popping and falling
        /// bubbels
        /// </summary>
        private void PostShotLandingProcessing()
        {
            if (shotLanded)
            {
                List<Point> unexploredPoints = new List<Point>();

                //wipe the bubbels popping list
                poppingBubbels.points = new List<Point>();
                poppingBubbels.colours = new List<Color>();
                fallingBubbels = new FallingBubbels();
                fallingBubbels.points = new List<Point>();
                fallingBubbels.positions = new List<Vector2>();
                fallingBubbels.color = new List<Color>();

                //loop collects the bubbles of the same colour popping
                EvaluatePoppingBubbels();

                //if any bubbels have popped
                if (bubbelsPopping)
                {
                    EvaluateFallingBubbels();
                    score.Hit();
                }
                else
                {
                    score.Miss();
                    score.shotsMissed++;
                }

                //end shot landed phase
                shotLanded = false;
            }
        }

        /// <summary>
        /// Finds popping bubbels, if a sufficiently large chain is made, bubbels are added 
        /// to the popping list and removed from the playing field
        /// </summary>
        private void EvaluatePoppingBubbels()
        {
            //build up the list of 'popping bubbles' - neighbours of the same colour
            //or neighbours of neighbours that are of the same colour (chains)
            List<Point> unexploredPoints = new List<Point>();
            //The just added bubble is first part of the chain
            unexploredPoints.Add(justAddedBubbel);

            Point currentPoint;
            List<Point> neighboursOfCurrentPoint;
            while (unexploredPoints.Count > 0)
            {
                currentPoint = unexploredPoints[0];
                neighboursOfCurrentPoint = GetSameColourNeighbours(currentPoint);

                //add new neighbours to poppingBubbels
                //and to unexplored
                foreach (Point p in neighboursOfCurrentPoint)
                {
                    if (!poppingBubbels.points.Contains(p))
                    {
                        unexploredPoints.Add(p);
                        poppingBubbels.points.Add(p);
                    }
                }
                unexploredPoints.RemoveAt(0);
            }

            //if any bubbels have popped
            if (poppingBubbels.points.Count > 2)
            {
                //make the popping bubbels transparent in the playing field
                foreach (Point p in poppingBubbels.points)
                {
                    poppingBubbels.colours.Add(playingField[p.X, p.Y]);
                    playingField[p.X, p.Y] = Color.Transparent;
                }

                //make it so the popping is started
                bubbelsPopping = true;
            }
        }

        /// <summary>
        /// Calculates any bubbels that will drop and adds them to the score
        /// </summary>
        private void EvaluateFallingBubbels()
        {
            Point currentPoint;
            List<Point> neighboursOfCurrentPoint;
            List<Point> secureBubbels = new List<Point>();
            List<Point> unexploredSecureBubbels = new List<Point>();

            //add top row of bubbels to unexplored bubbels
            for (int j = 0; j < playingField.GetLength(0); j++)
            {
                if (playingField[j, 0] != Color.Transparent && !poppingBubbels.points.Contains(new Point(j,0)))
                {
                    Point temp = new Point(j, 0);
                    secureBubbels.Add(temp);
                    unexploredSecureBubbels.Add(temp);
                }
            }


            //for each bubbel in the top row
            //that has not been explored
            //get all neighbours 
            while (unexploredSecureBubbels.Count > 0)
            {
                currentPoint = unexploredSecureBubbels[0];
                neighboursOfCurrentPoint = GetNoneTransparentNeighbours(currentPoint);

                //add new neighbours to unexplored and to secure
                foreach (Point p in neighboursOfCurrentPoint)
                {
                    if (!secureBubbels.Contains(p))
                    {
                        unexploredSecureBubbels.Add(p);
                        secureBubbels.Add(p);
                    }
                }
                unexploredSecureBubbels.RemoveAt(0);
            }

            score.numberDropped = 0;

            //any bubbel not in the secure list, is dropped.
            for (int i = 0; i < playingField.GetLength(1); i++)
            {
                for (int j = 0; j < playingField.GetLength(0); j++)
                {
                    if (playingField[j, i] != Color.Transparent && !secureBubbels.Contains(new Point(j, i)) && !poppingBubbels.points.Contains(new Point(j, i)))
                    {
                        score.numberDropped++;
                        fallingBubbels.points.Add(new Point(j, i));
                        fallingBubbels.positions.Add(VectorFromPoint(j, i));
                        fallingBubbels.color.Add(playingField[j, i]);
                    }
                }
            }
        }

        /// <summary>
        /// Gets neighbours of the centreBubbel which are the same colour
        /// </summary>
        /// <param name="centreBubbel">A point representing the bubbel you 
        /// wish to find the neighbours of</param>
        /// <returns>A list of points representing neighbours of
        /// the same colour</returns>
        private List<Point> GetSameColourNeighbours(Point centreBubbel)
        {
            Color centreBubbelColor = playingField[centreBubbel.X, centreBubbel.Y];
            List<Point> sameColourNeighbours = GetNeighbours(centreBubbel);
            for (int i = sameColourNeighbours.Count - 1; i >= 0; i--)
            {
                if (playingField[sameColourNeighbours[i].X, sameColourNeighbours[i].Y] != centreBubbelColor)
                    sameColourNeighbours.RemoveAt(i);
            }
            return sameColourNeighbours;
        }

        /// <summary>
        /// Gets none transparent neighbours. Popping bubbels
        /// are classed as transparent
        /// </summary>
        /// <param name="centreBubbel"></param>
        /// <returns></returns>
        private List<Point> GetNoneTransparentNeighbours(Point centreBubbel)
        {
            List<Point> nonTransparentNeighbours = GetNeighbours(centreBubbel);
            for (int i = nonTransparentNeighbours.Count - 1; i >= 0; i--)
            {
                if (playingField[nonTransparentNeighbours[i].X, nonTransparentNeighbours[i].Y] == Color.Transparent
                    || poppingBubbels.points.Contains(nonTransparentNeighbours[i]))
                {
                    nonTransparentNeighbours.RemoveAt(i);
                }
            }
            return nonTransparentNeighbours;
        }

        /// <summary>
        /// Finds the graphical 6 neighbours
        /// </summary>
        /// <param name="centreBubbel">the bubble you wish to find the
        /// neighbours of</param>
        /// <returns>a list of the neighbouring bubbles</returns>
        private List<Point> GetNeighbours(Point centreBubbel)
        {
            List<Point> neighbours = new List<Point>();
            if (centreBubbel.X > 0)
                neighbours.Add(new Point(centreBubbel.X - 1, centreBubbel.Y));
            if (centreBubbel.X < playingField.GetLength(0) - 1)
                neighbours.Add(new Point(centreBubbel.X + 1, centreBubbel.Y));
            if (centreBubbel.Y > 0)
                neighbours.Add(new Point(centreBubbel.X, centreBubbel.Y - 1));
            if (centreBubbel.Y < playingField.GetLength(1) - 1)
                neighbours.Add(new Point(centreBubbel.X, centreBubbel.Y + 1));


            if (centreBubbel.Y % 2 == 0)
            {
                //if an odd row
                if (topAlignedRight == 1)
                {
                    //this row is aligned right
                    if (centreBubbel.X < playingField.GetLength(0) - 1)
                    {
                        if (centreBubbel.Y > 0)
                            neighbours.Add(new Point(centreBubbel.X + 1, centreBubbel.Y - 1));
                        if (centreBubbel.Y < playingField.GetLength(1) - 1)
                            neighbours.Add(new Point(centreBubbel.X + 1, centreBubbel.Y + 1));
                    }
                }
                else
                {
                    //this row is aligned left
                    if (centreBubbel.X > 0)
                    {
                        if (centreBubbel.Y > 0)
                            neighbours.Add(new Point(centreBubbel.X - 1, centreBubbel.Y - 1));
                        if (centreBubbel.Y < playingField.GetLength(1) - 1)
                            neighbours.Add(new Point(centreBubbel.X - 1, centreBubbel.Y + 1));
                    }
                }
            }
            else
            {
                //if an even row
                if (topAlignedRight == 0)
                {
                    //this row is aligned right
                    if (centreBubbel.X < playingField.GetLength(0) - 1)
                    {
                        if (centreBubbel.Y > 0)
                            neighbours.Add(new Point(centreBubbel.X + 1, centreBubbel.Y - 1));
                        if (centreBubbel.Y < playingField.GetLength(1) - 1)
                            neighbours.Add(new Point(centreBubbel.X + 1, centreBubbel.Y + 1));
                    }

                }
                else
                {
                    //this row is aligned left
                    if (centreBubbel.X > 0)
                    {
                        if (centreBubbel.Y > 0)
                            neighbours.Add(new Point(centreBubbel.X - 1, centreBubbel.Y - 1));
                        if (centreBubbel.Y < playingField.GetLength(1) - 1)
                            neighbours.Add(new Point(centreBubbel.X - 1, centreBubbel.Y + 1));
                    }
                }
            }

            return neighbours;
        }

        /// <summary>
        /// Pops bubbels one by one, playing the pop sound and incrementing score.
        /// Once bubbels all bubbels are popped move on to bubbel dropping
        /// </summary>
        private void AnimateBubbelsPopping()
        {
            if (bubbelsPopping)
            {
                if (poppingBubbels.currentlyPoppingProgress == 0)
                {
                    soundBank.PlayCue("pop1");

                    //add to score
                    int scoreForThisBubbel = score.Pop();

                    int next = random.Next(poppingBubbels.points.Count - 1);
                    poppingBubbels.currentlyPoppingProgress = 0;
                    poppingBubbels.currentlyPopping = poppingBubbels.points[next];
                    poppingBubbels.currentlyPoppingColor = poppingBubbels.colours[next];
                    poppingParticleEngine.AddBubbel(new BubbelParticle(VectorFromPoint(poppingBubbels.currentlyPopping), poppingBubbels.currentlyPoppingColor, scoreForThisBubbel));
                    poppingBubbels.points.RemoveAt(next);
                    poppingBubbels.colours.RemoveAt(next);
                }

                poppingBubbels.currentlyPoppingProgress++;

                if (poppingBubbels.currentlyPoppingProgress > popInterval)
                {
                    poppingBubbels.currentlyPoppingProgress = 0;
                }


                if (poppingBubbels.points.Count == 0)
                {
                    score.numberPoppedInARow = 0;
                    poppingBubbels.currentlyPoppingProgress = 0;
                    bubbelsPopping = false;
                    bubbelsFalling = true;
                    foreach (Point p in fallingBubbels.points)
                    {
                        playingField[p.X, p.Y] = Color.Transparent;
                    }
                }
            }
        }

        /// <summary>
        /// Adds any unattached bubbels to falling bubbel particle engine
        /// </summary>
        private void DropBubbels()
        {
            if (bubbelsFalling)
            {
                for (int i = 0; i < fallingBubbels.positions.Count; i++)
                {
                    fallingParticleEngine.AddBubbel(new FallingParticle(fallingBubbels.positions[i], fallingBubbels.color[i]));
                }
                fallingBubbels = new FallingBubbels();
                //TODO - doens't this work? What is left todo...
                bubbelsFalling = false;
                needToCheckColours = true;
                fallingBubbels.accelerating = false;
                fallingBubbels.positions = new List<Vector2>();
            }
        }

        /// <summary>
        /// Checks the current colours in the playing field,
        /// ensure that availableColours, the cannon queue, 
        /// and the currently loaded cannon shot do not contain any colours
        /// no longer in the playing field.
        /// </summary>
        private void RemoveColoursNotOnField()
        {
            if (needToCheckColours)
            {
                //for each colour
                for (int c = availableColours.Count-1; c >= 0; c--)
                {
                    //if a colour is not in the playing field
                    if (!FieldContains(availableColours[c]))
                    {
                        RemoveColour(availableColours[c]);
                    }
                }
                needToCheckColours = false;
            }
        }

        /// <summary>
        /// Determines whether a color exists in the playing field
        /// </summary>
        /// <param name="color"></param>
        /// <returns>true when a colour is in the playing field;
        /// false when the colour is not in the playing field</returns>
        private bool FieldContains(Color color)
        {
            for (int i = 0; i < playingField.GetLength(1); i++)
            {
                for (int j = 0; j < playingField.GetLength(0); j++)
                {
                    if (playingField[j, i] == color)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the colour from the available colour list, the 
        /// queued shots, and the cannon
        /// </summary>
        /// <param name="color">the colour to be removed</param>
        private void RemoveColour(Color color)
        {
            //remove from available colours list
            availableColours.Remove(color);

            //remove it from the queued balls
            for (int i = cannonData.nextFiveShots.Count - 1; i >= 0; i--)
            {
                if (cannonData.nextFiveShots[i] == color)
                {
                    cannonData.nextFiveShots.RemoveAt(i);
                    cannonData.nextFiveShots.Add(availableColours[random.Next(availableColours.Count)]);
                }
            }

            //if currently loaded shot is removed colour load next shot into cannon
            if (cannonData.currentBallColor == color)
                LoadNextShot();
        }

        private void SpinLoadedShot()
        {
            cannonData.currentBallRotation += 0.1f;
        }

        /// <summary>
        /// Checks for game over conditions and if detected launches appropriate menu screen
        /// </summary>
        private void TestGameOver()
        {
            //if there's anything in the bottom row and it's not popping or falling
            //GAME OVER!
            if (!bubbelsPopping && !bubbelsPopping) //TODO wtf?
            {
                bool gameLost = false;
                bool gameWon = false;

                for (int j = 0; j < playingField.GetLength(0); j++)
                {
                    if (playingField[j, playingField.GetLength(1) - 1] != Color.Transparent)
                    {
                        gameLost = true;
                    }
                }

                if (gameLost)
                {
                    GameOver(false);
                    return;
                }



                //if there's nothing at all, and all popping is complete
                //SUCCESS!
                for (int j = 0; j < playingField.GetLength(0); j++)
                {
                    if (playingField[j, 0] != Color.Transparent)
                    {
                        gameWon = false;
                        break;
                    }
                    gameWon = true;
                }

                if (gameWon)
                {
                    GameOver(true);
                }
            }
        }

        private void CheckFaultCount()
        {
            if (score.shotsMissed > missedShotsAllowed)
            {
                score.shotsMissed = 0;
                DropField();
            }
        }

        private void DropField()
        {
            //move each row down
            for (int i = playingField.GetLength(1)-2; i >= 0; i--)
            {
                for (int j = 0; j < playingField.GetLength(0); j++)
                {
                    playingField[j, i + 1] = playingField[j, i];
                }
            }
            //change left align
            if (topAlignedRight == 1)
            {
                topAlignedRight = 0;
            }
            else
            {
                topAlignedRight = 1;
            }
            //add new top row
            GenerateRow(0);
            //drop hangers 
        }

        #endregion



        #region Draw methods

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            DrawGameFrame();
            DrawBallField();
            DrawPoppingBubbels();
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            DrawCannon();
            DrawQueue();
            DrawShot();
            DrawScore();
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawGameFrame()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            spriteBatch.Draw(backgroundTexture, screenRectangle, Color.White);

            //draw left side
            spriteBatch.Draw(foregroundTexture, leftAnchor, new Rectangle(0, 0, foregroundTexture.Width / 2, foregroundTexture.Height), Color.White);
            //draw right side
            spriteBatch.Draw(foregroundTexture, new Vector2(foregroundTexture.Width / 2, 0), new Rectangle(foregroundTexture.Width / 2, 0, foregroundTexture.Width / 2, foregroundTexture.Height), Color.White);

            //spriteBatch.Draw(currentModePanel, new Rectangle((int)leftAnchor.X+20, 100, 150, 220), Color.White);
        }

        private void DrawBallField()
        {

            for (int i = 0; i < playingField.GetLength(1); i++)
            {
                for (int j = 0; j < playingField.GetLength(0); j++)
                {
                    //If there is a ball at a location
                    if (playingField[j, i] != Color.Transparent)
                    {
                        //Draw it
                        spriteBatch.Draw(bubbelTexture, VectorFromPoint(j, i), playingField[j, i]);
                    }
                }
            }
        }

        private void DrawPoppingBubbels()
        {
            if (bubbelsPopping)
            {
                //draw 'existing' bubbels that are yet to be popped but will soon be popped
                for (int i = 0; i < poppingBubbels.points.Count; i++)
                {
                    spriteBatch.Draw(bubbelTexture, VectorFromPoint(poppingBubbels.points[i]), poppingBubbels.colours[i]);
                }
            }
        }

        private Vector2 VectorFromPoint(Point point)
        {
            return VectorFromPoint(point.X, point.Y);
        }

        private Vector2 VectorFromPoint(int j, int i)
        {
            return new Vector2(j * (ballWidth + horizontalPadding) + leftSide + leftPadding + (halfBallWidth * ((i + topAlignedRight) % 2)), i * rowSpacing);
        }

        private void DrawCannon()
        {
            //Fade bubbel into cannon
            if (shotReloading)
            {
                Color currentBallColorSemiTransparent = cannonData.currentBallColor;
                currentBallColorSemiTransparent.A = (byte)((float)shotReloadingStage/100.0f * 255.0f);
                spriteBatch.Draw(bubbelTexture, cannonFulcrum, null, currentBallColorSemiTransparent, cannonData.currentBallRotation, bubbelOrigin, (float)shotReloadingStage / 100.0f, SpriteEffects.None, 0.4f);
            }
            else
            {
                spriteBatch.Draw(bubbelTexture, cannonFulcrum, null, cannonData.currentBallColor, cannonData.currentBallRotation, bubbelOrigin, 1, SpriteEffects.None, 0.4f);
            }
            spriteBatch.Draw(cannonBodyFore, cannonFulcrum, null, Color.Wheat, cannonData.Angle, cannonBodyOrigin, 1, SpriteEffects.None, 0.1f);
            spriteBatch.Draw(cannonBodyBack, cannonFulcrum, null, Color.White, cannonData.Angle, cannonBodyOrigin, 1, SpriteEffects.None, 0.5f);
            spriteBatch.Draw(cannonFrame, cannonFulcrum, null, Color.White, 0, cannonFrameOrigin, 1, SpriteEffects.None, 0.6f);
        }

        private void DrawQueue()
        {

            if (shotReloading)
            {
                float percentageDone = (float)shotReloadingStage / 100.0f;
                //Fade out the bubbel quickly
                if (shotReloadingStage < 25)
                {
                    Color currentBallColorSemiTransparent = cannonData.currentBallColor;
                    currentBallColorSemiTransparent.A = (byte)(255 - percentageDone * 1023);
                    spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[0], null, currentBallColorSemiTransparent, 0.0f, bubbelOrigin, 1.0f , SpriteEffects.None, 0.9f);

                    spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[1], null, cannonData.nextFiveShots[0], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                    spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[2], null, cannonData.nextFiveShots[1], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                    spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[3], null, cannonData.nextFiveShots[2], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                    spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[4], null, cannonData.nextFiveShots[3], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                }
                //Slide next bubbels along
                else
                {
                    float percentageSlideDone = ((float)shotReloadingStage-25.0f) / 75.0f;
                    //move balls 40 right
                    Vector2 interpVec = new Vector2(40 * (1.0f-percentageSlideDone), 0);
                    spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[0] + interpVec, null, cannonData.nextFiveShots[0], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                    spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[1] + interpVec, null, cannonData.nextFiveShots[1], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                    spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[2] + interpVec, null, cannonData.nextFiveShots[2], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                    spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[3] + interpVec, null, cannonData.nextFiveShots[3], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                    spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[4] + interpVec, null, cannonData.nextFiveShots[4], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                }
                
                //spriteBatch.Draw(feedTexture, new Vector2(586, 537), null, Color.White, 0.0f, zeroOrigin, 1.0f, SpriteEffects.None, 0.5f);

                shotReloadingStage += 5;
                if (shotReloadingStage > 100)
                {
                    shotReloading = false;
                    shotReloadingStage = 0;
                }
            }
            else
            {
                //spriteBatch.Draw(feedTexture, new Vector2(586, 537), null, Color.White, 0.0f, zeroOrigin, 1.0f, SpriteEffects.None, 0.5f);
                spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[0], null, cannonData.nextFiveShots[0], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[1], null, cannonData.nextFiveShots[1], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[2], null, cannonData.nextFiveShots[2], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[3], null, cannonData.nextFiveShots[3], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
                spriteBatch.Draw(bubbelTexture, cannonData.nextFiveShotLocation[4], null, cannonData.nextFiveShots[4], 0.0f, bubbelOrigin, 1.0f, SpriteEffects.None, 0.9f);
            }
        }

        private void DrawShot()
        {
            if (shotFired)
            {
                spriteBatch.Draw(bubbelTexture, shotLocation, null, shotColor, 0, new Vector2(18, 18), 1, SpriteEffects.None, 0.3f);
            }
        }

        private void DrawScore()
        {
            spriteBatch.DrawString(scoreFont, "Score: " + score.currentTotalScore, new Vector2(45, 15), Color.Maroon);
        }


        #endregion

    }
}
