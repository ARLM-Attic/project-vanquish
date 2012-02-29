using System.Linq;
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
        /// Camera Manager
        /// </summary>
        CameraManager cameraManager;

        /// <summary>
        /// Physics Manager
        /// </summary>
        static PhysicsManager physicsManager;

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
            cameraManager = new CameraManager("Default", 
                                              new FirstPersonCamera(MathHelper.PiOver4, 
                                                        device.PresentationParameters.BackBufferWidth / device.PresentationParameters.BackBufferHeight, 
                                                        1.0f, 500f));
            physicsManager = new PhysicsManager(new Vector3(0, 0, 0));
            models = new List<Actor>();
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
        /// Draws all models in the List.
        /// </summary>
        public void Draw()
        {
            device.DepthStencilState = DepthStencilState.Default;
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.BlendState = BlendState.Opaque;

            foreach (Actor actor in models.Where(a => a.BoundingSphere.Intersects(CameraManager.GetActiveCamera().BoundingFrustum)))
                actor.Draw();
        }

        /// <summary>
        /// Draws all models using a specified Effect.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="effect">The effect.</param>
        public void Draw(GraphicsDevice device, Effect effect)
        {
            foreach (Actor actor in models)
                actor.DrawWithEffect(device, effect);
        }
        
        /// <summary>
        /// Updates the Scene Manager.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {
            CameraManager.GetActiveCamera().Update();
            physicsManager.Update(gameTime);
        } 
        #endregion
    }
}
