using System;
using Microsoft.Xna.Framework;

namespace ProjectVanquish.Cameras
{
    public class FirstPersonCamera : PerspectiveCamera
    {
        #region Fields
        /// <summary>
        /// Base Rotation
        /// </summary>
        protected Matrix baseRotation = Matrix.Identity;

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
        /// <param name="fieldOfView">The field of view.</param>
        /// <param name="aspectRatio">The aspect ratio.</param>
        /// <param name="nearClip">The near clip.</param>
        /// <param name="farClip">The far clip.</param>
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
            get { return this.xRotation; }
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
                WorldMatrix = rotationMatrix * baseRotation * translationMatrix;
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
                WorldMatrix = rotationMatrix * baseRotation * translationMatrix;
            }
        }
        #endregion

        #region Members
        /// <summary>
        /// Sets the base rotation.
        /// </summary>
        public void SetBaseRotation()
        {
            baseRotation = worldMatrix;
            baseRotation.Translation = Vector3.Zero;
            XRotation = xRotation;
            YRotation = yRotation;
        }
        #endregion
    }
}
