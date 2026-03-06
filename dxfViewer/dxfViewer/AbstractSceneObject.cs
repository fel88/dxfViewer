using OpenTK.Mathematics;
using System.IO;
using System.Xml.Linq;

namespace dxfViewer
{
    public abstract class AbstractSceneObject :ISceneObject
    {
        
        public AbstractSceneObject()
        {
            
        }
        public bool Frozen { get; set; }

        public AbstractSceneObject(XElement item)
        {
            
        }

        public string Name { get; set; }

        public abstract void Draw(GpuDrawingContext ctx);

        public virtual void RemoveChild(ISceneObject dd)
        {
            Childs.Remove(dd);
        }

        public virtual void Store(TextWriter writer)
        {

        }

        public virtual ISceneObject[] GetAll(Predicate<ISceneObject> p)
        {
            if (Childs.Count == 0)
                return [this];

            return new[] { this }.Union(Childs.SelectMany(z => z.GetAll(p))).ToArray();
        }

        public abstract IEnumerable<Vector3d> GetPoints();

        public bool Visible { get; set; } = true;
        public bool Selected { get; set; }

        public List<ISceneObject> Childs { get; set; } = new List<ISceneObject>();

        public ISceneObject Parent { get; set; }
        public int Id { get; set; }

        
        public Matrix4d Matrix { get; set; }
        public int Z { get; set; }
    }
}
