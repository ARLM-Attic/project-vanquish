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
using ProjectVanquish.Core;
using ProjectVanquish.Cameras;
using ProjectVanquish.Renderers;

namespace ProjectVanquishTest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        DeferredRenderer renderer;
        private KeyboardState lastKeyboardState;
        private int lastMouseX;
        private int lastMouseY;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here            
            renderer = new DeferredRenderer(GraphicsDevice, Content);
            renderer.UseSSAO = true;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            SceneManager.AddModel(Content.Load<Model>("Models/Sponza"));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>b
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // TODO: Add your update logic here
            float dt = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            // Move the camera with keyboard and mouse input 
            float cameraMoveAmount = 50.0f * dt;
            float cameraRotateAmount = 0.25f * dt;
            float modelRotateAmount = 0.5f * dt;

            if (keyboardState.IsKeyDown(Keys.W))
                renderer.Camera.Position += renderer.Camera.WorldMatrix.Forward * cameraMoveAmount;
            if (keyboardState.IsKeyDown(Keys.S))
                renderer.Camera.Position += renderer.Camera.WorldMatrix.Backward * cameraMoveAmount;
            if (keyboardState.IsKeyDown(Keys.A))
                renderer.Camera.Position += renderer.Camera.WorldMatrix.Left * cameraMoveAmount;
            if (keyboardState.IsKeyDown(Keys.D))
                renderer.Camera.Position += renderer.Camera.WorldMatrix.Right * cameraMoveAmount;
            if (keyboardState.IsKeyDown(Keys.Q))
                renderer.Camera.Position += renderer.Camera.WorldMatrix.Up * cameraMoveAmount;
            if (keyboardState.IsKeyDown(Keys.E))
                renderer.Camera.Position += renderer.Camera.WorldMatrix.Down * cameraMoveAmount;

            if (lastMouseX == -1)
                lastMouseX = mouseState.X;
            if (lastMouseY == -1)
                lastMouseY = mouseState.Y;

            int mouseMoveX = mouseState.X - lastMouseX;
            int mouseMoveY = mouseState.Y - lastMouseY;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                renderer.Camera.YRotation -= cameraRotateAmount * mouseMoveX;
                renderer.Camera.XRotation -= cameraRotateAmount * mouseMoveY;
            }

            lastMouseX = mouseState.X;
            lastMouseY = mouseState.Y;
            renderer.Update(gameTime);

            Window.Title = "X: " + renderer.Camera.Position.X + " Y: " + renderer.Camera.Position.Y + " Z: " + renderer.Camera.Position.Z;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            renderer.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
