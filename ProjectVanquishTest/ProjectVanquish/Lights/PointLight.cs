using Microsoft.Xna.Framework;

namespace ProjectVanquish.Lights
{
    /// <summary>
    /// A point(omni) light source
    /// </summary>
    public class PointLight : Light
    {
        #region Fields
        /// <summary>
        /// Range of the light
        /// </summary>
        protected float range = 0.0f; 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PointLight"/> class.
        /// </summary>
        public PointLight()
            : base()
        {
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the position of the light
        /// </summary>
        public Vector3 Position
        {
            get { return worldMatrix.Translation; }
            set { worldMatrix.Translation = value; }
        }

        /// <summary>
        /// Gets or sets the range of the light
        /// </summary>
        public float Range
        {
            get { return range; }
            set { range = value; }
        } 
        #endregion        
    }
}
