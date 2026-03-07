using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace dxfViewer
{
    public class PolylineGpuObject : IDisposable, IGpuObject
    {
        bool deleted = false;


        int VBO, VAO;
        int numPoints;
        public PolylineGpuObject(Vector2d[] verts)
        {
            int idx = 0;
            float[] vertices = new float[verts.Length * 2];
            for (int i = 0; i < verts.Length; i++)
            {
                vertices[idx++] = (float)verts[i].X;
                vertices[idx++] = (float)verts[i].Y;

            }

            numPoints = verts.Length;

            GL.GenVertexArrays(1, out VAO);
            GL.GenBuffers(1, out VBO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(VAO);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

        }

        public PrimitiveType PrimitiveType = PrimitiveType.LineStrip;

        public void Draw()
        {
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType, 0, numPoints);
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
