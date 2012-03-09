/*
 * Modified version of Tim Jones Bounding Box Component
 * 
 * http://timjones.tw/blog/archive/2010/12/10/drawing-an-xna-model-bounding-box
 * 
 */

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectVanquish.Core.BoundingBoxes;
using ProjectVanquish.Cameras;

namespace Vanquish.Renderers
{
    /// <summary>
    /// Bounding Box Renderer
    /// </summary>
    public class BoundingBoxRenderer
    {
        #region Fields
        /// <summary>
        /// Bounding Box Buffers
        /// </summary>
        private BoundingBoxBuffers buffers;

        /// <summary>
        /// Basic Effect
        /// </summary>
        private BasicEffect effect;

        /// <summary>
        /// Graphics Device
        /// </summary>
        private GraphicsDevice device;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBoxRenderer"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="model">The model.</param>
        public BoundingBoxRenderer(GraphicsDevice device, Model model)
        {
            this.device = device;
            BoundingBox boundingBox = CreateBoundingBox(model);
            buffers = CreateBoundingBoxBuffers(boundingBox);

            effect = new BasicEffect(device);
            effect.LightingEnabled = false;
            effect.TextureEnabled = false;
            effect.VertexColorEnabled = true;
        } 
        #endregion

        #region Members
        /// <summary>
        /// Adds the vertex.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="position">The position.</param>
        private static void AddVertex(List<VertexPositionColor> vertices, Vector3 position)
        {
            vertices.Add(new VertexPositionColor(position, Color.Yellow));
        }

        /// <summary>
        /// Creates the bounding box.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        private static BoundingBox CreateBoundingBox(Model model)
        {
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            BoundingBox result = new BoundingBox();
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    BoundingBox? meshPartBoundingBox = GetBoundingBox(meshPart, boneTransforms[mesh.ParentBone.Index]);
                    if (meshPartBoundingBox != null)
                        result = BoundingBox.CreateMerged(result, meshPartBoundingBox.Value);
                }
            return result;
        }

        /// <summary>
        /// Creates the bounding box buffers.
        /// </summary>
        /// <param name="boundingBox">The bounding box.</param>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <returns></returns>
        private BoundingBoxBuffers CreateBoundingBoxBuffers(BoundingBox boundingBox)
        {
            BoundingBoxBuffers boundingBoxBuffers = new BoundingBoxBuffers();
            boundingBoxBuffers.PrimitiveCount = 24;
            boundingBoxBuffers.VertexCount = 48;

            VertexBuffer vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), boundingBoxBuffers.VertexCount, BufferUsage.WriteOnly);
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();

            const float ratio = 5.0f;

            Vector3 xOffset = new Vector3((boundingBox.Max.X - boundingBox.Min.X) / ratio, 0, 0);
            Vector3 yOffset = new Vector3(0, (boundingBox.Max.Y - boundingBox.Min.Y) / ratio, 0);
            Vector3 zOffset = new Vector3(0, 0, (boundingBox.Max.Z - boundingBox.Min.Z) / ratio);
            Vector3[] corners = boundingBox.GetCorners();

            // Corner 1.
            AddVertex(vertices, corners[0]);
            AddVertex(vertices, corners[0] + xOffset);
            AddVertex(vertices, corners[0]);
            AddVertex(vertices, corners[0] - yOffset);
            AddVertex(vertices, corners[0]);
            AddVertex(vertices, corners[0] - zOffset);

            // Corner 2.
            AddVertex(vertices, corners[1]);
            AddVertex(vertices, corners[1] - xOffset);
            AddVertex(vertices, corners[1]);
            AddVertex(vertices, corners[1] - yOffset);
            AddVertex(vertices, corners[1]);
            AddVertex(vertices, corners[1] - zOffset);

            // Corner 3.
            AddVertex(vertices, corners[2]);
            AddVertex(vertices, corners[2] - xOffset);
            AddVertex(vertices, corners[2]);
            AddVertex(vertices, corners[2] + yOffset);
            AddVertex(vertices, corners[2]);
            AddVertex(vertices, corners[2] - zOffset);

            // Corner 4.
            AddVertex(vertices, corners[3]);
            AddVertex(vertices, corners[3] + xOffset);
            AddVertex(vertices, corners[3]);
            AddVertex(vertices, corners[3] + yOffset);
            AddVertex(vertices, corners[3]);
            AddVertex(vertices, corners[3] - zOffset);

            // Corner 5.
            AddVertex(vertices, corners[4]);
            AddVertex(vertices, corners[4] + xOffset);
            AddVertex(vertices, corners[4]);
            AddVertex(vertices, corners[4] - yOffset);
            AddVertex(vertices, corners[4]);
            AddVertex(vertices, corners[4] + zOffset);

            // Corner 6.
            AddVertex(vertices, corners[5]);
            AddVertex(vertices, corners[5] - xOffset);
            AddVertex(vertices, corners[5]);
            AddVertex(vertices, corners[5] - yOffset);
            AddVertex(vertices, corners[5]);
            AddVertex(vertices, corners[5] + zOffset);

            // Corner 7.
            AddVertex(vertices, corners[6]);
            AddVertex(vertices, corners[6] - xOffset);
            AddVertex(vertices, corners[6]);
            AddVertex(vertices, corners[6] + yOffset);
            AddVertex(vertices, corners[6]);
            AddVertex(vertices, corners[6] + zOffset);

            // Corner 8.
            AddVertex(vertices, corners[7]);
            AddVertex(vertices, corners[7] + xOffset);
            AddVertex(vertices, corners[7]);
            AddVertex(vertices, corners[7] + yOffset);
            AddVertex(vertices, corners[7]);
            AddVertex(vertices, corners[7] + zOffset);

            vertexBuffer.SetData(vertices.ToArray());
            boundingBoxBuffers.Vertices = vertexBuffer;

            IndexBuffer indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, boundingBoxBuffers.VertexCount, BufferUsage.WriteOnly);
            indexBuffer.SetData(Enumerable.Range(0, boundingBoxBuffers.VertexCount).Select(i => (short)i).ToArray());
            boundingBoxBuffers.Indices = indexBuffer;

            return boundingBoxBuffers;
        }

        /// <summary>
        /// Draw the Bounding Box for the model
        /// </summary>
        public void Draw(Camera camera)
        {
            device.SetVertexBuffer(buffers.Vertices);
            device.Indices = buffers.Indices;

            effect.World = Matrix.Identity;
            effect.View = camera.ViewMatrix;
            effect.Projection = camera.ProjectionMatrix;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, buffers.VertexCount, 0, buffers.PrimitiveCount);
            }
        }

        /// <summary>
        /// Gets the bounding box.
        /// </summary>
        /// <param name="meshPart">The mesh part.</param>
        /// <param name="transform">The transform.</param>
        /// <returns></returns>
        private static BoundingBox? GetBoundingBox(ModelMeshPart meshPart, Matrix transform)
        {
            if (meshPart.VertexBuffer == null)
                return null;

            Vector3[] positions = VertexElementExtractor.GetVertexElement(meshPart, VertexElementUsage.Position);
            if (positions == null)
                return null;

            Vector3[] transformedPositions = new Vector3[positions.Length];
            Vector3.Transform(positions, ref transform, transformedPositions);

            return BoundingBox.CreateFromPoints(transformedPositions);
        } 
        #endregion
    }
}
