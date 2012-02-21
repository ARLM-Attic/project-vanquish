using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectVanquish.Cameras;
using ProjectVanquish.Core;
using ProjectVanquish.Renderers.PostProcessing;

namespace ProjectVanquish.Renderers
{
    public class DeferredRenderer
    {
        #region Fields
        ContentManager content;
        GraphicsDevice device;
        private RenderTarget2D colorRT, normalRT, depthRT, linearDepthRT, lightRT, sceneRT, bloomRT;
        private RenderTargetBinding[] renderTargets;
        private Effect clearBufferEffect, depthEffect, finalEffect, directionalLightEffect, pointLightEffect;
        private Vector2 halfPixel;
        private QuadRenderer fullscreenQuad;
        private SpriteBatch spriteBatch;
        private SceneManager sceneManager;
        private Model sphere;
        private CascadeShadowRenderer shadowRenderer;
        private SSAORenderer ssaoRenderer;
        private Bloom bloom; 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredRenderer"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="content">The content.</param>
        public DeferredRenderer(GraphicsDevice device, ContentManager content)
        {
            this.content = content;
            this.device = device;

            int backbufferWidth = device.PresentationParameters.BackBufferWidth;
            int backbufferHeight = device.PresentationParameters.BackBufferHeight;

            halfPixel = new Vector2()
            {
                X = 0.5f / (float)backbufferWidth,
                Y = 0.5f / (float)backbufferHeight
            };

            colorRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            depthRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            linearDepthRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            lightRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            sceneRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            bloomRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            renderTargets = new RenderTargetBinding[] { colorRT, normalRT, depthRT };

            clearBufferEffect = content.Load<Effect>("Shaders/GBuffer/Clear");
            depthEffect = content.Load<Effect>("Shaders/GBuffer/Depth");
            finalEffect = content.Load<Effect>("Shaders/GBuffer/Final");
            directionalLightEffect = content.Load<Effect>("Shaders/Lights/DirectionalLight");
            pointLightEffect = content.Load<Effect>("Shaders/Lights/PointLight");

            fullscreenQuad = new QuadRenderer(device);

            spriteBatch = new SpriteBatch(device);

            sceneManager = new SceneManager(device, content);

            shadowRenderer = new CascadeShadowRenderer(device, content);

            ssaoRenderer = new SSAORenderer(device, content, backbufferWidth, backbufferHeight);

            bloom = new Bloom(device, content);

            sphere = content.Load<Model>("Models/Sphere");
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether [use SSAO].
        /// </summary>
        /// <value><c>true</c> if [use SSAO]; otherwise, <c>false</c>.</value>
        public bool UseSSAO { get; set; } 
        #endregion

        #region Members
        /// <summary>
        /// Clears the G buffer.
        /// </summary>
        void ClearGBuffer()
        {
            clearBufferEffect.CurrentTechnique.Passes[0].Apply();
            fullscreenQuad.Draw();
        }

        /// <summary>
        /// Combines the G buffer.
        /// </summary>
        void CombineGBuffer(ref RenderTarget2D shadowOcclusion)
        {
            // Set Scene RenderTarget
            device.SetRenderTarget(sceneRT);

            finalEffect.Parameters["colorMap"].SetValue(colorRT);
            finalEffect.Parameters["lightMap"].SetValue(lightRT);
            finalEffect.Parameters["shadowMap"].SetValue(shadowOcclusion);
            finalEffect.Parameters["halfPixel"].SetValue(halfPixel);
            finalEffect.CurrentTechnique.Passes[0].Apply();
            fullscreenQuad.Draw();
        }

        /// <summary>
        /// Resolves the G buffer.
        /// </summary>
        void ResolveGBuffer()
        {
            device.SetRenderTargets(null);
        }

        /// <summary>
        /// Sets the G buffer.
        /// </summary>
        void SetGBuffer()
        {
            device.SetRenderTargets(renderTargets);
        }

        /// <summary>
        /// Draws the current frame.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Draw(GameTime gameTime)
        {
            SetGBuffer();
            ClearGBuffer();
            sceneManager.Draw();
            ResolveGBuffer();
            DrawDepth();
            var shadowOcclusion = shadowRenderer.Draw(device, linearDepthRT, sceneManager);
            DrawLights();
            CombineGBuffer(ref shadowOcclusion);
            if (UseSSAO)
            {
                ssaoRenderer.Draw(device, renderTargets, sceneRT, sceneManager, bloomRT);
                bloom.Draw(device, bloomRT);
            }
            else
                bloom.Draw(device, sceneRT);
            DrawDebug(ref shadowOcclusion);
        }

        /// <summary>
        /// Draws the depth.
        /// </summary>
        void DrawDepth()
        {
            // Set the Depth RenderTarget
            device.SetRenderTarget(linearDepthRT);

            // Clear the scene
            device.Clear(ClearOptions.Target, new Vector4(1.0f), 1.0f, 0);
            device.Clear(ClearOptions.DepthBuffer, new Vector4(1.0f), 1.0f, 0);

            // Set the Depth effect
            depthEffect.CurrentTechnique = depthEffect.Techniques["LinearDepth"];
            depthEffect.Parameters["g_matView"].SetValue(CameraManager.GetActiveCamera().ViewMatrix);
            depthEffect.Parameters["g_matProj"].SetValue(CameraManager.GetActiveCamera().ProjectionMatrix);
            depthEffect.Parameters["g_fFarClip"].SetValue(CameraManager.GetActiveCamera().FarClip);
            
            // Draw the Models
            sceneManager.Draw(device, depthEffect);

            device.SetRenderTarget(null);
        }

        /// <summary>
        /// Draws the lights.
        /// </summary>
        void DrawLights()
        {
            device.SetRenderTarget(lightRT);
            device.Clear(Color.Transparent);
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.None;

            // Draw lights
            DrawDirectionalLight(new Vector3(0, -1, 0), Color.White);

            DrawPointLight(new Vector3(-5, 1, 1), Color.Gold, 10, 1);
            DrawPointLight(new Vector3(0, 1, 1), Color.Gold, 10, 1);
            DrawPointLight(new Vector3(5, 1, 1), Color.Gold, 10, 1);

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.SetRenderTarget(null);
        }

        /// <summary>
        /// Draws the directional light.
        /// </summary>
        /// <param name="lightDirection">The light direction.</param>
        /// <param name="color">The color.</param>
        void DrawDirectionalLight(Vector3 lightDirection, Color color)
        {
            directionalLightEffect.Parameters["colorMap"].SetValue(colorRT);
            directionalLightEffect.Parameters["normalMap"].SetValue(normalRT);
            directionalLightEffect.Parameters["depthMap"].SetValue(depthRT);
            directionalLightEffect.Parameters["lightDirection"].SetValue(lightDirection);
            directionalLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            directionalLightEffect.Parameters["cameraPosition"].SetValue(CameraManager.GetActiveCamera().Position);
            directionalLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(CameraManager.GetActiveCamera().ViewMatrix
                                                                                            * CameraManager.GetActiveCamera().ProjectionMatrix));
            directionalLightEffect.Parameters["halfPixel"].SetValue(halfPixel);
            directionalLightEffect.Techniques[0].Passes[0].Apply();
            fullscreenQuad.Draw();
        }

