using OpenTK.Mathematics;

namespace dxfViewer
{
    public class LocalContour
    {
        public float Len
        {
            get
            {
                float len = 0;
                for (int i = 1; i <= Points.Count; i++)
                {
                    var p1 = Points[i - 1];
                    var p2 = Points[i % Points.Count];
                    len += (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
                }
                return len;
            }
        }
        public List<Vector2d> Points = new List<Vector2d>();
        public bool Enable = true;
        public List<LocalContour> Childrens = new List<LocalContour>();
        public LocalContour Parent;
        public object Tag;

        public void Scale(double v)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new Vector2d((float)(Points[i].X * v), (float)(Points[i].Y * v));
            }
        }

        internal bool IsClosed(double eps = 1e-6)
        {
            return (Points[0] - Points[^1]).Length < eps;
        }
    }
}
