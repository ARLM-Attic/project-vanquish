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
        /// Intensity of the light
        /// </summary>
        protected float intensity = 0.0f;

        /// <summary>
        /// Range of the light
        /// </summary>
        protected float range = 0.0f; 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PointLight"/> class.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="color">The color.</param>
        /// <param name="range">The range.</param>
        /// <param name="intensity">The intensity.</param>
        public PointLight(Vector3 position, Vector3 color, float range, float intensity)
            : base()
        {
            Position = position;
            Color = color;
            Range = range;
            Intensity = intensity;
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        public float Intensity
        {
            get { return intensity; }
            set { intensity = value;}
        }

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
