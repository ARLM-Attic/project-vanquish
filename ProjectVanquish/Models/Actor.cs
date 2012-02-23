using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectVanquish.Models.Interfaces;

namespace ProjectVanquish.Models
{
    public class Actor : IEntity
    {
        #region Fields
        Model model;
        Vector3 position, rotation, scale;
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
        /// Gets the bounding box.
        /// </summary>
        /// <value>The bounding box.</value>
        public BoundingBox BoundingBox 
        { 
            get { return new BoundingBox(Position - Scale / 2f, Position + Scale / 2f); }
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        public Model Model 
        { 
            get { return model; } 
            set { model = value; } 
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public Vector3 Position 
        { 
            get { return position; } 
            set { position = value; } 
        }

        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        /// <value>The rotation.</value>
        public Vector3 Rotation 
        { 
            get { return rotation; } 
            set { rotation = value; } 
        }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>The scale.</value>
        public Vector3 Scale 
        { 
            get { return scale; } 
            set { scale = value; } 
        }

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
