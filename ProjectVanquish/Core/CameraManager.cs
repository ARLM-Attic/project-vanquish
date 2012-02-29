using System;
using System.Collections.Generic;
using ProjectVanquish.Cameras;

namespace ProjectVanquish.Core
{
    public class CameraManager
    {
        #region Fields
        /// <summary>
        /// Dictionary of Cameras
        /// </summary>
        private static Dictionary<string, BaseCamera> cameras;

        /// <summary>
        /// Active Camera Name
        /// </summary>
        private static string activeCamera; 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraManager"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="camera">The camera.</param>
        public CameraManager(string name, BaseCamera camera)
        {
            cameras = new Dictionary<string, BaseCamera>();
            if (camera == null || String.IsNullOrEmpty(name))
                throw new Exception("Camera or name cannot be null");

            AddCamera(name, camera);
            SetActiveCamera(name);
        } 
        #endregion

        #region Members
        /// <summary>
        /// Adds the camera.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="camera">The camera.</param>
        public static void AddCamera(string name, BaseCamera camera)
        {
            if (camera == null || String.IsNullOrEmpty(name))
                throw new Exception("Camera or name cannot be null");

            cameras.Add(name, camera);
        }

        /// <summary>
        /// Gets the active camera.
        /// </summary>
        /// <returns></returns>
        public static BaseCamera GetActiveCamera()
        {
            if (String.IsNullOrEmpty(activeCamera))
                throw new Exception("No camera is currently active");

            return (BaseCamera)cameras[activeCamera];
        }

        /// <summary>
        /// Removes the camera.
        /// </summary>
        /// <param name="name">The name.</param>
        public static void RemoveCamera(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new Exception("No camera is currently active");

            cameras.Remove(name);
        }

        /// <summary>
        /// Sets the active camera.
        /// </summary>
        /// <param name="name">The name.</param>
        public static void SetActiveCamera(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("Name cannot be null");

            if (!cameras.ContainsKey(name))
                throw new ArgumentOutOfRangeException("No camera exists with that name.");

            activeCamera = name;
        } 
        #endregion
    }
}
