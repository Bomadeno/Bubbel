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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using MattsMenuLibrary;

namespace Bubbel_Shot
{
    public class BubbelParticle
    {
        public int lifeSpan;
        public int currentLife;
        public Vector2 location;
        public Color color;
        public float orientation;
        public int particleScore;
        Random r;

        public BubbelParticle(Vector2 location, Color color, int particleScore)
        {
            r = new Random();
            this.location = location;
            this.color = color;
            lifeSpan = 10;
            currentLife = 0;
            orientation = (float)r.NextDouble()*3;
            this.particleScore = particleScore;
        }
    }

    public class PoppingParticleEngine : DrawableGameComponent
    {
        public List<BubbelParticle> bubbelParticles;
        private bool isRunning;
        private SpriteBatch spriteBatch;
        private ContentManager cm;

        //Bubbel popping textures
        Texture2D frameOne;
        Texture2D frameTwo;
        Texture2D frameThree;
        Texture2D frameFour;
        Texture2D frameFive;
        Texture2D frameSix;
        Texture2D frameSeven;
        Texture2D frameEight;
        Texture2D frameNine;

        private Vector2 animationTextureOrigin = new Vector2(20,20);
        private Vector2 offset = new Vector2(18, 18);

        /// <summary>
        /// Constructor, creates the particle engine
        /// </summary>
        /// <param name="game">The instance of Game this particle
        /// engine will be used in</param>
        public PoppingParticleEngine(Game game, ContentManager cm)
            : base(game)
        {
            bubbelParticles = new List<BubbelParticle>();
            isRunning = true;
            this.cm = cm;
        }

        #region Gameflow Control

        /// <summary>
        /// Call to pause the updating and drawing of
        /// the particles
        /// </summary>
        public void Pause()
        {
            isRunning = false;
        }

        /// <summary>
        /// Call to resume the updating and drawing of
        /// the particles
        /// </summary>
        public void Resume()
        {
            isRunning = true;
        }

        #endregion

        #region Load 

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            frameOne = cm.Load<Texture2D>("frameOne");
            frameTwo = cm.Load<Texture2D>("frameTwo");
            frameThree = cm.Load<Texture2D>("frameThree");
            frameFour = cm.Load<Texture2D>("frameFour");
            frameFive = cm.Load<Texture2D>("frameFive");
            frameSix = cm.Load<Texture2D>("frameSix");
            frameSeven = cm.Load<Texture2D>("frameSeven");
            frameEight = cm.Load<Texture2D>("frameEight");
            frameNine = cm.Load<Texture2D>("frameNine");

            base.LoadContent();
        }

        #endregion
        
        public void AddBubbel(BubbelParticle bp)
        {
            bubbelParticles.Add(bp);
        }

        public override void Update(GameTime gameTime)
        {
            if (isRunning)
            {
                for (int i = bubbelParticles.Count - 1; i >= 0; i--)
                {
                    //age all particles
                    bubbelParticles[i].currentLife++;
                    //remove any dead particles
                    if (bubbelParticles[i].lifeSpan < bubbelParticles[i].currentLife)
                    {
                        bubbelParticles.RemoveAt(i);
                    }
                }
                //TODO maybe vary location slightly?
            } 
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {  
            spriteBatch.Begin();

                foreach (BubbelParticle bp in bubbelParticles)
                {
                    //draw all particles at the correct stage
                    if (bp.currentLife == 1)
                    {
                        spriteBatch.Draw(frameOne, bp.location+offset, null, bp.color, bp.orientation, animationTextureOrigin, 1, SpriteEffects.None, 1);
                    }
                    else if (bp.currentLife ==2)
                    {
                        spriteBatch.Draw(frameTwo, bp.location + offset, null, bp.color, bp.orientation, animationTextureOrigin, 1, SpriteEffects.None, 1);
                    }
                    else if (bp.currentLife == 3)
                    {
                        spriteBatch.Draw(frameThree, bp.location + offset, null, bp.color, bp.orientation, animationTextureOrigin, 1, SpriteEffects.None, 1);
                    }
                    else if (bp.currentLife == 4)
                    {
                        spriteBatch.Draw(frameFour, bp.location + offset, null, bp.color, bp.orientation, animationTextureOrigin, 1, SpriteEffects.None, 1);
                    }
                    else if (bp.currentLife == 5)
                    {
                        spriteBatch.Draw(frameFive, bp.location + offset, null, bp.color, bp.orientation, animationTextureOrigin, 1, SpriteEffects.None, 1);
                    }
                    else if (bp.currentLife == 6)
                    {
                        spriteBatch.Draw(frameSix, bp.location + offset, null, bp.color, bp.orientation, animationTextureOrigin, 1, SpriteEffects.None, 1);
                    }
                    else if (bp.currentLife == 7)
                    {
                        spriteBatch.Draw(frameSeven, bp.location + offset, null, bp.color, bp.orientation, animationTextureOrigin, 1, SpriteEffects.None, 1);
                    }
                    else if (bp.currentLife == 8)
                    {
                        spriteBatch.Draw(frameEight, bp.location + offset, null, bp.color, bp.orientation, animationTextureOrigin, 1, SpriteEffects.None, 1);
                    }
                    else if (bp.currentLife == 9)
                    {
                        spriteBatch.Draw(frameNine, bp.location + offset, null, bp.color, bp.orientation, animationTextureOrigin, 1, SpriteEffects.None, 1);
                    }
                    //spriteBatch.Draw();

                    
                }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
