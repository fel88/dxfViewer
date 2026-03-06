using OpenTK.Mathematics;
using System.IO;

namespace dxfViewer
{
    public interface ISceneObject
    {
        void Draw(GpuDrawingContext ctx);
        int Id { get; set; }
        Vector3d[] BBox { get; }
        Vector2d Offset { get; set; }
        IEnumerable<Vector3d> GetPoints();
        ISceneObject Parent { get; set; }
        List<ISceneObject> Childs { get; }
        string Name { get; set; }
        bool Visible { get; set; }
        bool Frozen { get; set; }

        bool Selected { get; set; }
        Matrix4d Matrix { get; set; }

        ISceneObject[] GetAll(Predicate<ISceneObject> p);
        void RemoveChild(ISceneObject dd);
        void Store(TextWriter writer);
        int Z { get; set; }
    }
}
