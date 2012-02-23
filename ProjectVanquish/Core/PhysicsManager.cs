using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Constraints;
using BEPUphysics.Settings;
using Microsoft.Xna.Framework;
using ProjectVanquish.Models;

namespace ProjectVanquish.Core
{
    public class PhysicsManager
    {
        #region Fields
        /// <summary>
        /// BEPU Space
        /// </summary>
        private Space space;

        /// <summary>
        /// List of Physics Objects
        /// </summary>
        private IList<PhysicsObject> physicsObjects; 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicsManager"/> class.
        /// </summary>
        /// <param name="gravity">The gravity.</param>
        public PhysicsManager(Vector3 gravity)
        {
            space = new Space();
            space.ForceUpdater.Gravity = gravity;
            SolverSettings.DefaultMinimumIterations = 2;
            MotionSettings.DefaultPositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            MotionSettings.UseExtraExpansionForContinuousBoundingBoxes = true;
            MotionSettings.CoreShapeScaling = 0.99f;
            space.Solver.IterationLimit = 20;

            // Check if we can use mutli-threading
            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < Environment.ProcessorCount - 1; i++)
                    space.ThreadManager.AddThread();

                space.BroadPhase.AllowMultithreading = true;
            }

            physicsObjects = new List<PhysicsObject>();
        } 
        #endregion

        #region Properties
        /// <summary>
        /// Gets the physics objects.
        /// </summary>
        /// <value>The physics objects.</value>
        public IList<PhysicsObject> PhysicsObjects { get { return physicsObjects; } }

        /// <summary>
        /// Gets the Space.
        /// </summary>
        /// <value>The space.</value>
        public Space Space { get { return space; } } 
        #endregion

        #region Members
        /// <summary>
        /// Updates the physics engine.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        public void Update(GameTime gameTime)
        {
            space.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        } 
        #endregion
    }
}
