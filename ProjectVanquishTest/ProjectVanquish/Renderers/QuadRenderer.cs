using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ProjectVanquish.Renderers
{
    public partial class QuadRenderer : DrawableGameComponent
    {
        #region Fields
        VertexPositionTexture[] verts = null;
        short[] ib = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="QuadRenderer"/> class.
        /// </summary>
        /// <param name="game">The Game that the game component should be attached to.</param>
        public QuadRenderer(Game game)
            : base(game)
        {
        }
        #endregion

        #region Members
        /// <summary>
        /// Called when graphics resources need to be loaded. Override this method to load any component-specific graphics resources.
        /// </summary>
        protected override void LoadContent()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)base.Game.Services.GetService(typeof(IGraphicsDeviceService));

            verts = new VertexPositionTexture[]
                        {
                            new VertexPositionTexture(new Vector3(0,0,0), new Vector2(1,1)),
                            new VertexPositionTexture(new Vector3(0,0,0), new Vector2(0,1)),
                            new VertexPositionTexture(new Vector3(0,0,0), new Vector2(0,0)),
                            new VertexPositionTexture(new Vector3(0,0,0), new Vector2(1,0))
                        };

            ib = new short[] { 0, 1, 2, 2, 3, 0 };
        }

        /// <summary>
        /// Renders the specified v1.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        public void Render(Vector2 v1, Vector2 v2)
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)base.Game.Services.GetService(typeof(IGraphicsDeviceService));
            GraphicsDevice device = graphicsService.GraphicsDevice;

            verts[0].Position.X = v2.X;
            verts[0].Position.Y = v1.Y;

            verts[1].Position.X = v1.X;
            verts[1].Position.Y = v1.Y;

            verts[2].Position.X = v1.X;
            verts[2].Position.Y = v2.Y;

            verts[3].Position.X = v2.X;
            verts[3].Position.Y = v2.Y;

            device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, verts, 0, 4, ib, 0, 2);
        }
        #endregion
    }
}