using Microsoft.Xna.Framework;

namespace ProjectVanquish.Cameras
{
    public interface ICamera
    {
        #region Properties
        /// <summary>
        /// Gets or sets the aspect ratio.
        /// </summary>
        /// <value>The aspect ratio.</value>
        float AspectRatio
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the bounding frustum.
        /// </summary>
        /// <value>The bounding frustum.</value>
        BoundingFrustum BoundingFrustum
        {
            get;
        }

        /// <summary>
        /// Gets or sets the far clip.
        /// </summary>
        /// <value>The far clip.</value>
        float FarClip
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the field of view.
        /// </summary>
        /// <value>The field of view.</value>
        float FieldOfView
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the near clip.
        /// </summary>
        /// <value>The near clip.</value>
        float NearClip
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        /// <value>The orientation.</value>
        Quaternion Orientation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        Vector3 Position
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        /// <value>The projection matrix.</value>
        Matrix ProjectionMatrix
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        /// <value>The view matrix.</value>
        Matrix ViewMatrix
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the view projection matrix.
        /// </summary>
        /// <value>The view projection matrix.</value>
        Matrix ViewProjectionMatrix
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        /// <value>The world matrix.</value>
        Matrix WorldMatrix
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the X rotation.
        /// </summary>
        /// <value>The X rotation.</value>
        float XRotation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Y rotation.
        /// </summary>
        /// <value>The Y rotation.</value>
        float YRotation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Z rotation.
        /// </summary>
        /// <value>The Z rotation.</value>
        float ZRotation
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
        void GetProjectionMatrix(out Matrix projectionMatrix);

        /// <summary>
        /// Gets the view matrix.
        /// </summary>
        /// <param name="viewMatrix">The view matrix.</param>
        void GetViewMatrix(out Matrix viewMatrix);

        /// <summary>
        /// Gets the world matrix.
        /// </summary>
        /// <param name="worldMatrix">The world matrix.</param>
        void GetWorldMatrix(out Matrix worldMatrix);

        /// <summary>
        /// Sets the view matrix.
        /// </summary>
        /// <param name="viewMatrix">The view matrix.</param>
        void SetViewMatrix(ref Matrix viewMatrix);

        /// <summary>
        /// Sets the world matrix.
        /// </summary>
        /// <param name="worldMatrix">The world matrix.</param>
        void SetWorldMatrix(ref Matrix worldMatrix);

        /// <summary>
        /// Updates this instance.
        /// </summary>
        void Update(); 
        #endregion
    }
}
