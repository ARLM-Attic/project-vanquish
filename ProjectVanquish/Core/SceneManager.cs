using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectVanquish.Cameras;
using ProjectVanquish.Models;

namespace ProjectVanquish.Core
{
    public class SceneManager
    {
        #region Fields
        /// <summary>
        /// Content Manager
        /// </summary>
        ContentManager content;

        /// <summary>
        /// Graphics Device
        /// </summary>
        GraphicsDevice device;

        /// <summary>
        /// First Person Camera
        /// </summary>
        FirstPersonCamera camera;

        /// <summary>
        /// List of Models
        /// </summary>
        static IList<Actor> models; 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneManager"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="content">The content.</param>
        public SceneManager(GraphicsDevice device, ContentManager content)
        {
            this.content = content;
            this.device = device;
            camera = new FirstPersonCamera(MathHelper.PiOver4, (float)device.PresentationParameters.BackBufferWidth / (float)device.PresentationParameters.BackBufferHeight, 1.0f, 500f);
            camera.Position = new Vector3(0, 5, 10);
            models = new List<Actor>();
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the camera.
        /// </summary>
        /// <value>The camera.</value>
        public FirstPersonCamera Camera { get { return camera; } }

        /// <summary>
        /// Gets the models.
        /// </summary>
        /// <value>The models.</value>
        public IList<Actor> Models { get { return models; } } 
        #endregion

        #region Members
        /// <summary>
        /// Adds a model.
        /// </summary>
        /// <param name="model">The model.</param>
        public static void AddModel(Model model)
        {
            models.Add(new Actor(model));
        }

        /// <summary>
        /// Adds the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="position">The position.</param>
        public static void AddModel(Model model, Vector3 position)
        {
            models.Add(new Actor(model, position));
        }

        /// <summary>
        /// Adds the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        public static void AddModel(Model model, Vector3 position, Vector3 rotation)
        {
            models.Add(new Actor(model, position, rotation));
        }

        /// <summary>
        /// Adds the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="scale">The scale.</param>
        public static void AddModel(Model model, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            models.Add(new Actor(model, position, rotation, scale));
        }

        /// <summary>
        /// Draws all models in the List.
        /// </summary>
        public void Draw()
        {
            device.DepthStencilState = DepthStencilState.Default;
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.BlendState = BlendState.Opaque;

            foreach (Actor actor in models)
                DrawModel(actor.Model, actor.World, camera);
        }

        /// <summary>
        /// Draws the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="world">The world.</param>
        /// <param name="camera">The camera.</param>
        void DrawModel(Model model, Matrix world, FirstPersonCamera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                }

                mesh.Draw();
            }
        }

        /// <summary>
        /// Draws all models using a specified Effect.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="effect">The effect.</param>
        public void Draw(GraphicsDevice device, Effect effect)
        {
            for (int i = 0; i < models.Count; i++)
            {
                for (int j = 0; j < models[i].Model.Meshes.Count; j++)
                {
                    ModelMesh mesh = models[i].Model.Meshes[j];
                    Matrix world = models[i].World;

                    effect.Parameters["g_matWorld"].SetValue(world);
                    Matrix transpose, inverseTranspose;
                    Matrix.Transpose(ref world, out transpose);
                    Matrix.Invert(ref transpose, out inverseTranspose);
                    effect.Parameters["g_matWorldIT"].SetValue(inverseTranspose);

                    effect.CurrentTechnique.Passes[0].Apply();

                    for (int k = 0; k < mesh.MeshParts.Count; k++)
                    {
                        ModelMeshPart part = mesh.MeshParts[k];
                        device.SetVertexBuffer(part.VertexBuffer);
                        device.Indices = part.IndexBuffer;
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
                }

                device.SetVertexBuffer(null);
                device.Indices = null;
            }
        }

        /// <summary>
        /// Updates the Scene Manager.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {
            camera.Update();
        } 
        #endregion
    }
}
