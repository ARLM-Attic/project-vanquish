using Microsoft.Xna.Framework;

namespace ProjectVanquish.Cameras
{
    public class PerspectiveCamera : BaseCamera
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PerspectiveCamera"/> class.
        /// </summary>
        /// <param name="fieldOfView">The field of view.</param>
        /// <param name="aspectRatio">The aspect ratio.</param>
        /// <param name="nearClip">The near clip.</param>
        /// <param name="farClip">The far clip.</param>
        public PerspectiveCamera(float fieldOfView, float aspectRatio, float nearClip, float farClip)
            : base()
        {
            this.fieldOfView = fieldOfView;
            this.aspectRatio = aspectRatio;
            this.nearClip = nearClip;
            this.farClip = farClip;
            Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClip, farClip, out projectionMatrix);
            Update();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the aspect ratio.
        /// </summary>
        /// <value>The aspect ratio.</value>
        public override float AspectRatio
        {
            get { return aspectRatio; }
            set
            {
                aspectRatio = value;
                Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClip, farClip, out projectionMatrix);
                Update();
            }
        }

        /// <summary>
        /// Gets or sets the far clip.
        /// </summary>
        /// <value>The far clip.</value>
        public override float FarClip
        {
            get { return farClip; }
            set
            {
                farClip = value;
                Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClip, farClip, out projectionMatrix);
                Update();
            }
        }

        /// <summary>
        /// Gets or sets the field of view.
        /// </summary>
        /// <value>The field of view.</value>
        public override float FieldOfView
        {
            get { return fieldOfView; }
            set
            {
                fieldOfView = value;
                Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClip, farClip, out projectionMatrix);
                Update();
            }
        }

        /// <summary>
        /// Gets or sets the near clip.
        /// </summary>
        /// <value>The near clip.</value>
        public override float NearClip
        {
            get { return nearClip; }
            set
            {
                nearClip = value;
                Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClip, farClip, out projectionMatrix);
                Update();
            }
        }
        #endregion
    }
}
