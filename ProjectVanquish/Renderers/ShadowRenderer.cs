using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectVanquish.Cameras;
using ProjectVanquish.Core;

namespace ProjectVanquish.Renderers
{
    /// <summary>
    /// Shadow Filtering Type
    /// </summary>
    public enum ShadowFilteringType
    {
        PCF2x2 = 1,
        PCF3x3 = 2,
        PCF5x5 = 3,
        PCF7x7 = 4
    }

    class ShadowRenderer
    {
        #region Constants
        /// <summary>
        /// Shadow Map size
        /// </summary>
        public const int SHADOWMAPSIZE = 2048;
        #endregion

        #region Fields
        /// <summary>
        /// Is it enabled?
        /// </summary>
        bool isEnabled = true;

        /// <summary>
        /// Sprite Batch
        /// </summary>
        SpriteBatch spriteBatch;

        /// <summary>
        /// Frustum corners
        /// </summary>
        Vector3[] frustumCornersLS = new Vector3[8];
        Vector3[] frustumCornersVS = new Vector3[8];
        Vector3[] frustumCornersWS = new Vector3[8];
        Vector3[] farFrustumCornerVS = new Vector3[4];

        /// <summary>
        /// Light Cameras
        /// </summary>
        OrthographicCamera lightCamera;

        /// <summary>
        /// Shadow Effect
        /// </summary>
        Effect shadowEffect;

        /// <summary>
        /// Shadow Filtering Type - defaulted to 2x2
        /// </summary>
        ShadowFilteringType filteringType = ShadowFilteringType.PCF2x2;

        /// <summary>
        /// Disabled Shadow, Shadow Map and Shadow Occlusion RenderTarget
        /// </summary>
        RenderTarget2D disabledShadowOcclusion, shadowMap, shadowOcclusion;

        /// <summary>
        /// Effect Technique Collection
        /// </summary>
        EffectTechniqueCollection shadowOcclusionTechniques;
        
        /// <summary>
        /// Fullscreen Quad
        /// </summary>
        QuadRenderer fullscreenQuad;

