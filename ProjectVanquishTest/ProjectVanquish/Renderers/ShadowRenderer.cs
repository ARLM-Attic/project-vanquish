using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ProjectVanquish.Cameras;
using ProjectVanquish.Core;

namespace ProjectVanquish.Renderers
{
    /// <summary>
    /// Shadow Filtering Type
    /// </summary>
    public enum ShadowFilteringType
    {
        /// <summary>
        /// Percentage Closer Filtering 2x2
        /// </summary>
        PCF2x2 = 0,
        /// <summary>
        /// Percentage Closer Filtering 3x3
        /// </summary>
        PCF3x3 = 1,
        /// <summary>
        /// Percentage Closer Filtering 5x5
        /// </summary>
        PCF5x5 = 2,
        /// <summary>
        /// Percentage Closer Filtering 7x7
        /// </summary>
        PCF7x7 = 3
    }

    /// <summary>
    /// Shadow Renderer
    /// </summary>
    public class ShadowRenderer
    {
        #region Constants
        /// <summary>
        /// Shadow Map Size
        /// </summary>
        public const int ShadowMapSize = 2048;
        #endregion

        #region Fields
        RenderTarget2D shadowMap;
        Effect shadowMapEffect;
        RenderTarget2D shadowOcclusion;
        RenderTarget2D disabledShadowOcclusion;

        ContentManager contentManager;
        GraphicsDevice graphicsDevice;

        FullScreenQuad fullScreenQuad;

        Vector3[] frustumCornersVS = new Vector3[8];
        Vector3[] frustumCornersWS = new Vector3[8];
        Vector3[] frustumCornersLS = new Vector3[8];
        Vector3[] farFrustumCornersVS = new Vector3[4];
        OrthographicCamera lightCamera;

        static bool enabled = true;
        ShadowFilteringType filteringType = ShadowFilteringType.PCF5x5;

        EffectTechnique[] shadowOcclusionTechniques = new EffectTechnique[4]; 
        #endregion
        
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ShadowRenderer"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="contentManager">The content manager.</param>
        public ShadowRenderer(GraphicsDevice graphicsDevice,
                                ContentManager contentManager)
        {
            this.contentManager = contentManager;
            this.graphicsDevice = graphicsDevice;

            // Load the effect we need
            shadowMapEffect = contentManager.Load<Effect>("Shaders/Shadows/ShadowMap");

            // Create the shadow map, using a 32-bit floating-point surface format
            shadowMap = new RenderTarget2D(graphicsDevice, ShadowMapSize, ShadowMapSize, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);

            // Create the shadow occlusion texture using the same dimensions as the backbuffer
            shadowOcclusion = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight, false,
                                                 SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);

            // Create a 1x1 texture that we'll clear to white and return when shadows are disabled
            disabledShadowOcclusion = new RenderTarget2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            // Create the full-screen quad
            fullScreenQuad = new FullScreenQuad(graphicsDevice);

            // We'll keep an array of EffectTechniques that will let us map a
            // ShadowFilteringType to a technique for calculating shadow occlusion
            shadowOcclusionTechniques[0] = shadowMapEffect.Techniques["CreateShadowTerm2x2PCF"];
            shadowOcclusionTechniques[1] = shadowMapEffect.Techniques["CreateShadowTerm3x3PCF"];
            shadowOcclusionTechniques[2] = shadowMapEffect.Techniques["CreateShadowTerm5x5PCF"];
            shadowOcclusionTechniques[3] = shadowMapEffect.Techniques["CreateShadowTerm7x7PCF"];
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether or not shadow occlusion
        /// should be calculated.
        /// </summary>
        public static bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets or sets the type of the shadow filtering.
        /// </summary>
        /// <value>The type of the shadow filtering.</value>
        public ShadowFilteringType ShadowFilteringType
        {
            get { return filteringType; }
            set { filteringType = value; }
        }

        /// <summary>
        /// Gets the shadow map.
        /// </summary>
        /// <value>The shadow map.</value>
        public RenderTarget2D ShadowMap { get { return shadowMap; } } 
        #endregion

        #region Members
        /// <summary>
        /// Determines the size of the frustum needed to cover the viewable area,
        /// then creates an appropriate orthographic projection.
        /// </summary>
        /// <param name="light">The directional light to use</param>
        /// <param name="mainCamera">The camera viewing the scene</param>
        protected void CalculateFrustum(Lights.DirectionalLight light, Camera mainCamera)
        {
            // Shorten the view frustum according to the shadow view distance
            Matrix cameraMatrix;
            mainCamera.GetWorldMatrix(out cameraMatrix);

            // Find the centroid
            Vector3 frustumCentroid = new Vector3(0, 0, 0);
            for (int i = 0; i < 8; i++)
                frustumCentroid += frustumCornersWS[i];

            frustumCentroid /= 8;

            // Position the shadow-caster camera so that it's looking at the centroid,
            // and backed up in the direction of the sunlight
            float distFromCentroid = MathHelper.Max((mainCamera.FarClip - mainCamera.NearClip), Vector3.Distance(frustumCornersVS[4], frustumCornersVS[5])) + 50.0f;
            Matrix viewMatrix = Matrix.CreateLookAt(frustumCentroid - (light.Direction * distFromCentroid), frustumCentroid, new Vector3(0, 1, 0));

            // Determine the position of the frustum corners in light space
            Vector3.Transform(frustumCornersWS, ref viewMatrix, frustumCornersLS);

            // Calculate an orthographic projection by sizing a bounding box 
            // to the frustum coordinates in light space
            Vector3 mins = frustumCornersLS[0];
            Vector3 maxes = frustumCornersLS[0];
            for (int i = 0; i < 8; i++)
            {
                if (frustumCornersLS[i].X > maxes.X)
                    maxes.X = frustumCornersLS[i].X;
                else if (frustumCornersLS[i].X < mins.X)
                    mins.X = frustumCornersLS[i].X;
                if (frustumCornersLS[i].Y > maxes.Y)
                    maxes.Y = frustumCornersLS[i].Y;
                else if (frustumCornersLS[i].Y < mins.Y)
                    mins.Y = frustumCornersLS[i].Y;
                if (frustumCornersLS[i].Z > maxes.Z)
                    maxes.Z = frustumCornersLS[i].Z;
                else if (frustumCornersLS[i].Z < mins.Z)
                    mins.Z = frustumCornersLS[i].Z;
            }

            // Create an orthographic camera for use as a shadow caster
            const float nearClipOffset = 100.0f;
            lightCamera = new OrthographicCamera(mins.X, maxes.X, mins.Y, maxes.Y, -maxes.Z - nearClipOffset, -mins.Z);
            lightCamera.SetViewMatrix(ref viewMatrix);
        }

