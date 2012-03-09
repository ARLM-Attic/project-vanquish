using System;
using Microsoft.Xna.Framework;

namespace ProjectVanquish.Cameras
{
    /// <summary>
    /// First Person Camera
    /// </summary>
    public class FirstPersonCamera : PerspectiveCamera
    {
        #region Fields
        /// <summary>
        /// Base Orientation
        /// </summary>
        protected Matrix baseOrientation = Matrix.Identity; 

        /// <summary>
        /// X Rotation
        /// </summary>
        protected float xRotation;

        /// <summary>
        /// Y Rotation
        /// </summary>
        protected float yRotation;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstPersonCamera"/> class.
        /// </summary>
        /// <param name="fieldOfView">The vertical field of view</param>
        /// <param name="aspectRatio">Aspect ratio of the projection</param>
        /// <param name="nearClip">Distance to near clipping plane</param>
        /// <param name="farClip">Distance to far clipping plane</param>
        public FirstPersonCamera(float fieldOfView, float aspectRatio, float nearClip, float farClip)
            : base(fieldOfView, aspectRatio, nearClip, farClip)
        {
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the X rotation.
        /// </summary>
        /// <value>The X rotation.</value>
        public float XRotation
        {
            get { return xRotation; }
            set
            {
                xRotation = value;
                if (xRotation > Math.PI / 2.0f)
                    xRotation = (float)Math.PI / 2.0f;
                else if (xRotation < -Math.PI / 2.0f)
                    xRotation = (float)-Math.PI / 2.0f;
                Matrix rotationMatrix, translationMatrix;
                Matrix.CreateFromYawPitchRoll(yRotation, xRotation, 0, out rotationMatrix);
                translationMatrix = Matrix.CreateTranslation(Position);
                WorldMatrix = rotationMatrix * baseOrientation * translationMatrix;
            }
        }

        /// <summary>
        /// Gets or sets the Y rotation.
        /// </summary>
        /// <value>The Y rotation.</value>
        public float YRotation
        {
            get { return yRotation; }
            set
            {
                yRotation = value;
                Matrix rotationMatrix, translationMatrix;
                Matrix.CreateFromYawPitchRoll(yRotation, xRotation, 0, out rotationMatrix);
                translationMatrix = Matrix.CreateTranslation(Position);
                WorldMatrix = rotationMatrix * baseOrientation * translationMatrix;
            }
        } 
        #endregion

        #region Members
        /// <summary>
        /// Sets the base orientation.
        /// </summary>
        public void SetBaseOrientation()
        {
            baseOrientation = worldMatrix;
            baseOrientation.Translation = Vector3.Zero;
            XRotation = xRotation;
            YRotation = yRotation;
        } 
        #endregion
    }
}
