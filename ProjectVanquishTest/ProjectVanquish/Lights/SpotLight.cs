using Microsoft.Xna.Framework;

namespace ProjectVanquish.Lights
{
    /// <summary>
    /// A spotlight
    /// </summary>
    public class SpotLight : PointLight
    {
        #region Fields
        protected float innerCone;
        protected float outerCone; 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SpotLight"/> class.
        /// </summary>
        public SpotLight(Vector3 position, Vector3 color, float range, float intensity)
            : base(position, color, range, intensity)
        {
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the direction of the light
        /// </summary>
        public Vector3 Direction
        {
            get { return worldMatrix.Forward; }
            set
            {
                Vector3 position = worldMatrix.Translation;
                Vector3 forward = value;
                Vector3 up = Vector3.Up;
                if (forward == Vector3.Up || forward == Vector3.Down)
                {
                    Vector3 right = Vector3.Right;
                    Vector3.Cross(ref right, ref forward, out up);
                }
                Matrix.CreateWorld(ref position, ref forward, ref up, out worldMatrix);
            }
        }

        /// <summary>
        /// Gets or sets the width of the inner cone, in radians
        /// </summary>
        public float InnerCone
        {
            get { return innerCone; }
            set { innerCone = value; }
        }

        /// <summary>
        /// Gets or sets the width of the outer cone, in radians
        /// </summary>
        public float OuterCone
        {
            get { return outerCone; }
            set { outerCone = value; }
        } 
        #endregion
    }
}