        /// <summary>
        /// Renders a list of models to the shadow map, and returns a surface 
        /// containing the shadow occlusion factor
        /// </summary>
        /// <param name="modelList">The list of models to render</param>
        /// <param name="depthTexture">Texture containing depth for the scene</param>
        /// <param name="light">The light for which the shadow is being calculated</param>
        /// <param name="mainCamera">The camera viewing the scene containing the light</param>
        /// <returns>The shadow occlusion texture</returns>
        public RenderTarget2D Draw(SceneManager scene, RenderTarget2D depthTexture, Lights.DirectionalLight light, Camera mainCamera)
        {
            if (enabled)
            {
                // Set our targets
                graphicsDevice.SetRenderTarget(shadowMap);
                graphicsDevice.Clear(ClearOptions.Target, Color.White, 1.0f, 0);
                graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

                // Get corners of the main camera's bounding frustum
                Matrix cameraTransform, viewMatrix;
                mainCamera.GetWorldMatrix(out cameraTransform);
                mainCamera.GetViewMatrix(out viewMatrix);
                mainCamera.BoundingFrustum.GetCorners(frustumCornersWS);
                Vector3.Transform(frustumCornersWS, ref viewMatrix, frustumCornersVS);
                for (int i = 0; i < 4; i++)
                    farFrustumCornersVS[i] = frustumCornersVS[i + 4];

                CalculateFrustum(light, mainCamera);

                DrawShadowMap(scene);

                DrawShadowOcclusion(mainCamera, depthTexture);

                return shadowOcclusion;
            }
            else
            {
                // If we're disabled, just clear our 1x1 texture to white and return it
                graphicsDevice.SetRenderTarget(disabledShadowOcclusion);
                graphicsDevice.Clear(ClearOptions.Target, Color.White, 1.0f, 0);

                return disabledShadowOcclusion;
            }
        }

        /// <summary>
        /// Renders the shadow map using the orthographic camera created in
        /// CalculateFrustum.
        /// </summary>
        /// <param name="modelList">The list of models to be rendered</param>        
        protected void DrawShadowMap(SceneManager scene)
        {
            // Set the shadow map as the current render target and clear it
            //graphicsDevice.SetRenderTarget(shadowMap);
            //graphicsDevice.Clear(ClearOptions.Target, new Vector4(1.0f), 1.0f, 0);
            //graphicsDevice.Clear(ClearOptions.DepthBuffer, new Vector4(1.0f), 1.0f, 0);

            // Set up the Effect
            shadowMapEffect.CurrentTechnique = shadowMapEffect.Techniques["GenerateShadowMap"];
            shadowMapEffect.Parameters["g_matViewProj"].SetValue(lightCamera.ViewProjectionMatrix);

            // Apply the Effect
            shadowMapEffect.CurrentTechnique.Passes[0].Apply();

            // Draw the models
            scene.DrawSceneWithCustomEffect(graphicsDevice, shadowMapEffect);
        }

        /// <summary>
        /// Renders a texture containing the final shadow occlusion
        /// </summary>
        protected void DrawShadowOcclusion(Camera mainCamera, RenderTarget2D depthTexture)
        {
            // Set the RenderTarget
            graphicsDevice.SetRenderTarget(shadowOcclusion);

            Matrix cameraTransform;
            mainCamera.GetWorldMatrix(out cameraTransform);

            // Setup the Effect
            shadowMapEffect.CurrentTechnique = shadowOcclusionTechniques[(int)filteringType];
            shadowMapEffect.Parameters["g_matInvView"].SetValue(cameraTransform);
            shadowMapEffect.Parameters["g_matLightViewProj"].SetValue(lightCamera.ViewProjectionMatrix);
            shadowMapEffect.Parameters["g_vFrustumCornersVS"].SetValue(farFrustumCornersVS);
            shadowMapEffect.Parameters["ShadowMap"].SetValue(shadowMap);
            shadowMapEffect.Parameters["DepthTexture"].SetValue(depthTexture);
            shadowMapEffect.Parameters["g_vOcclusionTextureSize"].SetValue(new Vector2(shadowOcclusion.Width, shadowOcclusion.Height));
            shadowMapEffect.Parameters["g_vShadowMapSize"].SetValue(new Vector2(ShadowMapSize, ShadowMapSize));

            // Apply the Effect
            shadowMapEffect.CurrentTechnique.Passes[0].Apply();

            // Draw the FullscreenQuad		
            fullScreenQuad.Draw(graphicsDevice);
        } 
        #endregion
    }
}
