using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using ProjectVanquish.Core;

namespace ProjectVanquish.Models
{
    public abstract class PhysicsObject
    {
        #region Fields
        /// <summary>
        /// Object movable?
        /// </summary>
        bool movable = false;

        /// <summary>
        /// Entity
        /// </summary>
        Entity entity;

        /// <summary>
        /// Object Mass
        /// </summary>
        float mass = 1f;
        #endregion

        //public float Mass
        //{
        //    get { return mass; }
        //    set 
        //    {                
        //        mass = value;
        //        entity.Mass = mass;
        //    }
        //}

        //public Vector3 Position
        //{
        //    get { return entity.Position; }
        //    set { entity.Position = value; }
        //}

        //public Matrix Rotation
        //{
        //    get { return Matrix3X3.ToMatrix4X4(entity.OrientationMatrix); }
        //    set { entity.OrientationMatrix = Matrix3X3.CreateFromMatrix(value); }
        //}

        //public Matrix WorldTransform
        //{
        //    get { return entity.WorldTransform; }
        //    set { entity.WorldTransform = value; }
        //}

        //public bool Movable
        //{
        //    get { return entity.IsDynamic; }
        //    set 
        //    {
        //        if (movable && !value)
        //        {
        //            entity.BecomeKinematic();
        //            return;
        //        }

        //        if (!movable && value)
        //            entity.BecomeDynamic(Mass);
        //    }
        //}

        //public BoundingBox BoundingBox
        //{
        //    get
        //    {
        //        if (entity != null && entity.CollisionInformation != null)
        //            return entity.CollisionInformation.BoundingBox;

        //        return new BoundingBox(Position - Vector3.One, Position + Vector3.One);
        //    }
        //}

        //public Vector3 Scale
        //{
        //    get { return Vector3.One; }
        //}

        //public Vector3 LinearVelocity
        //{
        //    get { return entity.LinearVelocity; }
        //    set { entity.LinearVelocity = value; }
        //}

        //public Vector3 AngularVelocity
        //{
        //    get { return entity.AngularVelocity; }
        //    set { entity.AngularVelocity = value; }
        //}

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicsObject"/> class.
        /// </summary>
        public PhysicsObject(bool isMovable)
        {
            movable = isMovable;
            InitializeEntity();
        }
        #endregion

        #region Members
        /// <summary>
        /// Initializes the entity.
        /// </summary>
        protected void InitializeEntity()
        {
            entity = new Box(Vector3.Zero, 0f, 0f, 0f, mass);
            SceneManager.PhysicsManager.PhysicsObjects.Add(this);
        }

        //public void OffsetModel(Vector3 positionOffset)
        //{
        //    entity.CollisionInformation.LocalPosition = positionOffset;
        //}

        /// <summary>
        /// Removes this instance.
        /// </summary>
        public void Remove()
        {
            entity.Space.Remove(entity);
            SceneManager.PhysicsManager.PhysicsObjects.Remove(this);
        }
        #endregion
    }
}
