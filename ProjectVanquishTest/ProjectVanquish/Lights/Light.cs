using Microsoft.Xna.Framework;

namespace ProjectVanquish.Lights
{
    /// <summary>
    /// Abstract base class for all lights
    /// </summary>
    public abstract class Light
    {
        #region Fields
        protected Vector3 color = Vector3.Zero;
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
        /// Gets or sets the color of the light
        /// </summary>
        public Vector3 Color
        {
            get { return color; }
            set { color = value; }
        } 
        #endregion

        #region Members
        /// <summary>
        /// Gets the world matrix for the light
        /// </summary>
        /// <param name="worldMatrix">Receives the world matrix</param>
        public void GetWorldMatrix(out Matrix worldMatrix)
        {
            worldMatrix = this.worldMatrix;
        }

        /// <summary>
        /// Sets the world matrix for the light
        /// </summary>
        /// <param name="worldMatrix">The matrix to use as the world matrix</param>
        public void SetWorldMatrix(ref Matrix worldMatrix)
        {
            this.worldMatrix = worldMatrix;
        } 
        #endregion
    }
}
