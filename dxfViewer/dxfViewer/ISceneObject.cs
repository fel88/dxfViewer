using OpenTK.Mathematics;
using System.IO;

namespace dxfViewer
{
    public interface ISceneObject
    {
        void Draw(GpuDrawingContext ctx);
        int Id { get; set; }
        ISceneObject Parent { get; set; }
        List<ISceneObject> Childs { get; }
        string Name { get; set; }
        bool Visible { get; set; }
        bool Frozen { get; set; }

        bool Selected { get; set; }
        Matrix4d Matrix { get; }

        ISceneObject[] GetAll(Predicate<ISceneObject> p);
        void RemoveChild(ISceneObject dd);
        void Store(TextWriter writer);
        int Z { get; set; }
    }
}
