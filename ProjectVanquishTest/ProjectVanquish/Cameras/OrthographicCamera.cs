using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace ProjectVanquish.Cameras
{
    /// <summary>
    /// Camera that uses an orthographic projection
    /// </summary>
    public class OrthographicCamera : Camera
    {
        #region Fields
        float width, height;

        float xMin, xMax, yMin, yMax; 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="OrthographicCamera"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="nearClip">The near clip.</param>
        /// <param name="farClip">The far clip.</param>
        public OrthographicCamera(float width, float height, float nearClip, float farClip)
            : base()
        {
            this.width = width;
            this.height = height;
            this.nearClip = nearClip;
            this.farClip = farClip;
            this.xMax = width / 2;
            this.yMax = height / 2;
            this.xMin = -width / 2;
            this.yMin = -height / 2;
            Matrix.CreateOrthographic(width, height, nearClip, farClip, out projectionMatrix);
            Update();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrthographicCamera"/> class.
        /// </summary>
        /// <param name="xMin">The x min.</param>
        /// <param name="xMax">The x max.</param>
        /// <param name="yMin">The y min.</param>
        /// <param name="yMax">The y max.</param>
        /// <param name="nearClip">The near clip.</param>
        /// <param name="farClip">The far clip.</param>
        public OrthographicCamera(float xMin, float xMax, float yMin, float yMax, float nearClip, float farClip)
            : base()
        {
            this.xMin = xMin;
            this.yMin = yMin;
            this.xMax = xMax;
            this.yMax = yMax;
            this.width = xMax - xMin;
            this.height = yMax - yMin;
            this.nearClip = nearClip;
            this.farClip = farClip;
            Matrix.CreateOrthographicOffCenter(xMin, xMax, yMin, yMax, nearClip, farClip, out projectionMatrix);
            Update();

            Debug.Assert(xMax > xMin && yMax > yMin, "Invalid ortho camera params");
        } 
        #endregion

        #region Properties
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
                Matrix.CreateOrthographicOffCenter(xMin, xMax, yMin, yMax, nearClip, farClip, out projectionMatrix);
                Update();
            }
        }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>The height.</value>
        public float Height
        {
            get { return height; }
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
                Matrix.CreateOrthographicOffCenter(xMin, xMax, yMin, yMax, nearClip, farClip, out projectionMatrix);
                Update();
            }
        }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>The width.</value>
        public float Width
        {
            get { return width; }
        }

        /// <summary>
        /// Gets or sets the X min.
        /// </summary>
        /// <value>The X min.</value>
        public float XMin
        {
            get { return xMin; }
            set
            {
                xMin = value;
                width = xMax - xMin;
                Matrix.CreateOrthographicOffCenter(xMin, xMax, yMin, yMax, nearClip, farClip, out projectionMatrix);
                Update();
            }
        }

        /// <summary>
        /// Gets or sets the X max.
        /// </summary>
        /// <value>The X max.</value>
        public float XMax
        {
            get { return xMax; }
            set
            {
                xMax = value;
                width = xMax - xMin;
                Matrix.CreateOrthographicOffCenter(xMin, xMax, yMin, yMax, nearClip, farClip, out projectionMatrix);
                Update();
            }
        }

        /// <summary>
        /// Gets or sets the Y min.
        /// </summary>
        /// <value>The Y min.</value>
        public float YMin
        {
            get { return xMin; }
            set
            {
                yMin = value;
                height = yMax - yMin;
                Matrix.CreateOrthographicOffCenter(xMin, xMax, yMin, yMax, nearClip, farClip, out projectionMatrix);
                Update();
            }
        }

        /// <summary>
        /// Gets or sets the Y max.
        /// </summary>
        /// <value>The Y max.</value>
        public float YMax
        {
            get { return xMin; }
            set
            {
                yMax = value;
                height = yMax - yMin;
                Matrix.CreateOrthographicOffCenter(xMin, xMax, yMin, yMax, nearClip, farClip, out projectionMatrix);
                Update();
            }
        } 
        #endregion
    }
}
