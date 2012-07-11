using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectVanquish.Cameras;
using ProjectVanquish.Core;

namespace ProjectVanquish.Renderers
{
    /// <summary>
    /// Deferred Renderer
    /// </summary>
    public class DeferredRenderer : DrawableGameComponent
    {
        #region Fields
        private FirstPersonCamera camera;
        private QuadRenderer quadRenderer;
        SceneManager scene;
        private LightManager lightManager;
        private RenderTarget2D colorRT, normalRT, depthRT, depthTexture, lightRT, sceneRT;
        private SpriteBatch spriteBatch;
        private Vector2 halfPixel;
        private Effect clearBufferEffect, finalCombineEffect, depthShadowEffect;
        private Model sphereModel;
        private ShadowRenderer shadowRenderer;
        private SSAORenderer ssaoRenderer;
        private SkyRenderer skyRenderer;
        private KeyboardState lastKeyboardState;
        private int lastMouseX;
        private int lastMouseY;
        private GamePadState lastGamepadState;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredRenderer"/> class.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public DeferredRenderer(Game game)
            : base(game)
        {
            scene = new SceneManager(game);
        } 
        #endregion

        #region Members
        /// <summary>
        /// Clears the G buffer.
        /// </summary>
        private void ClearGBuffer()
        {
            clearBufferEffect.Techniques[0].Passes[0].Apply();
            quadRenderer.Render(Vector2.One * -1, Vector2.One);
        }

