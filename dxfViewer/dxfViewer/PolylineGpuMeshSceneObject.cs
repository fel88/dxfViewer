using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.IO;
using System.Xml.Linq;

namespace dxfViewer
{
    public class PolylineGpuMeshSceneObject : AbstractSceneObject
    {

        public void RestoreXml(XElement elem)
        {

        }

        public override void Store(TextWriter writer)
        {

        }

        protected PolylineGpuObject gpuObject;
        public PolylineGpuMeshSceneObject()
        {

        }

        public PolylineGpuMeshSceneObject(PolylineGpuObject gpuObject)
        {
            this.gpuObject = gpuObject;
        }
        public Vector3d[] BBox;
        public IEnumerable<Vector3d> GetPoints()
        {
            var mtrx = Matrix;

            return BBox.Select(z => Vector3d.TransformVector(z, mtrx));
        }



        
        public bool Fill { get; set; } = true;


        public OpenTK.Mathematics.Matrix4 ToMatrix4(Matrix4d matrix4d)
        {
            OpenTK.Mathematics.Matrix4 matrix4 = new(
                new OpenTK.Mathematics.Vector4((float)matrix4d.Row0.X, (float)matrix4d.Row0.Y, (float)matrix4d.Row0.Z, (float)matrix4d.Row0.W),
                new OpenTK.Mathematics.Vector4((float)matrix4d.Row1.X, (float)matrix4d.Row1.Y, (float)matrix4d.Row1.Z, (float)matrix4d.Row1.W),
                new OpenTK.Mathematics.Vector4((float)matrix4d.Row2.X, (float)matrix4d.Row2.Y, (float)matrix4d.Row2.Z, (float)matrix4d.Row2.W),
                new OpenTK.Mathematics.Vector4((float)matrix4d.Row3.X, (float)matrix4d.Row3.Y, (float)matrix4d.Row3.Z, (float)matrix4d.Row3.W)
            );
            return matrix4;
        }

        public override void Draw(GpuDrawingContext ctx)
        {
            if (!Visible)
                return;

            GL.PushMatrix();
            if (Parent != null)
            {
                var dd2 = Parent.Matrix;
                GL.MultMatrix(ref dd2);
            }

            Matrix4d dd = Matrix;
            GL.MultMatrix(ref dd);
            ctx.SetModelShader();
            //ctx.ModelShader.setMat4("model", ToMatrix4(dd));

            gpuObject.Draw();

            GL.UseProgram(0);
            GL.Disable(EnableCap.Lighting);

            if (Fill)
            {


            }
           
            GL.PopMatrix();
        }
    }
}
