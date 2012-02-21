using Microsoft.Xna.Framework;

namespace ProjectVanquish.Core.Lights
{
    public class DirectionalLight : Light
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectionalLight"/> class.
        /// </summary>
        public DirectionalLight()
            : base()
        {
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>The direction.</value>
        public Vector3 Direction
        {
            get { return this.worldMatrix.Forward; }
            set
            {
                Vector3 position = this.worldMatrix.Translation;
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
        #endregion
    }
}
