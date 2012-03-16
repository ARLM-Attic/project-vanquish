using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectVanquish.Cameras;
using ProjectVanquish.Models.Interfaces;
using Vanquish.Renderers;

namespace ProjectVanquish.Models
{
    public class Actor : IEntity
    {
        #region Fields
        BoundingBox boundingBox;
        BoundingBoxRenderer boundingBoxRenderer;
        Matrix[] bones;
        Model model;
        Vector3 position, rotation, scale;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public Actor(GraphicsDevice device, Model model)
        {
            this.model = model;
            position = Vector3.Zero;
            rotation = Vector3.Zero;
            scale = Vector3.One;
            bones = new Matrix[model.Bones.Count];
            CalculateBoundingBox(device);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="position">The position.</param>
        public Actor(GraphicsDevice device, Model model, Vector3 position)
        {
            this.model = model;
            this.position = position;
            rotation = Vector3.Zero;
            scale = Vector3.One;
            bones = new Matrix[model.Bones.Count];
            CalculateBoundingBox(device);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        public Actor(GraphicsDevice device, Model model, Vector3 position, Vector3 rotation)
        {
            this.model = model;
            this.position = position;
            this.rotation = rotation;
            scale = Vector3.One;
            bones = new Matrix[model.Bones.Count];
            CalculateBoundingBox(device);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="scale">The scale.</param>
        public Actor(GraphicsDevice device, Model model, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.model = model;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            bones = new Matrix[model.Bones.Count];
            CalculateBoundingBox(device);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the bones.
        /// </summary>
        public Matrix[] Bones
        {
            get { return bones; }
        }

        /// <summary>
        /// Gets the bounding box.
        /// </summary>
        /// <value>The bounding box.</value>
        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }

        /// <summary>
        /// Gets the bounding sphere.
        /// </summary>
        /// <value>The bounding sphere.</value>
        public BoundingSphere BoundingSphere
        {
            get { return model.Meshes[0].BoundingSphere; }
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        public Model Model
        {
            get { return model; }
            set { model = value; }
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        /// <value>The rotation.</value>
        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>The scale.</value>
        public Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        /// <summary>
        /// Gets the world.
        /// </summary>
        /// <value>The world.</value>
        public Matrix World
        {
            get
            {
                return Matrix.CreateTranslation(position) * Matrix.CreateScale(scale)
                    * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationY(rotation.Y)
                    * Matrix.CreateRotationZ(rotation.Z);
            }
        }
        #endregion

        #region Members
        /// <summary>
        /// Calculates the bounding box.
        /// </summary>
        void CalculateBoundingBox(GraphicsDevice device)
        {
            boundingBoxRenderer = new BoundingBoxRenderer(device, model);
        }

        /// <summary>
        /// Draws the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="world">The world.</param>
        public void Draw(Camera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(World);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                }

                mesh.Draw();
            }
        }

        /// <summary>
        /// Draws the bounding box.
        /// </summary>
        public void DrawBoundingBox(Camera camera)
        {
            boundingBoxRenderer.Draw(camera);
        }

        /// <summary>
        /// Draws the with effect.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="effect">The effect.</param>
        public void DrawWithEffect(GraphicsDevice device, Effect effect)
        {
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                ModelMesh mesh = model.Meshes[i];
                Matrix world;
                world = World;
                effect.Parameters["g_matWorld"].SetValue(Matrix.CreateScale(scale) * World);

                Matrix transpose, inverseTranspose;
                Matrix.Transpose(ref world, out transpose);
                Matrix.Invert(ref transpose, out inverseTranspose);
                effect.Parameters["g_matWorldIT"].SetValue(inverseTranspose);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        // Set VertexBuffer
                        device.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);

                        // Set IndexBuffer
                        device.Indices = part.IndexBuffer;

                        // Apply the Effect
                        pass.Apply();

                        // Draw the Primitives
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
        }
        #endregion
    }
}
