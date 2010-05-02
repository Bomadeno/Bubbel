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

namespace Bubbel_Shot
{
    public class FallingParticle
    {
        public Vector2 speed;
        public Vector2 acceleration = new Vector2(0, 0.5f);
        public Vector2 location;
        public Color color;
        public float orientation;
        public int particleScore;
        private const float terminalVelocity = 15;
        private Random random;

        public FallingParticle(Vector2 location, Color color)
        {
            this.location = location;
            this.speed = new Vector2(0, 0);
            random = new Random();

            //'black out' colour
            Vector3 col = color.ToVector3();

            col.X = col.X * 0.3f;
            col.Y = col.Y * 0.3f;
            col.Z = col.Z * 0.3f;

            this.color = new Color(col);
            
        }

        public void Update()
        {
            //update location (move)
            location = location + speed;

            //update speed (accelerate)
            if (speed.Length() < terminalVelocity)
            {
                speed = speed + acceleration;
            }
        }
    }





    class FallingParticleEngine : DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        List<FallingParticle> fallingParticles;
        bool isRunning;
        Texture2D ballTexture;

        public FallingParticleEngine(Game game)
            : base(game)
        {
            fallingParticles = new List<FallingParticle>();
            isRunning = true;
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
            base.LoadContent();
            ballTexture = Game.Content.Load<Texture2D>("Bubbel");
        }

        #endregion

        public void AddBubbel(FallingParticle fp)
        {
            fallingParticles.Add(fp);
        }

        public override void Update(GameTime gameTime)
        {
            if (isRunning)
            {
                for (int i = fallingParticles.Count - 1; i >= 0; i--)
                {
                    //age all particles
                    fallingParticles[i].Update();
                    //remove any dead particles
                    if (fallingParticles[i].location.Y > 700)
                    {
                        fallingParticles.RemoveAt(i);
                    }
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            foreach (FallingParticle fp in fallingParticles)
            {
                //draw all particles at the correct stage
                spriteBatch.Draw(ballTexture, fp.location, fp.color);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
