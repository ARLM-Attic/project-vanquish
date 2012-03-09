using Microsoft.Xna.Framework;

namespace ProjectVanquish.Cameras
{
    /// <summary>
    /// Abstract base class for all camera types
    /// </summary>
    public abstract class Camera
    {
        #region Fields
        protected Matrix viewMatrix = Matrix.Identity;
        protected Matrix worldMatrix = Matrix.Identity;
        protected Matrix projectionMatrix = Matrix.Identity;
        protected Matrix viewProjMatrix = Matrix.Identity;
        protected BoundingFrustum boundingFrustum;
        protected float nearClip;
        protected float farClip; 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        public Camera()
        {
            boundingFrustum = new BoundingFrustum(viewProjMatrix);
            worldMatrix = Matrix.Identity;
            viewMatrix = Matrix.Identity;
        } 
        #endregion

        #region Properties
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
            get { return farClip; }
            set { }
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
            get { return viewProjMatrix; }
            set { viewProjMatrix = value; }
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
        /// Gets the view proj matrix.
        /// </summary>
        /// <param name="viewProjMatrix">The view proj matrix.</param>
        public void GetViewProjMatrix(out Matrix viewProjMatrix)
        {
            viewProjMatrix = this.viewProjMatrix;
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
        /// Updates the view-projection matrix and frustum coordinates based on
        /// the current camera position/orientation and projection parameters.
        /// </summary>
        protected void Update()
        {
            // Make our view matrix
            Matrix.Invert(ref worldMatrix, out viewMatrix);

            // Create the combined view-projection matrix
            Matrix.Multiply(ref viewMatrix, ref projectionMatrix, out viewProjMatrix);

            // Create the bounding frustum
            boundingFrustum.Matrix = viewProjMatrix;
        } 
        #endregion
    }
}
