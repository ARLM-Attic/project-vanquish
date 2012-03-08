using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectVanquish.Cameras;
using ProjectVanquish.Models;

namespace ProjectVanquish.Core
{
    /// <summary>
    /// Scene Manager
    /// </summary>
    public class SceneManager
    {
        #region Fields
        private Game game;
        static IList<Actor> models = null;
        private Camera camera;

        /// <summary>
        /// Physics Manager
        /// </summary>
        static PhysicsManager physicsManager;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneManager"/> class.
        /// </summary>
        /// <param name="game">The game.</param>
        public SceneManager(Game game)
        {
            this.game = game;
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the models.
        /// </summary>
        /// <value>The models.</value>
        public IList<Actor> Models { get { return models; } }

        /// <summary>
        /// Gets the physics manager.
        /// </summary>
        /// <value>The physics manager.</value>
        public static PhysicsManager PhysicsManager { get { return physicsManager; } } 
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
        /// Draws the scene.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void DrawScene(GameTime gameTime)
        {
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            game.GraphicsDevice.BlendState = BlendState.Opaque;

            foreach (Actor model in models)
                model.Draw(camera);
        }

        /// <summary>
        /// Draws the scene with custom effect.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="effect">The effect.</param>
        public void DrawSceneWithCustomEffect(GraphicsDevice device, Effect effect)
        {
            foreach (Actor model in models)
                model.DrawWithEffect(device, effect);
        }

        /// <summary>
        /// Draws the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="world">The world.</param>
        private void DrawModel(Model model, Matrix world)
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
        /// Initializes the scene.
        /// </summary>
        /// <param name="camera">The camera.</param>
        public void InitializeScene(Camera camera)
        {
            models = new List<Actor>();
            physicsManager = new PhysicsManager(new Vector3(0, 0, 0));
            this.camera = camera;
        } 
        #endregion
    }
}
