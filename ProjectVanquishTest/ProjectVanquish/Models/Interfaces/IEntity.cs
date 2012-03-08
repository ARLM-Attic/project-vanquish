using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectVanquish.Models.Interfaces
{
    interface IEntity
    {
        BoundingBox BoundingBox { get; }

        Model Model { get; set; }

        Vector3 Position { get; set; }

        Vector3 Rotation { get; set; }

        Vector3 Scale { get; set; }
    }
}
