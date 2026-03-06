using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.IO;
using System.Xml.Linq;

namespace dxfViewer
{
    public class PolylineGpuMeshSceneObject : AbstractSceneObject, ISceneObject
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

        public override IEnumerable<Vector3d> GetPoints()
        {
            var mtrx = Matrix;
            if (BBox != null)
                return BBox.Select(z => mtrx.ExtractTranslation() + z);

            return [];
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
            ctx.ModelShader.setMat4("model", ToMatrix4(dd));
            //ctx.ModelShader.setMat4("model", Matrix4.CreateTranslation((float)Offset.X, (float)Offset.Y, 0));

            gpuObject.Draw();

            GL.UseProgram(0);
            GL.Disable(EnableCap.Lighting);

            if (Fill)
            {


            }

            GL.PopMatrix();
        }

        internal void CalcBbox(Vector3d[] arr1)
        {
            var minx = arr1.Min(z => z.X);
            var miny = arr1.Min(z => z.Y);
            var maxx = arr1.Max(z => z.X);
            var maxy = arr1.Max(z => z.Y);
            BBox = new Vector3d[]
            {
                        new Vector3d (minx,miny,0),
                        new Vector3d (minx,maxy,0),
                        new Vector3d (maxx,maxy,0),
                        new Vector3d (maxx,miny,0),
            };
        }
    }
}
