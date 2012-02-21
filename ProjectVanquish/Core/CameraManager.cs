using System;
using System.Collections.Generic;
using ProjectVanquish.Cameras;

namespace ProjectVanquish.Core
{
    public class CameraManager
    {
        private static Dictionary<string, ICamera> cameras;
        private static string activeCamera;

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraManager"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="camera">The camera.</param>
        public CameraManager(string name, ICamera camera)
        {
            cameras = new Dictionary<string, ICamera>();
            if (camera == null || String.IsNullOrEmpty(name))
                throw new Exception("Camera or name cannot be null");

            AddCamera(name, camera);
            SetActiveCamera(name);
        }

        /// <summary>
        /// Adds the camera.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="camera">The camera.</param>
        public static void AddCamera(string name, ICamera camera)
        {
            if (camera == null || String.IsNullOrEmpty(name))
                throw new Exception("Camera or name cannot be null");

            cameras.Add(name, camera);
        }

        /// <summary>
        /// Gets the active camera.
        /// </summary>
        /// <returns></returns>
        public static ICamera GetActiveCamera()
        {
            if (String.IsNullOrEmpty(activeCamera))
                throw new Exception("No camera is currently active");

            return cameras[activeCamera];
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
    }
}
