using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectVanquish.Renderers.PostProcessing
{
    class Bloom
    {
        #region Fields
        /// <summary>
        /// Bloom, Blur and Final Effects
        /// </summary>
        Effect bloomEffect, blurEffect, finalEffect;

        /// <summary>
        /// Bloom and Blur RenderTargets
        /// </summary>
        RenderTarget2D bloomRT, blurHRT, blurVRT;

        /// <summary>
        /// Quad Renderer
        /// </summary>
        QuadRenderer fullscreenQuad; 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Bloom"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="content">The content.</param>
        public Bloom(GraphicsDevice device, ContentManager content)
        {
            bloomEffect = content.Load<Effect>("Shaders/PostProcessing/Bloom/Bloom");
            blurEffect = content.Load<Effect>("Shaders/PostProcessing/Bloom/Blur");
            finalEffect = content.Load<Effect>("Shaders/PostProcessing/Bloom/Final");

            bloomRT = new RenderTarget2D(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight,
                                        false, SurfaceFormat.Color, DepthFormat.None);
            blurHRT = new RenderTarget2D(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight,
                                        false, SurfaceFormat.Color, DepthFormat.None);
            blurVRT = new RenderTarget2D(device, device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight,
                                        false, SurfaceFormat.Color, DepthFormat.None);

            fullscreenQuad = new QuadRenderer(device);
        } 
        #endregion

        #region Members
        /// <summary>
        /// Draws the Bloom Effect.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="sceneRT">The scene RT.</param>
        public void Draw(GraphicsDevice device, RenderTarget2D sceneRT)
        {
            // Bloom pass
            device.SetRenderTarget(bloomRT);
            device.Clear(Color.Black);
            device.Textures[0] = sceneRT;
            bloomEffect.CurrentTechnique.Passes[0].Apply();
            fullscreenQuad.Draw();
            device.SetRenderTarget(null);

            // Horizontal pass
            device.SetRenderTarget(blurHRT);
            device.Clear(Color.Black);
            device.Textures[0] = bloomRT;
            blurEffect.CurrentTechnique.Passes[0].Apply();
            fullscreenQuad.Draw();
            device.SetRenderTarget(null);

            // Vertical pass
            device.SetRenderTarget(blurVRT);
            device.Clear(Color.Black);
            device.Textures[0] = blurHRT;
            blurEffect.CurrentTechnique.Passes[0].Apply();
            fullscreenQuad.Draw();
            device.SetRenderTarget(null);

            // Final pass
            device.Clear(Color.Black);
            device.Textures[0] = blurVRT;
            finalEffect.Parameters["ColorMap"].SetValue(sceneRT);
            finalEffect.CurrentTechnique.Passes[0].Apply();
            fullscreenQuad.Draw();
        } 
        #endregion
    }
}
