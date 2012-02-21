using Microsoft.Xna.Framework;

namespace ProjectVanquish.Core.Lights
{
    public abstract class Light
    {
        #region Fields
        /// <summary>
        /// Color
        /// </summary>
        protected Vector3 color = Vector3.Zero;

        /// <summary>
        /// World Matrix
        /// </summary>
        protected Matrix worldMatrix = Matrix.Identity;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Light"/> class.
        /// </summary>
        public Light()
        {
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        public Vector3 Color
        {
            get { return this.color; }
            set { this.color = value; }
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public Vector3 Position
        {
            get { return this.worldMatrix.Translation; }
            set { this.worldMatrix.Translation = value; }
        }

        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        /// <value>The rotation.</value>
        public Quaternion Rotation
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>The scale.</value>
        public Vector3 Scale
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }
        #endregion

        #region Members
        /// <summary>
        /// Gets the world matrix.
        /// </summary>
        /// <param name="worldMatrix">The world matrix.</param>
        public void GetWorldMatrix(out Matrix worldMatrix)
        {
            worldMatrix = this.worldMatrix;
        }

        /// <summary>
        /// Sets the world matrix.
        /// </summary>
        /// <param name="worldMatrix">The world matrix.</param>
        public void SetWorldMatrix(ref Matrix worldMatrix)
        {
            this.worldMatrix = worldMatrix;
        }
        #endregion
    }
}
