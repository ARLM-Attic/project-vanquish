using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectVanquish.Renderers
{
    public class FullScreenQuad
    {
        /// <summary>
        /// A struct that represents a single vertex in the
        /// vertex buffer.
        /// </summary>
        private struct QuadVertex
            : IVertexType
        {
            public Vector3 position;
            public Vector3 texCoordAndCornerIndex;
            public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
                );

            VertexDeclaration IVertexType.VertexDeclaration
            {
                get { return VertexDeclaration; }
            }
        }

        VertexBuffer vertexBuffer;

        /// <summary>
        /// Gets the quad's vertex buffer
        /// </summary>
        public VertexBuffer VertexBuffer
        {
            get { return vertexBuffer; }
        }

        /// <summary>
        /// Creates an instance of FullScreenQuad
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice to use for creating resources</param>
        public FullScreenQuad(GraphicsDevice graphicsDevice)
        {
            CreateFullScreenQuad(graphicsDevice);
        }

        /// <summary>
        /// Draws the full screen quad
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice to use for rendering</param>
        public void Draw(GraphicsDevice graphicsDevice)
        {
            // Set the vertex buffer and declaration
            graphicsDevice.SetVertexBuffer(vertexBuffer);

            // Draw primitives
            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

            graphicsDevice.SetVertexBuffer(null);
        }

        /// <summary>
        /// Creates the VertexBuffer for the quad
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice to use</param>
        private void CreateFullScreenQuad(GraphicsDevice graphicsDevice)
        {
            // Create a vertex buffer for the quad, and fill it in
            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(QuadVertex), QuadVertex.VertexDeclaration.VertexStride * 4, BufferUsage.None);
            QuadVertex[] vbData = new QuadVertex[4];

            // Upper right
            vbData[0].position = new Vector3(1, 1, 1);
            vbData[0].texCoordAndCornerIndex = new Vector3(1, 0, 1);

            // Lower right
            vbData[1].position = new Vector3(1, -1, 1);
            vbData[1].texCoordAndCornerIndex = new Vector3(1, 1, 2);

            // Upper left
            vbData[2].position = new Vector3(-1, 1, 1);
            vbData[2].texCoordAndCornerIndex = new Vector3(0, 0, 0);

            // Lower left
            vbData[3].position = new Vector3(-1, -1, 1);
            vbData[3].texCoordAndCornerIndex = new Vector3(0, 1, 3);


            vertexBuffer.SetData(vbData);
        }
    }
}
