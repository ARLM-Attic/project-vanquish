using System;
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
        private RenderTarget2D colorRT, normalRT, depthRT, depthTexture, lightRT, sceneRT;
        private SpriteBatch spriteBatch;
        private Vector2 halfPixel;
        private Effect clearBufferEffect, directionalLightEffect, finalCombineEffect, pointLightEffect, depthShadowEffect;
        private Model sphereModel;
        private Lights.DirectionalLight light;
        private CascadeShadowRenderer shadowRenderer;
        private SSAORenderer ssaoRenderer;
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

            // Render the scene
            scene.DrawScene(gameTime);

            // Resolve the GBuffer
            ResolveGBuffer();

            // Draw Depth for Shadow Mapping
            DrawDepth(gameTime);

            // Render Shadows
            var shadowOcclusion = shadowRenderer.Draw(GraphicsDevice, depthTexture, scene, camera);

            // Draw Lights
            DrawLights(gameTime, ref shadowOcclusion);
            
            // Render SSAO if enabled
            if (SSAORenderer.Enabled)
                ssaoRenderer.Draw(GraphicsDevice, normalRT, depthRT, sceneRT, scene, camera, null);

            // Render output
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
            spriteBatch.Draw((Texture2D)ssaoRenderer.ssaoRT, new Rectangle(0, 0, 128, 128), Color.White);
            spriteBatch.Draw((Texture2D)ssaoRenderer.blurRT, new Rectangle(128, 0, 128, 128), Color.White);
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
        /// Draws a directional light.
        /// </summary>
        /// <param name="lightDirection">The light direction.</param>
        /// <param name="color">The color.</param>
        private void DrawDirectionalLight(Vector3 lightDirection, Color color)
        {
            // Set all parameters
            directionalLightEffect.Parameters["colorMap"].SetValue(colorRT);
            directionalLightEffect.Parameters["normalMap"].SetValue(normalRT);
            directionalLightEffect.Parameters["depthMap"].SetValue(depthRT);
            directionalLightEffect.Parameters["lightDirection"].SetValue(lightDirection);
            directionalLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            directionalLightEffect.Parameters["cameraPosition"].SetValue(camera.Position);
            directionalLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(camera.ViewMatrix * camera.ProjectionMatrix));
            directionalLightEffect.Parameters["halfPixel"].SetValue(halfPixel);

            // Apply the Effect
            directionalLightEffect.Techniques[0].Passes[0].Apply();

            // Draw a FullscreenQuad
            quadRenderer.Render(Vector2.One * -1, Vector2.One);
        }

        /// <summary>
        /// Draws the lights.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <param name="shadowOcclusion">The shadow occlusion.</param>
        private void DrawLights(GameTime gameTime, ref RenderTarget2D shadowOcclusion)
        {
            // Set the Light RenderTarget
            GraphicsDevice.SetRenderTarget(lightRT);

            // Clear all components to 0
            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            Color[] colors = new Color[10];
            colors[0] = Color.Red; colors[1] = Color.Blue;
            colors[2] = Color.IndianRed; colors[3] = Color.CornflowerBlue;
            colors[4] = Color.Gold; colors[5] = Color.Green;
            colors[6] = Color.Crimson; colors[7] = Color.SkyBlue;
            colors[8] = Color.Red; colors[9] = Color.ForestGreen;

            float angle = (float)gameTime.TotalGameTime.TotalSeconds;
            
            int n = 10;
            for (int i = 0; i < n; i++)
            {
                Vector3 pos = new Vector3((float)Math.Sin(i * MathHelper.TwoPi / n + angle), 0.30f, (float)Math.Cos(i * MathHelper.TwoPi / n + angle));
                DrawPointLight(pos * 40, colors[i % 10], 15, 2);

                pos = new Vector3((float)Math.Cos((i + 5) * MathHelper.TwoPi / n - angle), 0.30f, (float)Math.Sin((i + 5) * MathHelper.TwoPi / n - angle));
                DrawPointLight(pos * 20, colors[i % 10], 20, 1);

                pos = new Vector3((float)Math.Cos(i * MathHelper.TwoPi / n + angle), 0.10f, (float)Math.Sin(i * MathHelper.TwoPi / n + angle));
                DrawPointLight(pos * 75, colors[i % 10], 45, 2);

                pos = new Vector3((float)Math.Cos(i * MathHelper.TwoPi / n + angle), -0.3f, (float)Math.Sin(i * MathHelper.TwoPi / n + angle));
                DrawPointLight(pos * 20, colors[i % 10], 20, 2);
            }

            DrawPointLight(new Vector3(0, (float)Math.Sin(angle * 0.8) * 40, 0), Color.Red, 30, 5);
            DrawPointLight(new Vector3(0, 25, 0), Color.White, 30, 1);
            DrawPointLight(new Vector3(0, 0, 70), Color.Wheat, 55 + 10 * (float)Math.Sin(5 * angle), 3);

            DrawDirectionalLight(light.Direction,new Color(light.Color));

            // Reset the RenderTarget
            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // If SSAO is enabled, set the RenderTarget
            if (SSAORenderer.Enabled)
                GraphicsDevice.SetRenderTarget(sceneRT);

            // Set the effect parameters
            finalCombineEffect.Parameters["colorMap"].SetValue(colorRT);
            finalCombineEffect.Parameters["lightMap"].SetValue(lightRT);
            finalCombineEffect.Parameters["halfPixel"].SetValue(halfPixel);
            finalCombineEffect.Parameters["shadowOcclusion"].SetValue(shadowOcclusion);

            finalCombineEffect.Techniques[0].Passes[0].Apply();

            // Render a full-screen quad
            quadRenderer.Render(Vector2.One * -1, Vector2.One);
        }

        /// <summary>
        /// Draws a point light.
        /// </summary>
        /// <param name="lightPosition">The light position.</param>
        /// <param name="color">The color.</param>
        /// <param name="lightRadius">The light radius.</param>
        /// <param name="lightIntensity">The light intensity.</param>
        private void DrawPointLight(Vector3 lightPosition, Color color, float lightRadius, float lightIntensity)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Set the G-Buffer parameters
            pointLightEffect.Parameters["colorMap"].SetValue(colorRT);
            pointLightEffect.Parameters["normalMap"].SetValue(normalRT);
            pointLightEffect.Parameters["depthMap"].SetValue(depthRT);

            // Compute the light world matrix
            // scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
            pointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            pointLightEffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);

            // Light position
            pointLightEffect.Parameters["lightPosition"].SetValue(lightPosition);

            // Set the color, radius and Intensity
            pointLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            pointLightEffect.Parameters["lightRadius"].SetValue(lightRadius);
            pointLightEffect.Parameters["lightIntensity"].SetValue(lightIntensity);

            // Parameters for specular computations
            pointLightEffect.Parameters["cameraPosition"].SetValue(camera.Position);
            pointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(camera.ViewMatrix * camera.ProjectionMatrix));

            // Size of a halfpixel, for texture coordinates alignment
            pointLightEffect.Parameters["halfPixel"].SetValue(halfPixel);

            // Calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(camera.Position, lightPosition);

            // If we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < lightRadius)
                GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            else
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Apply the Effect
            pointLightEffect.Techniques[0].Passes[0].Apply();

            // Draw the Sphere mesh
            foreach (ModelMesh mesh in sphereModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
                    GraphicsDevice.Indices = meshPart.IndexBuffer;
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }

            // Reset RenderStates
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        /// <summary>
        /// Initializes the component. Override this method to load any non-graphics resources and query for any required services.
        /// </summary>
        public override void Initialize()
        {
            // Make our directional light source
            light = new Lights.DirectionalLight();
            light.Direction = new Vector3(-1, -1, -1);
            light.Color = new Vector3(0.7f, 0.7f, 0.7f);

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

            colorRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            depthRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            depthTexture = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            lightRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            sceneRT = new RenderTarget2D(GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            
            // Initialize SceneManager
            scene.InitializeScene(camera);

            // Load Effects
            clearBufferEffect = Game.Content.Load<Effect>("Shaders/GBuffer/ClearGBuffer");
            directionalLightEffect = Game.Content.Load<Effect>("Shaders/Lights/DirectionalLight");
            finalCombineEffect = Game.Content.Load<Effect>("Shaders/GBuffer/CombineFinal");
            pointLightEffect = Game.Content.Load<Effect>("Shaders/Lights/PointLight");
            depthShadowEffect = Game.Content.Load<Effect>("Shaders/Shadows/Depth");
            sphereModel = Game.Content.Load<Model>("Models/sphere");

            // Instantiate SpriteBatch
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            // Make our ShadowRenderer
            shadowRenderer = new CascadeShadowRenderer(GraphicsDevice, Game.Content);

            // Instantiate the SSAO Renderer
            ssaoRenderer = new SSAORenderer(Game, backBufferWidth, backBufferHeight);
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
                shadowRenderer.Enabled = !shadowRenderer.Enabled;

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

            lastMouseX = mouseState.X;
            lastMouseY = mouseState.Y;
            lastKeyboardState = keyboardState;

            base.Update(gameTime);
        } 
        #endregion
    }
}
