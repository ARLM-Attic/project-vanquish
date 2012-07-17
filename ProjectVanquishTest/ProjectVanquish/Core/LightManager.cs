using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectVanquish.Renderers;
using ProjectVanquish.Cameras;

namespace ProjectVanquish.Core
{
    /// <summary>
    /// LightManager
    /// </summary>
    public class LightManager
    {
        #region Fields
        Effect directionalLightEffect, hemisphericLightEffect, pointLightEffect;
        Model sphereModel;
        QuadRenderer fullscreenQuad;
        Vector2 halfPixel;
        Texture2D hemisphericColorMap;
        static Lights.DirectionalLight light;
        static IList<Lights.PointLight> pointLights;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="LightManager"/> class.
        /// </summary>
        /// <param name="game">The game.</param>
        public LightManager(Game game)
        {
            // Load Effects, Models and Quad Renderer
            directionalLightEffect = game.Content.Load<Effect>("Shaders/Lights/DirectionalLight");
            pointLightEffect = game.Content.Load<Effect>("Shaders/Lights/PointLight");
            hemisphericLightEffect = game.Content.Load<Effect>("Shaders/Lights/HemisphericLight");
            sphereModel = game.Content.Load<Model>("Models/Sphere");
            fullscreenQuad = new QuadRenderer(game);
            game.Components.Add(fullscreenQuad);
            halfPixel = new Vector2()
            {
                X = 0.5f / (float)game.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Y = 0.5f / (float)game.GraphicsDevice.PresentationParameters.BackBufferHeight
            };

            // Load the Color Map for the Hemispheric Light
            hemisphericColorMap = game.Content.Load<Texture2D>("Textures/ColorMap");

            // Make our directional light source
            light = new Lights.DirectionalLight();
            light.Direction = new Vector3(-1, -1, -1);
            light.Color = new Vector3(0.7f, 0.7f, 0.7f);

            // Instantiate the PointLights List
            pointLights = new List<Lights.PointLight>();
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the light.
        /// </summary>
        /// <value>The light.</value>
        public static Lights.DirectionalLight Light { get { return light; } }

        /// <summary>
        /// Gets or sets a value indicating whether [use hemispheric light].
        /// </summary>
        /// <value><c>true</c> if [use hemispheric light]; otherwise, <c>false</c>.</value>
        public static bool UseHemisphericLight { get; set; }
        #endregion

        #region Members
        /// <summary>
        /// Adds the point light.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="color">The color.</param>
        /// <param name="range">The range.</param>
        /// <param name="intensity">The intensity.</param>
        public static void AddPointLight(Vector3 position, Vector3 color, float range, float intensity)
        {
            pointLights.Add(new Lights.PointLight(position, color, range, intensity));
        }

        /// <summary>
        /// Adds the point light.
        /// </summary>
        /// <param name="pointLight">The point light.</param>
        public static void AddPointLight(Lights.PointLight pointLight)
        {
            pointLights.Add(pointLight);
        }

        /// <summary>
        /// Draws a directional light.
        /// </summary>
        /// <param name="lightDirection">The light direction.</param>
        /// <param name="color">The color.</param>
        void DrawDirectionalLight(RenderTarget2D colorRT, RenderTarget2D normalRT, RenderTarget2D depthRT, Camera camera)
        {
            // Set all parameters
            directionalLightEffect.Parameters["colorMap"].SetValue(colorRT);
            directionalLightEffect.Parameters["normalMap"].SetValue(normalRT);
            directionalLightEffect.Parameters["depthMap"].SetValue(depthRT);
            directionalLightEffect.Parameters["lightDirection"].SetValue(light.Direction);
            directionalLightEffect.Parameters["Color"].SetValue(light.Color);
            directionalLightEffect.Parameters["cameraPosition"].SetValue(camera.Position);
            directionalLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(camera.ViewMatrix * camera.ProjectionMatrix));
            directionalLightEffect.Parameters["halfPixel"].SetValue(halfPixel);

            // Apply the Effect
            directionalLightEffect.Techniques[0].Passes[0].Apply();

            // Draw a FullscreenQuad
            fullscreenQuad.Render(Vector2.One * -1, Vector2.One);
        }

