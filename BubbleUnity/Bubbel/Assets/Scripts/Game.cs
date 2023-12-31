using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Bubbel_Shot
{
    /// <summary>
    /// Bubbel needs to be in 800x600. Old times! So the cannon in the middle is at x= 400. XNA 0,0 is TOP LEFT.
    /// </summary>
    public class Game : MonoBehaviour
    {
        private readonly Color TransparentBlack = new Color(0,0,0, 0);

        [SerializeField] private SpriteRenderer bubbelPrefab;

        private List<SpriteRenderer> bubbelGridVisuals = new();

        //Playing field layout
        private int ballWidth = 36;
        private int halfBallWidth => ballWidth / 2;
        private int leftPadding = 2;
        private int horizontalPadding = 2;
        private int rowSpacing = 34;
        private const int collisionTolerance = 31;
        private int leftSide = 200;
        private int rightSide = 600;
        [SerializeField] private Vector3 scaleFactor = Vector3.one;


        //Game setup variables
        private const int topInitiallyAlignedRight = 1;
        private const float percentBlank = 0.1f;

        //the menus
        [SerializeField] private MenuController menuPrefab;
        private MenuController menu;

        [SerializeField] private GameObject rotatableCannonBody;
        [SerializeField] private SpriteRenderer bubbelInCannon;

        [SerializeField] private MouseCursorStyleController mouseCursorControllerPrefab;
        private Sprite currentModePanel; //CurrentModePanel
        private Sprite feedTexture; //Feed
        
        //the point on the screen which the cannon spins around
        private Vector3 cannonFulcrum = new(400, 525, 0);

        private CannonData cannonData;
        [SerializeField] private float keyboardAimSensitivity = 1f;

        //game flow control
        private bool onMainMenu = true;
        private bool gameRunning;
        private bool shotFired;
        private bool shotLanded;
        private bool bubbelsPopping;
        private bool bubbelsFalling;
        private bool needToCheckColours;
        private bool shotReloading;

        //score
        private Score score;
        [SerializeField] private TextMeshPro scoreDisplay;

        //Difficulty settings
        private int missedShotsAllowed;

        //Shot data
        private Vector2 shotDirection;
        private Vector2 shotLocation;
        private float shotSpeed = 6.5f;
        private Color shotColor;
        private Point justAddedBubbel;
        private int shotReloadingStage;

        //bubbels popping and dropping
        private FallingBubbels fallingBubbels;
        [SerializeField] private FallingParticleEngine fallingParticleEnginePrefab;
        private FallingParticleEngine fallingParticleEngine;
        private PoppingBubbels poppingBubbels;
        [SerializeField] private PoppingParticleEngine poppingParticleEnginePrefab;
        private PoppingParticleEngine poppingParticleEngine;

        private int popInterval = 4;

        //Playing Field Data
        private List<Color> availableColours;
        private int topAlignedRight;
        private Color[,] playingField;

        [SerializeField] private AudioSource generalAudioSource;
        [SerializeField] private AudioSource bounceAudio;

        [SerializeField] private AudioClip popAudio;
        [SerializeField] private AudioClip slurpAudio;

        private void Awake()
        {
            CreateParticleSystems();
            
            CreateMenus();
            
            //set up mice. something means they have to set up after initialised. TODO
            InitialiseMouses();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            PauseGame();
        }

        private void CreateParticleSystems()
        {
            poppingParticleEngine = Instantiate(poppingParticleEnginePrefab, transform);
            fallingParticleEngine = Instantiate(fallingParticleEnginePrefab, transform);
        }

        internal void ExitGame()
        {
            if(!Application.isEditor)
                Application.Quit();
#if UNITY_EDITOR
            else
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
#endif
        }

        private void CreateMenus()
        {
            menu = Instantiate(menuPrefab);
            menu.CrosslinkWithGame(this);
        }
        
        private void Start()
        {
            StartNewClassicGame();
        }

        private void InitialiseMouses()
        {
            //setup targeting mouse
            var mouseCursorSyle = Instantiate(mouseCursorControllerPrefab, transform);
            mouseFireControlRegion = new Rect(leftSide, 0, rightSide - leftSide, Screen.height);
            mouseCursorSyle.SetCrosshairRegion(mouseFireControlRegion);
        }

        /// <summary>
        /// Generates a random field of balls
        /// TODO - ensure no balls are left floating  https://github.com/Bomadeno/Bubbel/issues/4
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
                int x = UnityEngine.Random.Range(0, availableColours.Count);
                playingField[j, rowNumber] = availableColours[x];
                //makes a certain percentage of the slots empty
                if (UnityEngine.Random.Range(0f,1f) < percentBlank)
                {
                    playingField[j, rowNumber] = TransparentBlack;
                }
            }
        }


        public void PauseGame()
        {
            //todo this is nasty, really both game and menu should subscribe to pause event, instead menu calls game...
            menu.ShowPauseMenu();
            poppingParticleEngine.Pause();
            gameRunning = false;
        }

        public void ResumeGame()
        {
            poppingParticleEngine.Resume();
            gameRunning = true;
        }

        public void GameOver(bool hasWon)
        {
            gameRunning = false;

            if (hasWon)
            {
                menu.Won();
            }
            else
            {
                menu.Lost();
            }
        }

        public void StartNewClassicGame()
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

            menu.GetReadyToStart();

            //Setup initial available colours
            List<Color> sensibleColours = new List<Color>();
            sensibleColours.Add(Color.magenta);
            sensibleColours.Add(Color.yellow);
            sensibleColours.Add(new Color(0.26f, 0.17f, 0.5f));
            sensibleColours.Add(Color.green);
            sensibleColours.Add(Color.blue);
            sensibleColours.Add(new Color(0.18f, 1f, 0.91f));
            sensibleColours.Add(Color.red);
            sensibleColours.Add(new Color(0.04f, 0.28f, 0.05f));

            availableColours = new List<Color>();
            for (int i = 0; i < settings.NumberOfDifferentBallColours && i < sensibleColours.Count; i++)
            {
                int sensibleIndex = UnityEngine.Random.Range(0, sensibleColours.Count);
                while (availableColours.Contains(sensibleColours[sensibleIndex]))
                {
                    sensibleIndex = UnityEngine.Random.Range(0, sensibleColours.Count);
                }
                availableColours.Add(sensibleColours[sensibleIndex]);
            }

            missedShotsAllowed = settings.MissedShotsAllowed;

            //generate the playing field
            GeneratePlayingField(settings.InitialRowCount);

            //set up cannon
            cannonData = new CannonData();
            cannonData.currentBallColor = availableColours[UnityEngine.Random.Range(0, availableColours.Count)];
            cannonData.nextFiveShotLocation = new Vector2[5];
            cannonData.nextFiveShotLocation[0] = new Vector2(618, 568);
            cannonData.nextFiveShotLocation[1] = new Vector2(658, 568);
            cannonData.nextFiveShotLocation[2] = new Vector2(698, 568);
            cannonData.nextFiveShotLocation[3] = new Vector2(738, 568);
            cannonData.nextFiveShotLocation[4] = new Vector2(778, 568);


            cannonData.nextFiveShots = new List<Color>();
            for (int i = 0; i < 5; i++)
            {
                cannonData.nextFiveShots.Add(availableColours[UnityEngine.Random.Range(0, availableColours.Count)]);
            }
        }


        private Vector3 lastMousePosition;
        private Rect mouseFireControlRegion;

        private void TargetWithMouse()
        {
            if (lastMousePosition == Input.mousePosition)
            {
                //other input methods rule!
                return;
            }
            
            //todo this is broken
            lastMousePosition = Input.mousePosition;
            Vector3 fulcrumToMouse = cannonFulcrum - lastMousePosition;
            cannonData.AngleInRadians = -1 * (float)Math.Atan(fulcrumToMouse.x / fulcrumToMouse.y);
            if (cannonData.AngleInRadians < -1.1)
                cannonData.AngleInRadians = -1.1f;
            if (cannonData.AngleInRadians > 1.1)
                cannonData.AngleInRadians = 1.1f;
        }


        /// <summary>
        /// Fires a shot from the current cannon angle
        /// </summary>
        private void FireShot()
        {
            if (shotFired || shotLanded || bubbelsPopping || bubbelsFalling || shotReloading) return;
            
            //calculate the shot direction from the cannon angle
            //-90 gives 0 as the upwards angle
            float shotAngle = cannonData.AngleInRadians + Mathf.Deg2Rad * -90;
            shotDirection = new Vector2((float)Math.Cos(shotAngle) * shotSpeed, (float)Math.Sin(shotAngle) * shotSpeed);
            //start shot from cannon centre
            shotLocation = cannonFulcrum;
            //set colour of current ball to that of the cannon
            shotColor = cannonData.currentBallColor;
            generalAudioSource.Play();

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
            cannonData.nextFiveShots.Add(availableColours[UnityEngine.Random.Range(0, availableColours.Count)]);
        }

        /// <summary>
        /// Changes the shot direction as if it bounced off a vertical wall
        /// Also plays a bounce sound effect.
        /// </summary>
        private void BounceOffWall(AudioSide side)
        {
            shotDirection.x *= -1;
            switch (side)
            {
                case AudioSide.None:
                    bounceAudio.panStereo = 0;
                    break;
                case AudioSide.Left:
                    bounceAudio.panStereo = -1;
                    break;
                case AudioSide.Right:
                    bounceAudio.panStereo = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
            bounceAudio.Play();
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

            if (shotLocation.y < halfBallWidth)
            {
                return true;
            }

            for (int i = 0; i < playingField.GetLength(1); i++)
            {
                int y = i * rowSpacing;
                for (int j = 0; j < playingField.GetLength(0); j++)
                {
                    //For each bubble position in the gameboard that is not null
                    if (playingField[j, i] != TransparentBlack)
                    {
                        focusBall = new Vector2(j * (ballWidth + horizontalPadding) + leftSide + leftPadding + (halfBallWidth * ((i + topAlignedRight) % 2)), y);
                        focusBall += new Vector2(halfBallWidth, halfBallWidth);
                        distance = shotLocation - focusBall;
                        if (distance.magnitude < collisionTolerance)
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
            int i = (int)Math.Floor(shotLocation.y / rowSpacing);
            //get the 'x' column
            int j = (int)Math.Floor((shotLocation.x - halfBallWidth * ((i + topAlignedRight) % 2) - leftPadding - leftSide)) / (ballWidth + horizontalPadding);

            if (i >= playingField.GetLength(1))
                i = playingField.GetLength(1) - 1;
            if (j >= playingField.GetLength(0))
                j = playingField.GetLength(0) - 1;

            if (playingField[j, i] == TransparentBlack)
            {
                playingField[j, i] = shotColor;
                justAddedBubbel = new Point(j, i);
                generalAudioSource.PlayOneShot(slurpAudio);
                //todo play "slurp into field" effect
            }
            else
            {
                //move bubbel backwards and try again
                shotLocation -= shotDirection;

                if (shotLocation.x < leftSide + halfBallWidth)
                {
                    shotDirection.x *= -1;
                }
                else if (shotLocation.x > rightSide - halfBallWidth)
                {
                    shotDirection.x *= -1;
                }

                AddShotBubbelToField();
            }
            
        }

        protected void Update()
        {
            if (onMainMenu)
            {
                onMainMenu = false;

            }
            else
            {
                if (gameRunning)
                {
                    ProcessKeyboard(Time.deltaTime);
                    //TargetWithMouse(); -commented out awaiting proper mouse support
                    UpdateShot();
                    PostShotLandingProcessing();
                    AnimateBubbelsPopping();
                    DropBubbels();
                    SpinLoadedShot();
                    TestGameOver();
                    CheckFaultCount();
                    UpdateVisualRepresentation();
                }
                if (gameRunning)
                {
                    RemoveColoursNotOnField();
                }
            }
        }

        /// <summary>
        /// Processes the keyboard for user input
        /// Allows the user to control the cannon using left and right keys and space to fire
        /// </summary>
        private void ProcessKeyboard(float deltaTime)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                cannonData.AngleInRadians -= keyboardAimSensitivity * deltaTime;
                if (cannonData.AngleInRadians < -1.1)
                    cannonData.AngleInRadians = -1.1f;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                cannonData.AngleInRadians += keyboardAimSensitivity * deltaTime;
                if (cannonData.AngleInRadians > 1.1)
                    cannonData.AngleInRadians = 1.1f;
            }
            if ((Input.GetKeyDown(KeyCode.Space) || IsClickValidToFireShot())  && !shotFired && !shotLanded && !bubbelsPopping && !bubbelsFalling)
            {
                FireShot();
            }

            bool IsClickValidToFireShot()
            {
                return Input.GetMouseButtonDown(0) & mouseFireControlRegion.Contains(Input.mousePosition);
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
                if (shotLocation.x < leftSide + halfBallWidth)
                {
                    BounceOffWall(AudioSide.Left);
                }
                //if shot is to the right of the play area, invert x
                else if (shotLocation.x > rightSide - halfBallWidth)
                {
                    BounceOffWall(AudioSide.Right);
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
                    playingField[p.X, p.Y] = TransparentBlack;
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
                if (playingField[j, 0] != TransparentBlack && !poppingBubbels.points.Contains(new Point(j,0)))
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
                    if (playingField[j, i] != TransparentBlack && !secureBubbels.Contains(new Point(j, i)) && !poppingBubbels.points.Contains(new Point(j, i)))
                    {
                        score.numberDropped++;
                        fallingBubbels.points.Add(new Point(j, i));
                        fallingBubbels.positions.Add(WorldVectorFromGridPoint(j, i));
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
                if (playingField[nonTransparentNeighbours[i].X, nonTransparentNeighbours[i].Y] == TransparentBlack
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
                    generalAudioSource.PlayOneShot(popAudio);

                    //add to score
                    int scoreForThisBubbel = score.Pop();

                    int next = UnityEngine.Random.Range(0, poppingBubbels.points.Count);
                    poppingBubbels.currentlyPoppingProgress = 0;
                    poppingBubbels.currentlyPopping = poppingBubbels.points[next];
                    poppingBubbels.currentlyPoppingColor = poppingBubbels.colours[next];
                    poppingParticleEngine.AddBubbel(WorldVectorFromGridPoint(poppingBubbels.currentlyPopping), poppingBubbels.currentlyPoppingColor, scoreForThisBubbel);
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
                    foreach (var p in fallingBubbels.points)
                    {
                        playingField[p.X, p.Y] = TransparentBlack;
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
                    fallingParticleEngine.AddBubbel(fallingBubbels.positions[i], fallingBubbels.color[i]);
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
                    cannonData.nextFiveShots.Add(availableColours[UnityEngine.Random.Range(0, availableColours.Count)]);
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
            //always wait for popping to finish
            if (bubbelsPopping) 
                return;
            
            bool gameLost = false;
            bool gameWon = false;

            //if there's anything in the bottom row and it's not popping or falling
            //GAME OVER!
            for (int j = 0; j < playingField.GetLength(0); j++)
            {
                if (playingField[j, playingField.GetLength(1) - 1] != TransparentBlack)
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
                if (playingField[j, 0] != TransparentBlack)
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
        
        
        private Vector2 WorldVectorFromGridPoint(Point point)
        {
            return WorldVectorFromGridPoint(point.X, point.Y);
        }

        private Vector2 WorldVectorFromGridPoint(int j, int i)
        {
            return new Vector2(j * (ballWidth + horizontalPadding) + leftSide + leftPadding + halfBallWidth * ((i + topAlignedRight) % 2), i * rowSpacing) * scaleFactor;
        }
    

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected void UpdateVisualRepresentation()
        {
            if (onMainMenu)
            {

            }
            else
            {
                UpdateBallField();
                DrawPoppingBubbels();
                
                UpdateCannon();
                DrawQueue();
                DrawShot();
                DrawScore();
            }

        }
 
   

        private void UpdateBallField()
        {
            for(int i = 0; i< bubbelGridVisuals.Count; i++)
            {
                Destroy(bubbelGridVisuals[i].gameObject);
            }

            bubbelGridVisuals.Clear();
            
            for (int i = 0; i < playingField.GetLength(1); i++)
            {
                for (int j = 0; j < playingField.GetLength(0); j++)
                {
                    //If there is a ball at a location
                    if (playingField[j, i] != TransparentBlack)
                    {
                        DrawSingleBubbelAtPoint(j, i, playingField[j, i]);
                    }
                }
            }
        }

        private void DrawSingleBubbelInWorld(Vector2 position, Color bubbelColor)
        {
            //Draw it
            //todo these are recreated way too often. they shouldn't be
            var bubbel = Instantiate(bubbelPrefab, position, Quaternion.identity);
            bubbel.color = bubbelColor;
            //draw on top of background. not the best approach but it'll do
            bubbel.sortingOrder = 2;
            bubbelGridVisuals.Add(bubbel);
        }

        private void DrawSingleBubbelAtPoint(int j, int i, Color bubbelColor)
        {
            DrawSingleBubbelInWorld(WorldVectorFromGridPoint(j, i), bubbelColor);
        }

        private void DrawPoppingBubbels()
        {
            if (bubbelsPopping)
            {
                //draw 'existing' bubbels that are yet to be popped but will soon be popped
                for (int i = 0; i < poppingBubbels.points.Count; i++)
                {
                    var poppingBubbelPoint = poppingBubbels.points[i];
                    DrawSingleBubbelAtPoint(poppingBubbelPoint.X, poppingBubbelPoint.Y, poppingBubbels.colours[i]);
                }
            }
        }


        private void UpdateCannon()
        {
            //Port note: the gameobject should consist of frame -> background -> ball -> foreground. The cannon fore should be wheat color,  new Color(0.96f, 0.87f, 0.7f), #F5DEB3
            
            //todo ensure cannon and background are drawn in precisely the correct place  https://github.com/Bomadeno/Bubbel/issues/5
            //cannonInstance.position = (cannonFulcrum-new Vector3(30,0, 0)) * scaleFactor;
            
            //Fade bubbel into cannon. the bubbel spins in the cannon.
            if (shotReloading)
            {
                Color currentBallColorSemiTransparent = cannonData.currentBallColor;
                currentBallColorSemiTransparent.a = (byte)(shotReloadingStage/100.0f * 255.0f);
                bubbelInCannon.color = currentBallColorSemiTransparent;
                bubbelInCannon.transform.localRotation = Quaternion.Euler(0, 0, cannonData.currentBallRotation);
            }
            else
            {
                bubbelInCannon.color = cannonData.currentBallColor;
                bubbelInCannon.transform.localRotation = Quaternion.Euler(0, 0, cannonData.currentBallRotation);
            }
            rotatableCannonBody.transform.rotation = Quaternion.Euler(0, 0, -cannonData.AngleInRadians * Mathf.Rad2Deg);
        }

        
        private void DrawQueue()
        {
            //todo all bubbels in queue should be drawn from 18,18 (halfBallWidth)

            if (shotReloading)
            {
                float percentageDone = shotReloadingStage / 100.0f;
                //Fade out the bubbel quickly
                if (shotReloadingStage < 25)
                {
                    Color currentBallColorSemiTransparent = cannonData.currentBallColor;
                    currentBallColorSemiTransparent.a = (byte)(255 - percentageDone * 1023);
                    
                    DrawSingleBubbelInWorld(cannonData.nextFiveShotLocation[0]* scaleFactor, currentBallColorSemiTransparent);
                    
                    DrawSingleBubbelInWorld(cannonData.nextFiveShotLocation[1] * scaleFactor, cannonData.nextFiveShots[0]);
                    DrawSingleBubbelInWorld(cannonData.nextFiveShotLocation[2] * scaleFactor, cannonData.nextFiveShots[1]);
                    DrawSingleBubbelInWorld(cannonData.nextFiveShotLocation[3] * scaleFactor, cannonData.nextFiveShots[2]);
                    DrawSingleBubbelInWorld(cannonData.nextFiveShotLocation[4] * scaleFactor, cannonData.nextFiveShots[3]);
                }
                //Slide next bubbels along
                else
                {
                    float percentageSlideDone = (shotReloadingStage-25.0f) / 75.0f;
                    //move balls 40 right
                    Vector2 interpVec = new Vector2(40 * (1.0f-percentageSlideDone), 0);
                    
                    DrawSingleBubbelInWorld((cannonData.nextFiveShotLocation[0] + interpVec) * scaleFactor, cannonData.nextFiveShots[0]);
                    DrawSingleBubbelInWorld((cannonData.nextFiveShotLocation[1] + interpVec) * scaleFactor, cannonData.nextFiveShots[1]);
                    DrawSingleBubbelInWorld((cannonData.nextFiveShotLocation[2] + interpVec) * scaleFactor, cannonData.nextFiveShots[2]);
                    DrawSingleBubbelInWorld((cannonData.nextFiveShotLocation[3] + interpVec) * scaleFactor, cannonData.nextFiveShots[3]);
                    DrawSingleBubbelInWorld((cannonData.nextFiveShotLocation[4] + interpVec) * scaleFactor, cannonData.nextFiveShots[4]);
                }

                //todo probably delete: spriteBatch.Draw(feedTexture, new Vector2(586, 537), null, Color.White, 0.0f, zeroOrigin, 1.0f, SpriteEffects.None, 0.5f);

                shotReloadingStage += 5;
                if (shotReloadingStage > 100)
                {
                    shotReloading = false;
                    shotReloadingStage = 0;
                }
            }
            else
            {
                //todo probably delete: spriteBatch.Draw(feedTexture, new Vector2(586, 537), null, Color.White, 0.0f, zeroOrigin, 1.0f, SpriteEffects.None, 0.5f);
                
                //todo could just use interpVec above set to 0,0?
                DrawSingleBubbelInWorld((cannonData.nextFiveShotLocation[0]) * scaleFactor, cannonData.nextFiveShots[0]);
                DrawSingleBubbelInWorld((cannonData.nextFiveShotLocation[1]) * scaleFactor, cannonData.nextFiveShots[1]);
                DrawSingleBubbelInWorld((cannonData.nextFiveShotLocation[2]) * scaleFactor, cannonData.nextFiveShots[2]);
                DrawSingleBubbelInWorld((cannonData.nextFiveShotLocation[3]) * scaleFactor, cannonData.nextFiveShots[3]);
                DrawSingleBubbelInWorld((cannonData.nextFiveShotLocation[4]) * scaleFactor, cannonData.nextFiveShots[4]);
            }
        }
        

        private void DrawShot()
        {
            if (shotFired)
            {
                //offset so middle of bubbel = pivot
                DrawSingleBubbelInWorld((shotLocation - new Vector2(halfBallWidth,halfBallWidth)) * scaleFactor, shotColor);
            }
        }

        private void DrawScore()
        {
            scoreDisplay.text = "Score: " + score.currentTotalScore;
        }
    }
}