using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectVanquish.Models
{
    public class Actor
    {
        #region Fields
        Model model;
        Vector3 position, rotation = Vector3.Zero, scale = Vector3.One; 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public Actor(Model model)
        {
            this.model = model;
            position = Vector3.Zero;
            rotation = Vector3.Zero;
            scale = Vector3.One;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="position">The position.</param>
        public Actor(Model model, Vector3 position)
        {
            this.model = model;
            this.position = position;
            rotation = Vector3.Zero;
            scale = Vector3.One;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        public Actor(Model model, Vector3 position, Vector3 rotation)
        {
            this.model = model;
            this.position = position;
            this.rotation = rotation;
            scale = Vector3.One;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="scale">The scale.</param>
        public Actor(Model model, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.model = model;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        public Model Model { get { return model; } }

        /// <summary>
        /// Gets the world.
        /// </summary>
        /// <value>The world.</value>
        public Matrix World
        {
            get
            {
                return Matrix.CreateTranslation(position) * Matrix.CreateScale(scale)
                    * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationY(rotation.Y)
                    * Matrix.CreateRotationZ(rotation.Z);
            }
        } 
        #endregion
    }
}
