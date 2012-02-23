using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectVanquish.Models.Interfaces
{
    interface IRenderable
    {
        void Draw();

        void DrawWithEffect(GraphicsDevice device, Effect effect);
    }
}
