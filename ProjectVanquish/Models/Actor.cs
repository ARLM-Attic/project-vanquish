using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectVanquish.Core;
using ProjectVanquish.Models.Interfaces;
using System;

namespace ProjectVanquish.Models
{
    public class Actor : IEntity, IRenderable
    {
        #region Fields
        BoundingBox boundingBox;
        Matrix[] bones;
        Model model;
        Vector3 position, rotation, scale;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public Actor(Model model)
        {
            this.model = model;
            position = Vector3.Zero;
            rotation = Vector3.Zero;
            scale = Vector3.One;
            bones = new Matrix[model.Bones.Count];
            CalculateBoundingBox();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="position">The position.</param>
        public Actor(Model model, Vector3 position)
        {
            this.model = model;
            this.position = position;
            rotation = Vector3.Zero;
            scale = Vector3.One;
            bones = new Matrix[model.Bones.Count];
            CalculateBoundingBox();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        public Actor(Model model, Vector3 position, Vector3 rotation)
        {
            this.model = model;
            this.position = position;
            this.rotation = rotation;
            scale = Vector3.One;
            bones = new Matrix[model.Bones.Count];
            CalculateBoundingBox();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Actor"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="scale">The scale.</param>
        public Actor(Model model, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.model = model;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            bones = new Matrix[model.Bones.Count];
            CalculateBoundingBox();
        } 
        #endregion

        #region Properties
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
        void CalculateBoundingBox()
        {
            Vector3 modelMax = new Vector3(float.MinValue);
            Vector3 modelMin = new Vector3(float.MaxValue);

            foreach (ModelMesh mesh in model.Meshes)
            {
                Vector3 meshMax = new Vector3(float.MinValue);
                Vector3 meshMin = new Vector3(float.MaxValue);

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    // The stride is how big, in bytes, one vertex is in the vertex buffer
                    // We have to use this as we do not know the make up of the vertex
                    int stride = part.VertexBuffer.VertexDeclaration.VertexStride;

                    byte[] vertexData = new byte[stride * part.NumVertices];
                    part.VertexBuffer.GetData(part.VertexOffset * stride, vertexData, 0, part.NumVertices, 1); // fixed 13/4/11

                    // Find minimum and maximum xyz values for this mesh part
                    // We know the position will always be the first 3 float values of the vertex data
                    Vector3 vertPosition = new Vector3();
                    for (int ndx = 0; ndx < vertexData.Length; ndx += stride)
                    {
                        vertPosition.X = BitConverter.ToSingle(vertexData, ndx);
                        vertPosition.Y = BitConverter.ToSingle(vertexData, ndx + sizeof(float));
                        vertPosition.Z = BitConverter.ToSingle(vertexData, ndx + sizeof(float) * 2);

                        // update our running values from this vertex
                        meshMin = Vector3.Min(meshMin, vertPosition);
                        meshMax = Vector3.Max(meshMax, vertPosition);
                    }
                }

                // transform by mesh bone transforms
                meshMin = Vector3.Transform(meshMin, bones[mesh.ParentBone.Index]);
                meshMax = Vector3.Transform(meshMax, bones[mesh.ParentBone.Index]);

                // Expand model extents by the ones from this mesh
                modelMin = Vector3.Min(modelMin, meshMin);
                modelMax = Vector3.Max(modelMax, meshMax);
            }

            boundingBox = new BoundingBox(modelMin, modelMax);
        }

        /// <summary>
        /// Draws the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="world">The world.</param>
        public void Draw()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(World);
                    effect.Parameters["View"].SetValue(CameraManager.GetActiveCamera().ViewMatrix);
                    effect.Parameters["Projection"].SetValue(CameraManager.GetActiveCamera().ProjectionMatrix);
                }

                mesh.Draw();
            }
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
