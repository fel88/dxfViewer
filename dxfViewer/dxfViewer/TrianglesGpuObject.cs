using FxEngine.Interfaces;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

namespace dxfViewer
{
    public class Triangles2dGpuObject : IDisposable, IGpuObject
    {
        bool deleted = false;


        int VBO, VAO;
        int numVerts;
        public Triangles2dGpuObject(Vector2d[] verts)
        {
            int idx = 0;
            float[] vertices = new float[verts.Length * 2];
            for (int i = 0; i < verts.Length; i++)
            {
                vertices[idx++] = (float)verts[i].X;
                vertices[idx++] = (float)verts[i].Y;

            }

            numVerts = verts.Length;

            GL.GenVertexArrays(1, out VAO);
            GL.GenBuffers(1, out VBO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(VAO);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

        }
        public static bool AllowFix = true;
        public static Vector2d[][] TriangulateWithHoles(Vector2d[][] points, Vector2d[][] holes)
        {

            TriangleNet.Geometry.Polygon poly2 = new TriangleNet.Geometry.Polygon();


            foreach (var item in points)
            {
                var a = item.Select(z => new Vertex(z.X, z.Y, 0)).ToArray();
                if (a.Count() > 2)
                {
                    poly2.Add(new Contour(a));
                }
            }

            foreach (var item in holes)
            {
                var a = item.Select(z => new Vertex(z.X, z.Y, 0)).ToArray();
                if (a.Count() > 2)
                {
                    poly2.Add(new Contour(a), true);

                }
            }


            ConstraintMesher.ScoutCounter = 0;


            var trng = (new GenericMesher()).Triangulate(poly2, new ConstraintOptions(), new QualityOptions());

            return trng.Triangles.Select(z => new Vector2d[] {
                  new Vector2d(z.GetVertex(0).X, z.GetVertex(0).Y),
                  new Vector2d(z.GetVertex(1).X, z.GetVertex(1).Y),
                  new Vector2d(z.GetVertex(2).X, z.GetVertex(2).Y)
            }
                  ).ToArray();

        }

        int[] _vBO = new int[2]; //vertex buffer objects
                                 //Setting Up
        int count;
        int tcount;
        public PointF[] Verts;

        public Vector2d[] Fix(Vector2d[] a, float tolerance = 0.01f)
        {
            List<Vector2d> pnts2 = new List<Vector2d>();

            for (int j = 1; j < a.Count(); j++)
            {
                if (j == 1)
                {
                    pnts2.Add(a[0]);
                }
                var p0 = a[j - 1];
                var p1 = a[j];
                var ds = Math.Sqrt(Math.Pow(p0.X - p1.X, 2) + Math.Pow(p0.Y - p1.Y, 2));
                if (ds > tolerance)
                {
                    pnts2.Add(a[j]);
                }
            }
            return pnts2.ToArray();
        }

        public void init(Vector2d[][] verts, Vector2d[][] holes)
        {

            #region fix points
            if (AllowFix)
            {
                for (int i = 0; i < verts.Count(); i++)
                {
                    verts[i] = Fix(verts[i]);
                }
                for (int i = 0; i < holes.Count(); i++)
                {
                    holes[i] = Fix(holes[i]);
                }
            }
            #endregion

            count = verts.Length / 3;
            Verts = new PointF[count];
            int index = 0;



            //GeomLib.Polygon poly = new GeomLib.Polygon();
            List<PointF> ppp = new List<PointF>();
            for (int i = 0; i < Verts.Length; i += 2)
            {
                ppp.Add(Verts[i]);
            }
            //            poly.Points = ppp.ToArray();
            var trgs = TriangulateWithHoles(verts, holes);

            tcount = trgs.Count() * 3;

            GL.GenVertexArrays(1, out VAO);
            GL.GenBuffers(2, _vBO);
            GL.BindVertexArray(VAO);



            List<double> verts3 = new List<double>();
            foreach (var item in trgs)
            {

                foreach (var citem in item)
                {
                    verts3.Add(citem.X);
                    verts3.Add(citem.Y);
                    verts3.Add(-0.1f);
                }
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vBO[1]);
            GL.BufferData(BufferTarget.ArrayBuffer,
                new IntPtr(sizeof(float) * verts3.Count), verts3.ToArray(), BufferUsageHint.DynamicDraw);
            GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindVertexArray(0);

        }

        public void Draw()
        {
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, numVerts);
            GL.BindVertexArray(0);

        }

        public void Dispose()
        {
            if (deleted)
                return;

            deleted = true;
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO);
        }
    }
}
