using FxEngine.Interfaces;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.IO;
using System.Security.Policy;
using System.Xml.Linq;

namespace dxfViewer
{
    public class HatchGpuMeshSceneObject : AbstractSceneObject, ISceneObject
    {

        public void RestoreXml(XElement elem)
        {

        }

        public override void Store(TextWriter writer)
        {

        }

        protected IGpuObject gpuObject;
        public HatchGpuMeshSceneObject()
        {

        }

        public HatchGpuMeshSceneObject(IGpuObject gpuObject)
        {
            this.gpuObject = gpuObject;
        }

        public override IEnumerable<Vector2d> GetPoints()
        {
            var mtrx = Matrix;
            if (BBox != null)
                return BBox.Select(z => mtrx.ExtractTranslation().Xy + z);

            return [];
        }




        public bool Fill { get; set; } = false;
        public Vector3d? Color { get; internal set; } = null;
        public Vector3d? FillColor { get; internal set; } = null;

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

            var temp = ctx.ModelColor;

            if (Fill)
            {
                ctx.SetModelShader();
            }
            else
            {
                ctx.SetHatchShader();
            }


            if (Fill && FillColor != null)
            {                
                ctx.CurrentShader.SetColor(FillColor.Value.ToVector3() / 255);

            }
            if (!Fill && Color != null)
            {             
                ctx.CurrentShader.SetColor(Color.Value.ToVector3() / 255);
            }
            //ctx.HatchShader.setp(s.End.X - s.Start.X, s.End.Y - s.Start.Y, d.zoom, new float[] { v3[0], v3[1], v3[2] });

            ctx.CurrentShader.setMat4("model", ToMatrix4(dd));
            //ctx.ModelShader.setMat4("model", Matrix4.CreateTranslation((float)Offset.X, (float)Offset.Y, 0));

            gpuObject.Draw();

            ctx.ResetShader();

            ctx.ModelColor = temp;
            GL.Disable(EnableCap.Lighting);

            if (Fill)
            {


            }

            GL.PopMatrix();
        }

        internal void CalcBbox(Vector2d[] arr1)
        {
            var minx = arr1.Min(z => z.X);
            var miny = arr1.Min(z => z.Y);
            var maxx = arr1.Max(z => z.X);
            var maxy = arr1.Max(z => z.Y);
            BBox = new Vector2d[]
            {
                        new Vector2d (minx,miny),
                        new Vector2d (minx,maxy),
                        new Vector2d (maxx,maxy),
                        new Vector2d (maxx,miny),
            };
        }
    }
}
