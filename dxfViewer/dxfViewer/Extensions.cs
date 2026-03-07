using OpenTK.Mathematics;

namespace dxfViewer
{
    public static class Extensions
    {
        public static Vector3 ToVector3(this Vector3d v)
        {
            return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }

        public static Vector3 ToVector3(this Vector2d v)
        {
            return new Vector3((float)v.X, (float)v.Y, 0);
        }

        public static Vector2d ToVector2d(this Vector2 v)
        {
            return new Vector2d(v.X, v.Y);
        }
    }
}
