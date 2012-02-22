using System;
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
    public enum ShadowFilteringType1
    {
        PCF2x2 = 1,
        PCF3x3 = 2,
        PCF5x5 = 3,
        PCF7x7 = 4
    }

    class CascadeShadowRenderer
    {
        #region Constants
        /// <summary>
        /// Shadow Map size
        /// </summary>
        public const int SHADOWMAPSIZE = 1024;

        /// <summary>
        /// Number of splits
        /// </summary>
        public const int NUMBER_OF_SPLITS = 4;
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
        Vector3[] splitFrustumCornersVS = new Vector3[8];

        /// <summary>
        /// Light matrices
        /// </summary>
        Matrix[] lightViewProjectionMatrices = new Matrix[NUMBER_OF_SPLITS];
        Vector2[] lightClipPlanes = new Vector2[NUMBER_OF_SPLITS];

        /// <summary>
        /// Split Depths
        /// </summary>
        float[] splitDepths = new float[NUMBER_OF_SPLITS + 1];

        /// <summary>
        /// Light Cameras
        /// </summary>
        OrthographicCamera[] lightCameras = new OrthographicCamera[NUMBER_OF_SPLITS];

        /// <summary>
        /// Shadow Effect
        /// </summary>
        Effect shadowEffect;

        /// <summary>
        /// Shadow Filtering Type - defaulted to 2x2
        /// </summary>
        ShadowFilteringType filteringType = ShadowFilteringType.PCF5x5;

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
        public CascadeShadowRenderer(GraphicsDevice device, ContentManager contentManager)
        {
            // Load the Shadow Effect
            shadowEffect = contentManager.Load<Effect>("Shaders/Shadows/CascadeShadowMap");

            // Instantiate the SpriteBatch
            spriteBatch = new SpriteBatch(device);

            // Create the ShadowMap RenderTarget
            shadowMap = new RenderTarget2D(device, SHADOWMAPSIZE * NUMBER_OF_SPLITS, SHADOWMAPSIZE,
                                                false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
            
            // Create the Shadow Occlusion RenderTarget
            shadowOcclusion = new RenderTarget2D(device, device.PresentationParameters.BackBufferWidth,
                                                      device.PresentationParameters.BackBufferHeight,
                                                      false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

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
            shadowOcclusionTechniques = this.shadowEffect.Techniques;

            // Create a default Directional light
            light = new ProjectVanquish.Core.Lights.DirectionalLight();
            light.Direction = new Vector3(-1, -1, -1);
            light.Color = new Vector3(0.7f, 0.7f, 0.7f);
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
        protected OrthographicCamera CalculateFrustum(ProjectVanquish.Core.Lights.DirectionalLight light, float minZ, float maxZ)
        {
            // Shorten the view frustum according to the shadow view distance
            Matrix cameraMatrix;
            CameraManager.GetActiveCamera().GetWorldMatrix(out cameraMatrix);

            for (int i = 0; i < 4; i++)
                splitFrustumCornersVS[i] = frustumCornersVS[i + 4] * (minZ / CameraManager.GetActiveCamera().FarClip);

            for (int i = 4; i < 8; i++)
                splitFrustumCornersVS[i] = frustumCornersVS[i] * (maxZ / CameraManager.GetActiveCamera().FarClip);

            Vector3.Transform(splitFrustumCornersVS, ref cameraMatrix, frustumCornersWS);

            // Position the shadow-caster camera so that it's looking at the centroid,
            // and backed up in the direction of the sunlight
            Matrix viewMatrix = Matrix.CreateLookAt(Vector3.Zero - (light.Direction * 100), Vector3.Zero, new Vector3(0, 1, 0));

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


            // We snap the camera to 1 pixel increments so that moving the camera does not cause the shadows to jitter.
            // This is a matter of integer dividing by the world space size of a texel
            float diagonalLength = (frustumCornersWS[0] - frustumCornersWS[6]).Length();
            diagonalLength += 2;    //Without this, the shadow map isn't big enough in the world.
            float worldsUnitsPerTexel = diagonalLength / (float)SHADOWMAPSIZE;
            Vector3 vBorderOffset = (new Vector3(diagonalLength, diagonalLength, diagonalLength) - (maxes - mins)) * 0.5f;

            maxes += vBorderOffset;
            mins -= vBorderOffset;

            mins /= worldsUnitsPerTexel;
            mins.X = (float)Math.Floor(mins.X);
            mins.Y = (float)Math.Floor(mins.Y);
            mins.Z = (float)Math.Floor(mins.Z);
            mins *= worldsUnitsPerTexel;

            maxes /= worldsUnitsPerTexel;
            maxes.X = (float)Math.Floor(maxes.X);
            maxes.Y = (float)Math.Floor(maxes.Y);
            maxes.Z = (float)Math.Floor(maxes.Z);
            maxes *= worldsUnitsPerTexel;

            // Create an orthographic camera for use as a shadow caster
            const float nearClipOffset = 100.0f;
            OrthographicCamera lightCamera = new OrthographicCamera(mins.X, maxes.X, mins.Y, maxes.Y, -maxes.Z - nearClipOffset, -mins.Z);
            lightCamera.SetViewMatrix(ref viewMatrix);

            return lightCamera;
        }
        
        /// <summary>
        /// Renders the specified device.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="camera">The camera.</param>
        public RenderTarget2D Draw(GraphicsDevice device, RenderTarget2D depthRT, SceneManager scene)
        {            
            // Create the Shadow Occlusion
            RenderTarget2D shadowOcclusion = Draw(device, depthRT, scene, light);

            return shadowOcclusion;
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

                // Calculate the cascade splits.  We calculate these so that each successive
                // split is larger than the previous, giving the closest split the most amount
                // of shadow detail.  
                float N = NUMBER_OF_SPLITS;
                float near = CameraManager.GetActiveCamera().NearClip, far = CameraManager.GetActiveCamera().FarClip;
                splitDepths[0] = near;
                splitDepths[NUMBER_OF_SPLITS] = far;
                const float splitConstant = 0.95f;

                for (int i = 1; i < splitDepths.Length - 1; i++)
                    splitDepths[i] = splitConstant * near * (float)Math.Pow(far / near, i / N) + (1.0f - splitConstant) * ((near + (i / N)) * (far - near));

                // Render our scene geometry to each split of the cascade
                for (int i = 0; i < NUMBER_OF_SPLITS; i++)
                {
                    float minZ = splitDepths[i];
                    float maxZ = splitDepths[i + 1];
                    lightCameras[i] = CalculateFrustum(light, minZ, maxZ);
                    DrawShadowMap(device, scene, i);
                }

                // Render the shadow occlusion
                DrawShadowOcclusion(device, depthRT);

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
        protected void DrawShadowMap(GraphicsDevice device, SceneManager scene, int splitIndex)
        {
            device.SetRenderTarget(shadowMap);
            device.Clear(ClearOptions.Target, Color.White, 1.0f, 0);
            device.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            // Set the Viewport
            Viewport splitViewport = new Viewport();
            splitViewport.MinDepth = 0;
            splitViewport.MaxDepth = 1;
            splitViewport.Width = SHADOWMAPSIZE;
            splitViewport.Height = SHADOWMAPSIZE;
            splitViewport.X = splitIndex * SHADOWMAPSIZE;
            splitViewport.Y = 0;
            device.Viewport = splitViewport;

            // Set up the effect
            shadowEffect.CurrentTechnique = shadowOcclusionTechniques["GenerateShadowMap"];
            shadowEffect.Parameters["g_matViewProj"].SetValue(lightCameras[splitIndex].ViewProjectionMatrix);

            // Draw the models
            scene.Draw(device, shadowEffect);
        }

        /// <summary>
        /// Renders the shadow occlusion.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="depthTexture">The depth texture.</param>
        protected void DrawShadowOcclusion(GraphicsDevice device, RenderTarget2D depthTexture)
        {
            // Set the device to render the shadow occlusion texture
            device.SetRenderTarget(shadowOcclusion);

            Matrix cameraTransform;
            CameraManager.GetActiveCamera().GetWorldMatrix(out cameraTransform);

            // Determine which split a pixel belongs too
            for (int i = 0; i < NUMBER_OF_SPLITS; i++)
            {
                lightClipPlanes[i].X = -splitDepths[i];
                lightClipPlanes[i].Y = -splitDepths[i + 1];
                lightCameras[i].GetViewProjectionMatrix(out lightViewProjectionMatrices[i]);
            }

            // Setup the shadow effect
            shadowEffect.CurrentTechnique = shadowOcclusionTechniques[(int)filteringType];
            shadowEffect.Parameters["g_matInvView"].SetValue(cameraTransform);
            shadowEffect.Parameters["g_matLightViewProj"].SetValue(lightViewProjectionMatrices);
            shadowEffect.Parameters["g_vFrustumCornersVS"].SetValue(farFrustumCornerVS);
            shadowEffect.Parameters["g_vClipPlanes"].SetValue(lightClipPlanes);
            shadowEffect.Parameters["ShadowMap"].SetValue(shadowMap);
            shadowEffect.Parameters["DepthTexture"].SetValue(depthTexture);
            shadowEffect.Parameters["g_vOcclusionTextureSize"].SetValue(new Vector2(shadowOcclusion.Width, shadowOcclusion.Height));
            shadowEffect.Parameters["g_vShadowMapSize"].SetValue(new Vector2(shadowMap.Width, shadowMap.Height));
            shadowEffect.Parameters["g_bShowSplitColors"].SetValue(false);

            // Begin the effect
            shadowEffect.CurrentTechnique.Passes[0].Apply();

            // Draw the FullscreenQuad
            fullscreenQuad.Draw();
        }
        #endregion
    }
}
