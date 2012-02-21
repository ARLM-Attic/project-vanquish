using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ProjectVanquish.Renderers
{
    class QuadRenderer
    {
        #region Fields
        /// <summary>
        /// Graphics Device
        /// </summary>
        GraphicsDevice device;

        /// <summary>
        /// Vertex Buffer
        /// </summary>
        VertexBuffer vertexBuffer;

        /// <summary>
        /// Index Buffer
        /// </summary>
        IndexBuffer indexBuffer; 
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="QuadRenderer"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public QuadRenderer(GraphicsDevice device)
        {
            this.device = device;

            // Create the Vertices for our Quad
            VertexPositionTexture[] vertices = 
            {
                new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0))
            };

            // Create the VertexBuffer
            vertexBuffer = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration, vertices.Length, BufferUsage.None);
            vertexBuffer.SetData<VertexPositionTexture>(vertices);

            // Create the Indices
            ushort[] indices = { 0, 1, 2, 2, 3, 0 };

            // Create the IndexBuffer
            indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
            indexBuffer.SetData<ushort>(indices);
        } 
        #endregion

        #region Members
        /// <summary>
        /// Draws the Fullscreen Quad.
        /// </summary>
        public void Draw()
        {
            // Set VertexBuffer
            device.SetVertexBuffer(vertexBuffer);

            // Set IndexBuffer
            device.Indices = indexBuffer;

            // Draw Quad
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        } 
        #endregion
    }
}
