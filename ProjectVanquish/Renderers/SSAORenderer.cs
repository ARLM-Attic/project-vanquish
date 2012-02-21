using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectVanquish.Core;

namespace ProjectVanquish.Renderers
{
    class SSAORenderer
    {
        #region Fields
        Effect ssaoEffect, blurEffect, finalEffect;
        RenderTarget2D ssaoRT, blurRT;
        float sampleRadius, distanceScale;
        Texture2D randomNormals;
        QuadRenderer fullscreenQuad;
        Vector2 halfPixel, gBufferTextureSize;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SSAORenderer"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="content">The content.</param>
        /// <param name="Width">The width.</param>
        /// <param name="Height">The height.</param>
        public SSAORenderer(GraphicsDevice device, ContentManager content, int Width, int Height)
        {
            // Load SSAO effects
            ssaoEffect = content.Load<Effect>("Shaders/SSAO/SSAO");
            blurEffect = content.Load<Effect>("Shaders/SSAO/Blur");
            finalEffect = content.Load<Effect>("Shaders/SSAO/Final");

            // Create RenderTargets
            ssaoRT = new RenderTarget2D(device, Width, Height, false, SurfaceFormat.Color, DepthFormat.None);
            blurRT = new RenderTarget2D(device, Width, Height, false, SurfaceFormat.Color, DepthFormat.None);

            fullscreenQuad = new QuadRenderer(device);
            randomNormals = content.Load<Texture2D>("Textures/RandomNormal");

            // Set Sample Radius to Default
            sampleRadius = 0.05f;

            // Set Distance Scale to Default
            distanceScale = 30f;

            // Instantiate halfPixel and GBuffer Texture Size
            halfPixel = new Vector2()
            {
                X = 1.0f / ssaoRT.Width,
                Y = 1.0f / ssaoRT.Height
            };
            gBufferTextureSize = new Vector2(ssaoRT.Width, ssaoRT.Height);
        } 
        #endregion

        #region Members
        /// <summary>
        /// Blurs the SSAO.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="gBuffer">The g buffer.</param>
        void BlurSSAO(GraphicsDevice device, RenderTargetBinding[] gBuffer)
        {
            device.SetRenderTarget(blurRT);
            device.Clear(Color.White);

            device.Textures[3] = ssaoRT;
            device.SamplerStates[3] = SamplerState.LinearClamp;

            // Set Blur parameters
            blurEffect.Parameters["blurDirection"].SetValue(new Vector2(1.0f / 800.0f, 0.0f));

            // Apply Effect
            blurEffect.CurrentTechnique.Passes[0].Apply();

            // Draw
            fullscreenQuad.Draw();
        }

        /// <summary>
        /// Composes the SSAO Effect.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="sceneRT">The scene RT.</param>
        /// <param name="output">The output.</param>
        void Compose(GraphicsDevice device, RenderTarget2D sceneRT, RenderTarget2D output)
        {
            device.SetRenderTarget(output);
            device.Clear(Color.White);

            // Set Final Parameters
            device.Textures[0] = sceneRT;
            device.SamplerStates[0] = SamplerState.LinearClamp;
            device.Textures[1] = ssaoRT;
            device.SamplerStates[1] = SamplerState.LinearClamp;
            finalEffect.Parameters["halfPixel"].SetValue(halfPixel);

            // Apply Effect
            finalEffect.CurrentTechnique.Passes[0].Apply();

            // Draw
            fullscreenQuad.Draw();
        }

        /// <summary>
        /// Draws the SSAO Effect.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="gBuffer">The g buffer.</param>
        /// <param name="sceneRT">The scene RT.</param>
        /// <param name="scene">The scene.</param>
        /// <param name="output">The output.</param>
        public void Draw(GraphicsDevice device, RenderTargetBinding[] gBuffer, RenderTarget2D sceneRT, SceneManager scene, RenderTarget2D output)
        {
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.RasterizerState = RasterizerState.CullCounterClockwise;

            RenderSSAO(device, gBuffer, scene);
            BlurSSAO(device, gBuffer);
            Compose(device, sceneRT, output);
        }

        /// <summary>
        /// Renders the SSAO.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="gBuffer">The g buffer.</param>
        /// <param name="scene">The scene.</param>
        void RenderSSAO(GraphicsDevice device, RenderTargetBinding[] gBuffer, SceneManager scene)
        {
            device.SetRenderTarget(ssaoRT);
            device.Clear(Color.White);

            // Calculate Frustum Corner of the Camera
            Vector3 cornerFrustum = Vector3.Zero;
            cornerFrustum.Y = (float)Math.Tan(Math.PI / 3.0 / 2.0) * CameraManager.GetActiveCamera().FarClip;
            cornerFrustum.X = cornerFrustum.Y * CameraManager.GetActiveCamera().AspectRatio;
            cornerFrustum.Z = CameraManager.GetActiveCamera().FarClip;

            device.SamplerStates[0] = SamplerState.PointClamp;
            device.Textures[1] = gBuffer[1].RenderTarget;
            device.SamplerStates[1] = SamplerState.LinearClamp;
            device.Textures[2] = gBuffer[1].RenderTarget;
            device.SamplerStates[2] = SamplerState.PointClamp;
            device.Textures[3] = randomNormals;
            device.SamplerStates[3] = SamplerState.LinearWrap;

            // Set SSAO parameters
            ssaoEffect.Parameters["Projection"].SetValue(CameraManager.GetActiveCamera().ProjectionMatrix);
            ssaoEffect.Parameters["cornerFustrum"].SetValue(cornerFrustum);
            ssaoEffect.Parameters["sampleRadius"].SetValue(sampleRadius);
            ssaoEffect.Parameters["distanceScale"].SetValue(distanceScale);
            ssaoEffect.Parameters["GBufferTextureSize"].SetValue(gBufferTextureSize);

            // Apply Effect
            ssaoEffect.CurrentTechnique.Passes[0].Apply();

            // Draw
            fullscreenQuad.Draw();
        } 
        #endregion
    }
}