        /// <summary>
        /// Draws the hemispheric light.
        /// </summary>
        void DrawHemisphericLight(GraphicsDevice device, SceneManager scene, Camera camera)
        {
            device.BlendState = BlendState.Opaque;

            // Only apply the effect to those models in the Frustum
            foreach (Models.Actor actor in SceneManager.Models.Where(a => camera.BoundingFrustum.Intersects(a.BoundingSphere)))
            {
                foreach (ModelMesh mesh in actor.Model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        // Set the Effect Parameters
                        hemisphericLightEffect.Parameters["matWorldViewProj"].SetValue(actor.World * camera.ViewMatrix * camera.ProjectionMatrix);
                        hemisphericLightEffect.Parameters["matInverseWorld"].SetValue(actor.World);
                        hemisphericLightEffect.Parameters["vLightDirection"].SetValue(new Vector4(light.Direction, 1));
                        hemisphericLightEffect.Parameters["ColorMap"].SetValue(hemisphericColorMap);
                        hemisphericLightEffect.Parameters["LightIntensity"].SetValue(0.3f);
                        hemisphericLightEffect.Parameters["SkyColor"].SetValue(new Vector4(light.Color, 1));

                        // Apply the Effect
                        hemisphericLightEffect.Techniques[0].Passes[0].Apply();

                        // Render the Primitives
                        device.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);
                        device.Indices = part.IndexBuffer;
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }

            device.BlendState = BlendState.AlphaBlend;
        }

        /// <summary>
        /// Draws the lights.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="colorRT">The color RT.</param>
        /// <param name="normalRT">The normal RT.</param>
        /// <param name="depthRT">The depth RT.</param>
        /// <param name="lightRT">The light RT.</param>
        /// <param name="shadowOcclusion">The shadow occlusion.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="scene">The scene.</param>
        public void DrawLights(GraphicsDevice device, RenderTarget2D colorRT, RenderTarget2D normalRT, RenderTarget2D depthRT, RenderTarget2D lightRT, Camera camera, SceneManager scene)
        {
            // Set the Light RenderTarget
            device.SetRenderTarget(lightRT);

            // Clear all components to 0
            device.Clear(Color.Transparent);
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.None;

            // Render either the Directional or Hemispheric light
            if (UseHemisphericLight)
                DrawHemisphericLight(device, scene, camera);
            else
                DrawDirectionalLight(colorRT, normalRT, depthRT, camera);

            // Render each PointLight
            foreach (Lights.PointLight pointLight in pointLights)
                DrawPointLight(device, colorRT, normalRT, depthRT, camera, pointLight);

            // Reset RenderStates
            device.BlendState = BlendState.Opaque;
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;

            // Reset the RenderTarget
            device.SetRenderTarget(null);
        }

        /// <summary>
        /// Draws a point light.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="colorRT">The color RT.</param>
        /// <param name="normalRT">The normal RT.</param>
        /// <param name="depthRT">The depth RT.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="pointLight">The point light.</param>
        void DrawPointLight(GraphicsDevice device, RenderTarget2D colorRT, RenderTarget2D normalRT, RenderTarget2D depthRT, Camera camera, Lights.PointLight pointLight)
        {
            // Set the G-Buffer parameters
            pointLightEffect.Parameters["colorMap"].SetValue(colorRT);
            pointLightEffect.Parameters["normalMap"].SetValue(normalRT);
            pointLightEffect.Parameters["depthMap"].SetValue(depthRT);

            // Compute the light world matrix
            // scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(pointLight.Range) * Matrix.CreateTranslation(pointLight.Position);
            pointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            pointLightEffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);

            // Light position
            pointLightEffect.Parameters["lightPosition"].SetValue(pointLight.Position);

            // Set the color, radius and Intensity
            pointLightEffect.Parameters["Color"].SetValue(pointLight.Color);
            pointLightEffect.Parameters["lightRadius"].SetValue(pointLight.Range);
            pointLightEffect.Parameters["lightIntensity"].SetValue(pointLight.Intensity);

            // Parameters for specular computations
            pointLightEffect.Parameters["cameraPosition"].SetValue(camera.Position);
            pointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(camera.ViewMatrix * camera.ProjectionMatrix));

            // Size of a halfpixel, for texture coordinates alignment
            pointLightEffect.Parameters["halfPixel"].SetValue(halfPixel);

            // Calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(camera.Position, pointLight.Position);

            // If we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < pointLight.Range)
                device.RasterizerState = RasterizerState.CullClockwise;
            else
                device.RasterizerState = RasterizerState.CullCounterClockwise;

            device.DepthStencilState = DepthStencilState.None;

            // Apply the Effect
            pointLightEffect.Techniques[0].Passes[0].Apply();

            // Draw the Sphere mesh
            foreach (ModelMesh mesh in sphereModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    device.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
                    device.Indices = meshPart.IndexBuffer;
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
        } 
        #endregion
    }
}