        /// <summary>
        /// Called when the DrawableGameComponent needs to be drawn. Override this method with component-specific drawing code. Reference page contains links to related conceptual articles.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to Draw.</param>
        public override void Draw(GameTime gameTime)
        {
            // Set the GBuffer
            SetGBuffer();
            
            // Clear the GBuffer
            ClearGBuffer();

            // Render sky if enabled
            if (SkyRenderer.Enabled)
            {
                // To create a Static SunLight, uncomment this line
                //SkyRenderer.Parameters.LightDirection = new Vector4(LightManager.Light.Direction, 1);
                skyRenderer.Draw(gameTime, camera);
            }

            // Render the scene
            scene.DrawScene(gameTime);

            // Resolve the GBuffer
            ResolveGBuffer();
            
            // Draw Depth for Shadow Mapping
            DrawDepth(gameTime);

            // Render Shadows
            var shadowOcclusion = shadowRenderer.Draw(scene, depthTexture, LightManager.Light, camera);

            // Draw Lights
            lightManager.DrawLights(GraphicsDevice, colorRT, normalRT, depthRT, lightRT, camera, scene);

            // Combine the Final scene
            CombineFinal(shadowOcclusion);

            // Render SSAO if enabled
            if (SSAORenderer.Enabled)
                ssaoRenderer.Draw(GraphicsDevice, normalRT, depthRT, sceneRT, scene, camera, null);
                        
            // Render output
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
            spriteBatch.Draw((Texture2D)colorRT, new Rectangle(0, 0, 128, 128), Color.White);
            spriteBatch.Draw((Texture2D)depthRT, new Rectangle(128, 0, 128, 128), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws depth for the scene to a texture
        /// </summary>
        void DrawDepth(GameTime gameTime)
        {
            // Set to render to our depth texture
            GraphicsDevice.SetRenderTarget(depthTexture);

            // Clear the texture
            GraphicsDevice.Clear(ClearOptions.Target, new Vector4(1.0f), 1.0f, 0);
            GraphicsDevice.Clear(ClearOptions.DepthBuffer, new Vector4(1.0f), 1.0f, 0);

            // Setup our Depth Effect
            depthShadowEffect.CurrentTechnique = depthShadowEffect.Techniques["LinearDepth"];
            depthShadowEffect.Parameters["g_matView"].SetValue(camera.ViewMatrix);
            depthShadowEffect.Parameters["g_matProj"].SetValue(camera.ProjectionMatrix);
            depthShadowEffect.Parameters["g_fFarClip"].SetValue(camera.FarClip);

            // Apply the Effect
            depthShadowEffect.CurrentTechnique.Passes[0].Apply();

            // Draw the models
            scene.DrawSceneWithCustomEffect(GraphicsDevice, depthShadowEffect);
        }

        /// <summary>
        /// Combines the final scene.
        /// </summary>
        /// <param name="shadowOcclusion">The shadow occlusion.</param>
        void CombineFinal(RenderTarget2D shadowOcclusion)
        {
            // If SSAO is enabled, set the RenderTarget
            if (SSAORenderer.Enabled)
                GraphicsDevice.SetRenderTarget(sceneRT);

            // Set the effect parameters
            finalCombineEffect.Parameters["colorMap"].SetValue(colorRT);
            finalCombineEffect.Parameters["lightMap"].SetValue(lightRT);
            finalCombineEffect.Parameters["halfPixel"].SetValue(halfPixel);
            finalCombineEffect.Parameters["shadowOcclusion"].SetValue(shadowOcclusion);

            // Apply the Effect
            finalCombineEffect.Techniques[0].Passes[0].Apply();

            // Render a full-screen quad
            quadRenderer.Render(Vector2.One * -1, Vector2.One);
        }

        /// <summary>
        /// Initializes the component. Override this method to load any non-graphics resources and query for any required services.
        /// </summary>
        public override void Initialize()
        {
            // Instantiate the QuadRenderer
            quadRenderer = new QuadRenderer(Game);
            Game.Components.Add(quadRenderer);
            
            base.Initialize();
        }

        /// <summary>
        /// Called when graphics resources need to be loaded. Override this method to load any component-specific graphics resources.
        /// </summary>
        protected override void LoadContent()
        {
            camera = camera = new FirstPersonCamera(MathHelper.PiOver4, GraphicsDevice.PresentationParameters.BackBufferWidth / GraphicsDevice.PresentationParameters.BackBufferHeight, 1.0f, 500.0f);
            camera.Position = new Vector3(0, 5, 10);
            halfPixel.X = 0.5f / (float)GraphicsDevice.PresentationParameters.BackBufferWidth;
            halfPixel.Y = 0.5f / (float)GraphicsDevice.PresentationParameters.BackBufferHeight;

            // Get the sizes of the backbuffer, in order to have matching render targets   
            int backBufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int backBufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            // Configure RenderTargets
            //colorRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            //normalRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            //depthRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            //depthTexture = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);
            //lightRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            //sceneRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);

            colorRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Rgba64, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Rgba64, DepthFormat.None);
            depthRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Rgba64, DepthFormat.None);
            depthTexture = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);
            lightRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.None);
            sceneRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Rgba64, DepthFormat.Depth24);

            // Initialize SceneManager
            scene.InitializeScene(camera);

            // Instantiate the LightManager
            lightManager = new LightManager(Game);

            // Load Effects
            clearBufferEffect = Game.Content.Load<Effect>("Shaders/GBuffer/ClearGBuffer");
            finalCombineEffect = Game.Content.Load<Effect>("Shaders/GBuffer/CombineFinal");
            depthShadowEffect = Game.Content.Load<Effect>("Shaders/Shadows/Depth");
            sphereModel = Game.Content.Load<Model>("Models/sphere");

            // Instantiate SpriteBatch
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Make our ShadowRenderer
            shadowRenderer = new ShadowRenderer(GraphicsDevice, Game.Content);

            // Instantiate the SSAO Renderer
            ssaoRenderer = new SSAORenderer(Game, backBufferWidth, backBufferHeight);

            // Instantiate the Sky Renderer
            skyRenderer = new SkyRenderer(Game, Game.Content, camera);

            base.LoadContent();
        }

        /// <summary>
        /// Resolves the G buffer.
        /// </summary>
        private void ResolveGBuffer()
        {
            GraphicsDevice.SetRenderTargets(null);
        }

        /// <summary>
        /// Sets the G buffer.
        /// </summary>
        private void SetGBuffer()
        {
            GraphicsDevice.SetRenderTargets(colorRT, normalRT, depthRT);
        }

        /// <summary>
        /// Called when the GameComponent needs to be updated. Override this method with component-specific update code.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to Update</param>
        public override void Update(GameTime gameTime)
        {
            float dt = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            // Toggle shadow rendering on and off
            if (keyboardState.IsKeyDown(Keys.T) && lastKeyboardState.IsKeyUp(Keys.T))
                ShadowRenderer.Enabled = !ShadowRenderer.Enabled;

            // Switch through shadowmap filtering techniques
            if (keyboardState.IsKeyDown(Keys.G) && lastKeyboardState.IsKeyUp(Keys.G))
                shadowRenderer.ShadowFilteringType = (ShadowFilteringType)((((int)shadowRenderer.ShadowFilteringType) + 1) % 4);

            // Move the camera with keyboard and mouse input 
            float cameraMoveAmount = 50.0f * dt;
            float cameraRotateAmount = 0.25f * dt;
            float modelRotateAmount = 0.5f * dt;
            
            if (keyboardState.IsKeyDown(Keys.W))
                camera.Position += camera.WorldMatrix.Forward * cameraMoveAmount;
            if (keyboardState.IsKeyDown(Keys.S))
                camera.Position += camera.WorldMatrix.Backward * cameraMoveAmount;
            if (keyboardState.IsKeyDown(Keys.A))
                camera.Position += camera.WorldMatrix.Left * cameraMoveAmount;
            if (keyboardState.IsKeyDown(Keys.D))
                camera.Position += camera.WorldMatrix.Right * cameraMoveAmount;
            if (keyboardState.IsKeyDown(Keys.Q))
                camera.Position += camera.WorldMatrix.Up * cameraMoveAmount;
            if (keyboardState.IsKeyDown(Keys.E))
                camera.Position += camera.WorldMatrix.Down * cameraMoveAmount;
            if (keyboardState.IsKeyDown(Keys.D1))
                shadowRenderer.ShadowFilteringType = ShadowFilteringType.PCF2x2;
            if (keyboardState.IsKeyDown(Keys.D2))
                shadowRenderer.ShadowFilteringType = ShadowFilteringType.PCF3x3;
            if (keyboardState.IsKeyDown(Keys.D3))
                shadowRenderer.ShadowFilteringType = ShadowFilteringType.PCF5x5;
            if (keyboardState.IsKeyDown(Keys.D4))
                shadowRenderer.ShadowFilteringType = ShadowFilteringType.PCF7x7;

            if (keyboardState.IsKeyDown(Keys.Space))
                LightManager.AddPointLight(new Lights.PointLight(camera.Position, Color.Yellow.ToVector3(), 50f, 1));

            if (keyboardState.IsKeyDown(Keys.PageDown))
                skyRenderer.Theta -= 0.4f * dt;

            if (lastMouseX == -1)
                lastMouseX = mouseState.X;
            if (lastMouseY == -1)
                lastMouseY = mouseState.Y;

            int mouseMoveX = mouseState.X - lastMouseX;
            int mouseMoveY = mouseState.Y - lastMouseY;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                camera.YRotation -= cameraRotateAmount * mouseMoveX;
                camera.XRotation -= cameraRotateAmount * mouseMoveY;
            }

            SkyRenderer.Parameters.LightDirection = new Vector4(LightManager.Light.Direction, 1);
            SkyRenderer.Parameters.LightColor = new Vector4(LightManager.Light.Color, 1);
            skyRenderer.Update(gameTime);

            Game.Window.Title = String.Format("X:{0} = Y:{1} = Z:{2}", camera.Position.X, camera.Position.Y, camera.Position.Z);

            lastMouseX = mouseState.X;
            lastMouseY = mouseState.Y;
            lastKeyboardState = keyboardState;

            base.Update(gameTime);
        } 
        #endregion
    }
}
