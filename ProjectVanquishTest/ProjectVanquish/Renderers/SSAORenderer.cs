using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectVanquish.Core;
using ProjectVanquish.Cameras;

namespace ProjectVanquish.Renderers
{
    /// <summary>
    /// Screen Space Ambient Occlusion Renderer
    /// </summary>
    public class SSAORenderer
    {
        #region Fields
        static bool isEnabled;
        Effect ssaoEffect, blurEffect, finalEffect;
        public RenderTarget2D ssaoRT, blurRT;
        static float sampleRadius, distanceScale;
        Texture2D randomNormals;
        QuadRenderer fullscreenQuad;
        Vector2 gBufferTextureSize, halfPixel;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SSAORenderer"/> class.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="Width">The width.</param>
        /// <param name="Height">The height.</param>
        public SSAORenderer(Game game, int Width, int Height)
        {
            // Load SSAO effects
            ssaoEffect = game.Content.Load<Effect>("Shaders/SSAO/SSAO");
            blurEffect = game.Content.Load<Effect>("Shaders/SSAO/Blur");
            finalEffect = game.Content.Load<Effect>("Shaders/SSAO/Final");

            // Create RenderTargets
            ssaoRT = new RenderTarget2D(game.GraphicsDevice, Width, Height, false, SurfaceFormat.Color, DepthFormat.None);
            blurRT = new RenderTarget2D(game.GraphicsDevice, Width, Height, false, SurfaceFormat.Color, DepthFormat.None);

            // Instantiate the GBuffer Texture Size
            gBufferTextureSize = new Vector2(Width, Height);

            // Instantiate the HalfPixel
            halfPixel = new Vector2()
            {
                X = 1.0f / Width,
                Y = 1.0f / Height
            };

            // Instantiate the QuadRenderer
            fullscreenQuad = new QuadRenderer(game);
            game.Components.Add(fullscreenQuad);

            // Load the Random Normals Texture
            randomNormals = game.Content.Load<Texture2D>("Textures/RandomNormal");
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the distance scale.
        /// </summary>
        /// <value>The distance scale.</value>
        public static float DistanceScale
        {
            get { return distanceScale; }
            set { distanceScale = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SSAORenderer"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public static bool Enabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the sample radius.
        /// </summary>
        /// <value>The sample radius.</value>
        public static float SampleRadius
        {
            get { return sampleRadius; }
            set { sampleRadius = value; }
        }
        #endregion

        #region Members
        /// <summary>
        /// Blurs the SSAO.
        /// </summary>
        /// <param name="device">The device.</param>
        void BlurSSAO(GraphicsDevice device)
        {
            // Set the Blur RenderTarget and clear the GraphicsDevice
            device.SetRenderTarget(blurRT);
            device.Clear(Color.White);

            // Set SSAO RenderTarget on the GraphicsDevice
            device.Textures[3] = ssaoRT;
            device.SamplerStates[3] = SamplerState.LinearClamp;

            // Set Effect Parameters
            blurEffect.Parameters["blurDirection"].SetValue(Vector2.One);
            blurEffect.Parameters["targetSize"].SetValue(gBufferTextureSize);
            
            // Apply Effect
            blurEffect.CurrentTechnique.Passes[0].Apply();

            // Draw
            fullscreenQuad.Render(Vector2.One * -1, Vector2.One);
        }

        /// <summary>
        /// Composes the SSAO Effect.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="sceneRT">The scene RT.</param>
        /// <param name="output">The output.</param>
        void Compose(GraphicsDevice device, RenderTarget2D sceneRT, RenderTarget2D output)
        {
            // Set the RenderTarget
            device.SetRenderTarget(output);
            device.Clear(Color.White);

            // Set Samplers
            device.Textures[0] = sceneRT;
            device.SamplerStates[0] = SamplerState.LinearClamp;
            device.Textures[1] = blurRT;
            device.SamplerStates[1] = SamplerState.LinearClamp;

            // Set Final Parameters
            finalEffect.Parameters["halfPixel"].SetValue(halfPixel);

            // Apply Effect
            finalEffect.CurrentTechnique.Passes[0].Apply();

            // Draw
            fullscreenQuad.Render(Vector2.One * -1, Vector2.One);
        }

        /// <summary>
        /// Draws the SSAO Effect.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="depthRT">The depth RenderTarget.</param>
        /// <param name="sceneRT">The scene RenderTarget.</param>
        /// <param name="scene">The scene.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="output">The output.</param>
        public void Draw(GraphicsDevice device, RenderTarget2D normalRT, RenderTarget2D depthRT, RenderTarget2D sceneRT, SceneManager scene, Camera camera, RenderTarget2D output)
        {
            // Reset RenderStates
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.RasterizerState = RasterizerState.CullCounterClockwise;

            RenderSSAO(device, normalRT, depthRT, camera);
            BlurSSAO(device);
            Compose(device, sceneRT, output);
        }

        /// <summary>
        /// Renders the SSAO.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="normalRT">The normal RT.</param>
        /// <param name="depthRT">The depth RT.</param>
        /// <param name="camera">The camera.</param>
        void RenderSSAO(GraphicsDevice device, RenderTarget2D normalRT, RenderTarget2D depthRT, Camera camera)
        {
            // Set the RenderTarget and clear the GraphicsDevice
            device.SetRenderTarget(ssaoRT);
            device.Clear(Color.White);

            // Set Samplers
            device.Textures[1] = normalRT;
            device.SamplerStates[1] = SamplerState.LinearClamp;
            device.Textures[2] = normalRT;
            device.SamplerStates[2] = SamplerState.PointClamp;
            device.Textures[3] = randomNormals;
            device.SamplerStates[3] = SamplerState.LinearWrap;

            // Calculate Frustum Corner of the Camera
            Vector3 cornerFrustum = Vector3.Zero;
            cornerFrustum.Y = (float)Math.Tan(Math.PI / 3.0 / 2.0) * camera.FarClip;
            cornerFrustum.X = cornerFrustum.Y * camera.AspectRatio;
            cornerFrustum.Z = camera.FarClip;

            // Set SSAO parameters
            ssaoEffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            ssaoEffect.Parameters["cornerFrustum"].SetValue(cornerFrustum);
            ssaoEffect.Parameters["sampleRadius"].SetValue(sampleRadius);
            ssaoEffect.Parameters["distanceScale"].SetValue(distanceScale);
            ssaoEffect.Parameters["GBufferTextureSize"].SetValue(gBufferTextureSize);

            // Apply Effect
            ssaoEffect.CurrentTechnique.Passes[0].Apply();

            // Draw
            fullscreenQuad.Render(Vector2.One * -1, Vector2.One);
        }
        #endregion
    }
}