        /// <summary>
        /// Directional Light
        /// </summary>
        static ProjectVanquish.Core.Lights.DirectionalLight light;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ShadowRenderer"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="contentManager">The content.</param>
        public ShadowRenderer(GraphicsDevice device, ContentManager contentManager)
        {
            // Load the Shadow Effect
            shadowEffect = contentManager.Load<Effect>("Shaders/Shadows/ShadowMap");

            // Instantiate the SpriteBatch
            spriteBatch = new SpriteBatch(device);

            // Create the ShadowMap RenderTarget
            shadowMap = new RenderTarget2D(device, SHADOWMAPSIZE, SHADOWMAPSIZE,
                                                false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, 
                                                device.PresentationParameters.MultiSampleCount, RenderTargetUsage.DiscardContents);
            
            // Create the Shadow Occlusion RenderTarget
            shadowOcclusion = new RenderTarget2D(device, device.PresentationParameters.BackBufferWidth,
                                                      device.PresentationParameters.BackBufferHeight,
                                                      false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8,
                                                      device.PresentationParameters.MultiSampleCount, RenderTargetUsage.DiscardContents);

            // Create the Disabled Shadow Occlusion RenderTarget
            disabledShadowOcclusion = new RenderTarget2D(device, 1, 1, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            // Create the Fullscreen Quad
            fullscreenQuad = new QuadRenderer(device);

            // Get the Techniques
            // 0 - GenerateShadowMap
            // 1 - CreateShadowTerm2x2PCF
            // 2 - CreateShadowTerm3x3PCF
            // 3 - CreateShadowTerm5x5PCF
            // 4 - CreateShadowTerm7x7PCF
            shadowOcclusionTechniques = shadowEffect.Techniques;

            // Create a default Directional light
            light = new ProjectVanquish.Core.Lights.DirectionalLight();
            light.Direction = new Vector3(-1, -1, -1);
            light.Color = new Vector3(0.7f);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        /// <summary>
        /// Gets the shadow occlusion.
        /// </summary>
        /// <value>The shadow occlusion.</value>
        public RenderTarget2D ShadowOcclusion
        {
            get { return shadowOcclusion; }
        }

        public RenderTarget2D ShadowMap
        {
            get { return shadowMap; }
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
        #endregion

        #region Members
        /// <summary>
        /// Calculates the frustum.
        /// </summary>
        /// <param name="light">The light.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="minZ">The min Z.</param>
        /// <param name="maxZ">The max Z.</param>
        /// <returns></returns>
        void CalculateFrustum(ProjectVanquish.Core.Lights.DirectionalLight light)
        {
            // Shorten the view frustum according to the shadow view distance
            Matrix cameraMatrix;
            CameraManager.GetActiveCamera().GetWorldMatrix(out cameraMatrix);

            Vector3 frustumCentroid = new Vector3(0,0,0);
            for (int i = 0; i < 8; i++)
                frustumCentroid += frustumCornersWS[i];

            frustumCentroid /= 8;
            
            // Position the shadow-caster camera so that it's looking at the centroid,
            // and backed up in the direction of the sunlight
            float distFromCentroid = MathHelper.Max((CameraManager.GetActiveCamera().FarClip - CameraManager.GetActiveCamera().NearClip), 
                                                    Vector3.Distance(frustumCornersVS[4], frustumCornersVS[5])) + 50.0f;
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
        /// Renders the specified device.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="camera">The camera.</param>
        public RenderTarget2D Draw(GraphicsDevice device, RenderTarget2D depthRT, SceneManager scene)
        {            
            // Create the Shadow Occlusion
            return Draw(device, depthRT, scene, light);
        }

        /// <summary>
        /// Renders the specified device.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="light">The light.</param>
        RenderTarget2D Draw(GraphicsDevice device, RenderTarget2D depthRT, SceneManager scene, ProjectVanquish.Core.Lights.DirectionalLight light)
        {
            if (isEnabled)
            {
                device.SetRenderTarget(shadowMap);
                device.Clear(ClearOptions.Target, Color.White, 1.0f, 0);
                device.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
                
                // Get corners of the main camera's BoundingFrustum
                Matrix cameraTransform, viewMatrix;
                CameraManager.GetActiveCamera().GetWorldMatrix(out cameraTransform);
                CameraManager.GetActiveCamera().GetViewMatrix(out viewMatrix);
                CameraManager.GetActiveCamera().BoundingFrustum.GetCorners(frustumCornersWS);
                Vector3.Transform(frustumCornersWS, ref viewMatrix, frustumCornersVS);

                for (int i = 0; i < 4; i++)
                    farFrustumCornerVS[i] = frustumCornersVS[i + 4];

                CalculateFrustum(light);

                DrawShadowMap(device, scene);
                
                // Render the shadow occlusion
                DrawShadowOcclusion(device, CameraManager.GetActiveCamera(), depthRT);

                return shadowOcclusion;
            }
            else
            {
                // Disabled.  Clear our 1x1 texture to white.
                device.SetRenderTarget(disabledShadowOcclusion);
                device.Clear(ClearOptions.Target, Color.White, 1.0f, 0);

                return disabledShadowOcclusion;
            }
        }
                
        /// <summary>
        /// Renders the shadow map.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="splitIndex">Index of the split.</param>
        protected void DrawShadowMap(GraphicsDevice device, SceneManager scene)
        {
            device.SetRenderTarget(shadowMap);
            device.Clear(ClearOptions.Target, new Vector4(1.0f), 1.0f, 0);
            device.Clear(ClearOptions.DepthBuffer, new Vector4(1.0f), 1.0f, 0);

            // Set up the effect
            shadowEffect.CurrentTechnique = shadowOcclusionTechniques["GenerateShadowMap"];
            shadowEffect.Parameters["g_matViewProj"].SetValue(lightCamera.ViewProjectionMatrix);
            
            // Draw the models
            scene.Draw(device, shadowEffect);
        }

        /// <summary>
        /// Renders the shadow occlusion.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="depthTexture">The depth texture.</param>
        protected void DrawShadowOcclusion(GraphicsDevice device, ICamera camera, RenderTarget2D depthTexture)
        {
            // Set the device to render the shadow occlusion texture
            device.SetRenderTarget(shadowOcclusion);

            Matrix cameraTransform;
            camera.GetWorldMatrix(out cameraTransform);
            
            // Setup the shadow effect
            shadowEffect.CurrentTechnique = shadowOcclusionTechniques[(int)filteringType];
            shadowEffect.Parameters["g_matInvView"].SetValue(cameraTransform);
            shadowEffect.Parameters["g_matLightViewProj"].SetValue(lightCamera.ViewProjectionMatrix);
            shadowEffect.Parameters["g_vFrustumCornersVS"].SetValue(farFrustumCornerVS);
            shadowEffect.Parameters["ShadowMap"].SetValue(shadowMap);
            shadowEffect.Parameters["DepthTexture"].SetValue(depthTexture);
            shadowEffect.Parameters["g_vOcclusionTextureSize"].SetValue(new Vector2(shadowOcclusion.Width, shadowOcclusion.Height));
            shadowEffect.Parameters["g_vShadowMapSize"].SetValue(new Vector2(SHADOWMAPSIZE, SHADOWMAPSIZE));

            // Begin the effect
            shadowEffect.CurrentTechnique.Passes[0].Apply();

            // Draw the FullscreenQuad
            fullscreenQuad.Draw();
        }
        #endregion
    }
}
