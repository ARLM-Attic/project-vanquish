using Microsoft.Xna.Framework;

namespace ProjectVanquish.Cameras
{
    public abstract class BaseCamera : ICamera
    {
        #region Fields
        /// <summary>
        /// Projection Matrix
        /// </summary>
        protected Matrix projectionMatrix = Matrix.Identity;

        /// <summary>
        /// View Matrix
        /// </summary>
        protected Matrix viewMatrix = Matrix.Identity;

        /// <summary>
        /// View Projection Matrix
        /// </summary>
        protected Matrix viewProjectionMatrix = Matrix.Identity;

        /// <summary>
        /// World Matrix
        /// </summary>
        protected Matrix worldMatrix = Matrix.Identity;

        /// <summary>
        /// Aspect Ratio
        /// </summary>
        protected float aspectRatio;

        /// <summary>
        /// Field Of View
        /// </summary>
        protected float fieldOfView;

        /// <summary>
        /// BoundingFrustum
        /// </summary>
        protected BoundingFrustum boundingFrustum;

        /// <summary>
        /// Near Clip
        /// </summary>
        protected float nearClip;

        /// <summary>
        /// Far Clip
        /// </summary>
        protected float farClip;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCamera"/> class.
        /// </summary>
        public BaseCamera()
        {
            boundingFrustum = new BoundingFrustum(viewProjectionMatrix);
            viewMatrix = Matrix.Identity;
            worldMatrix = Matrix.Identity;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the aspect ratio.
        /// </summary>
        /// <value>The aspect ratio.</value>
        public virtual float AspectRatio
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
        /// Gets the bounding frustum.
        /// </summary>
        /// <value>The bounding frustum.</value>
        public BoundingFrustum BoundingFrustum
        {
            get { return boundingFrustum; }
        }

        /// <summary>
        /// Gets or sets the far clip.
        /// </summary>
        /// <value>The far clip.</value>
        public virtual float FarClip
        {
            get { return nearClip; }
            set { }
        }

        /// <summary>
        /// Gets or sets the field of view.
        /// </summary>
        /// <value>The field of view.</value>
        public virtual float FieldOfView
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
        public virtual float NearClip
        {
            get { return nearClip; }
            set { }
        }

        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        /// <value>The orientation.</value>
        public Quaternion Orientation
        {
            get
            {
                Quaternion orientation;
                Quaternion.CreateFromRotationMatrix(ref worldMatrix, out orientation);
                return orientation;
            }
            set
            {
                Quaternion orientation = value;
                Vector3 position = worldMatrix.Translation;
                Matrix.CreateFromQuaternion(ref orientation, out worldMatrix);
                worldMatrix.Translation = position;
                Update();
            }
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public Vector3 Position
        {
            get { return worldMatrix.Translation; }
            set
            {
                worldMatrix.Translation = value;
                Update();
            }
        }

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        /// <value>The projection matrix.</value>
        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
            set { projectionMatrix = value; }
        }
        
        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        /// <value>The view matrix.</value>
        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
            set
            {
                viewMatrix = value;
                Matrix.Invert(ref viewMatrix, out worldMatrix);
                Update();
            }
        }

        /// <summary>
        /// Gets or sets the view projection matrix.
        /// </summary>
        /// <value>The view projection matrix.</value>
        public Matrix ViewProjectionMatrix
        {
            get { return viewProjectionMatrix; }
            set { viewProjectionMatrix = value; }
        }

        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        /// <value>The world matrix.</value>
        public Matrix WorldMatrix
        {
            get { return worldMatrix; }
            set
            {
                worldMatrix = value;
                Update();
            }
        }

        /// <summary>
        /// Gets or sets the X rotation.
        /// </summary>
        /// <value>The X rotation.</value>
        public virtual float XRotation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Y rotation.
        /// </summary>
        /// <value>The Y rotation.</value>
        public virtual float YRotation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Z rotation.
        /// </summary>
        /// <value>The Z rotation.</value>
        public virtual float ZRotation
        {
            get;
            set;
        }
        #endregion

        #region Members
        /// <summary>
        /// Gets the projection matrix.
        /// </summary>
        /// <param name="projectionMatrix">The projection matrix.</param>
        public void GetProjectionMatrix(out Matrix projectionMatrix)
        {
            projectionMatrix = this.projectionMatrix;
        }

        /// <summary>
        /// Gets the view matrix.
        /// </summary>
        /// <param name="viewMatrix">The view matrix.</param>
        public void GetViewMatrix(out Matrix viewMatrix)
        {
            viewMatrix = this.viewMatrix;
        }

        /// <summary>
        /// Gets the view projection matrix.
        /// </summary>
        /// <param name="viewProjectionMatrix">The view projection matrix.</param>
        public void GetViewProjectionMatrix(out Matrix viewProjectionMatrix)
        {
            viewProjectionMatrix = this.viewProjectionMatrix;
        }

        /// <summary>
        /// Gets the world matrix.
        /// </summary>
        /// <param name="worldMatrix">The world matrix.</param>
        public void GetWorldMatrix(out Matrix worldMatrix)
        {
            worldMatrix = this.worldMatrix;
        }

        /// <summary>
        /// Applies a transform to the camera's world matrix,
        /// with the new transform applied first
        /// </summary>
        /// <param name="transform">The transform to be applied</param>
        public void PreTransform(ref Matrix transform)
        {
            Matrix.Multiply(ref transform, ref worldMatrix, out worldMatrix);
            Update();
        }

        /// <summary>
        /// Applies a transform to the camera's world matrix,
        /// with the new transform applied second
        /// </summary>
        /// <param name="transform">The transform to be applied</param>
        public void PostTransform(ref Matrix transform)
        {
            Matrix.Multiply(ref worldMatrix, ref transform, out worldMatrix);
            Update();
        }

        /// <summary>
        /// Sets the view matrix.
        /// </summary>
        /// <param name="viewMatrix">The view matrix.</param>
        public void SetViewMatrix(ref Matrix viewMatrix)
        {
            this.viewMatrix = viewMatrix;
            Matrix.Invert(ref viewMatrix, out worldMatrix);
            Update();
        }

        /// <summary>
        /// Sets the world matrix.
        /// </summary>
        /// <param name="worldMatrix">The world matrix.</param>
        public void SetWorldMatrix(ref Matrix worldMatrix)
        {
            this.worldMatrix = worldMatrix;
            Update();
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public virtual void Update()
        {
            // Create the View Matrix
            Matrix.Invert(ref worldMatrix, out viewMatrix);

            // Create the ViewProjection Matrix
            Matrix.Multiply(ref viewMatrix, ref projectionMatrix, out viewProjectionMatrix);

            // Create the BoundingFrustum
            boundingFrustum.Matrix = viewProjectionMatrix;
        }
        #endregion
    }
}