        /// <summary>
        /// Draws the point light.
        /// </summary>
        /// <param name="lightPosition">The light position.</param>
        /// <param name="color">The color.</param>
        /// <param name="lightRadius">The light radius.</param>
        /// <param name="lightIntensity">The light intensity.</param>
        void DrawPointLight(Vector3 lightPosition, Color color, float lightRadius, float lightIntensity)
        {
            // Set the G-Buffer parameters
            pointLightEffect.Parameters["colorMap"].SetValue(colorRT);
            pointLightEffect.Parameters["normalMap"].SetValue(normalRT);
            pointLightEffect.Parameters["depthMap"].SetValue(depthRT);

            // Compute the Light World matrix
            // Scale according to Light radius and translate it to Light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
            pointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightEffect.Parameters["View"].SetValue(CameraManager.GetActiveCamera().ViewMatrix);
            pointLightEffect.Parameters["Projection"].SetValue(CameraManager.GetActiveCamera().ProjectionMatrix);

            // Light position
            pointLightEffect.Parameters["lightPosition"].SetValue(lightPosition);

            // Set the color, radius and Intensity
            pointLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            pointLightEffect.Parameters["lightRadius"].SetValue(lightRadius);
            pointLightEffect.Parameters["lightIntensity"].SetValue(lightIntensity);

            // Parameters for specular computations
            pointLightEffect.Parameters["cameraPosition"].SetValue(CameraManager.GetActiveCamera().Position);
            pointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(CameraManager.GetActiveCamera().ViewMatrix
                                                                                      * CameraManager.GetActiveCamera().ProjectionMatrix));

            // Size of a halfpixel, for texture coordinates alignment
            pointLightEffect.Parameters["halfPixel"].SetValue(halfPixel);

            // Calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(CameraManager.GetActiveCamera().Position, lightPosition);

            // If we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < lightRadius)
                device.RasterizerState = RasterizerState.CullClockwise;
            else
                device.RasterizerState = RasterizerState.CullCounterClockwise;

            device.DepthStencilState = DepthStencilState.None;

            pointLightEffect.Techniques[0].Passes[0].Apply();
            foreach (ModelMesh mesh in sphere.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    device.Indices = meshPart.IndexBuffer;
                    device.SetVertexBuffer(meshPart.VertexBuffer);
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }

            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;
        }

        /// <summary>
        /// Draws the debug output.
        /// </summary>
        public void DrawDebug(ref RenderTarget2D shadowOcclusion)
        {
            int width = 128;
            int height = 128;

            // Set up a Drawing rectangle
            Rectangle rect = new Rectangle(0, 0, width, height);

            // Draw Color RenderTarget
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
            spriteBatch.Draw((Texture2D)colorRT, rect, Color.White);
            rect.X += width;
            spriteBatch.Draw((Texture2D)normalRT, rect, Color.White);
            rect.X += width;
            spriteBatch.Draw((Texture2D)depthRT, rect, Color.White);
            rect.X += width;
            spriteBatch.Draw((Texture2D)shadowRenderer.ShadowMap, rect, Color.White);
            rect.X += width;
            spriteBatch.Draw((Texture2D)shadowRenderer.ShadowOcclusion, rect, Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// Updates the specified game time.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {
            sceneManager.Update(gameTime);
        } 
        #endregion
    }
}
