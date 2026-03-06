using netDxf.Entities;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace dxfViewer
{
    public partial class Form1 : Form
    {
        public static Form1 Form;
        public Form1()
        {
            InitializeComponent();
            Form = this;
            menu = new RibbonMenu();

            tableLayoutPanel1.Controls.Add(menu, 0, 0);
            tableLayoutPanel1.SetColumnSpan(menu, 2);
            menu.Height = 115;
            menu.Dock = DockStyle.Top;
            var gl = new GLControlSettings()
            {
                Flags = OpenTK.Windowing.Common.ContextFlags.ForwardCompatible,
                Profile = OpenTK.Windowing.Common.ContextProfile.Compatability,
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                AlphaBits = 8,
                RedBits = 8,
                GreenBits = 8,
                BlueBits = 8,
                NumberOfSamples = 8,
                DepthBits = 24,
                StencilBits = 0
            };

            glControl = new GLControl(gl);
            glControl.Paint += Gl_Paint;

            evwrapper = new EventWrapperGlControl(glControl);

            glControl.MouseDown += GlControl_MouseDown;
            glControl.MouseUp += GlControl_MouseUp;

            tableLayoutPanel1.Controls.Add(glControl, 0, 1);
            glControl.Dock = DockStyle.Fill;
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }
        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                // if (pick != null)
                {
                    middleDrag = true;
                    //  startMeasurePick = pick;
                }
            }

            //CurrentTool.MouseDown(e);
        }
        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            middleDrag = false;
            //CurrentTool.MouseUp(e);
        }
        bool middleDrag = false;
        //IntersectInfo startMeasurePick = null;
        GLControl glControl;
        bool drawAxes = true;
        Vector3d lastHovered;
        object hovered;
        Matrix4d hoveredMatrix;
        public EventWrapperGlControl evwrapper;
        public Camera camera1;
        public CameraViewManager ViewManager;
        bool first = true;

        bool darkMode = true;
        void Redraw()
        {

            ViewManager.Update();

            GL.ClearColor(Color.LightGray);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            var o2 = Matrix4.CreateOrthographic(glControl.Width, glControl.Height, 1, 1000);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref o2);

            Matrix4 modelview2 = Matrix4.LookAt(0, 0, 70, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview2);

            GL.Enable(EnableCap.DepthTest);

            float zz = -500;
            if (darkMode)
            {
                gpuCtx.ModelColor = Color.FromArgb(255, 255, 255);
                GL.Begin(PrimitiveType.Quads);
                GL.Color3(Color.Black);
                GL.Vertex3(-glControl.Width / 2, -glControl.Height / 2, zz);
                GL.Vertex3(glControl.Width / 2, -glControl.Height / 2, zz);
                GL.Color3(Color.Black);
                GL.Vertex3(glControl.Width / 2, glControl.Height / 2, zz);
                GL.Vertex3(-glControl.Width / 2, glControl.Height, zz);
                GL.End();
            }
            else
            {
                gpuCtx.ModelColor = Color.FromArgb(255, 128, 64);
                GL.Begin(PrimitiveType.Quads);
                GL.Color3(Color.LightBlue);
                GL.Vertex3(-glControl.Width / 2, -glControl.Height / 2, zz);
                GL.Vertex3(glControl.Width / 2, -glControl.Height / 2, zz);
                GL.Color3(Color.AliceBlue);
                GL.Vertex3(glControl.Width / 2, glControl.Height / 2, zz);
                GL.Vertex3(-glControl.Width / 2, glControl.Height, zz);
                GL.End();
            }

            camera1.Setup(glControl.Size);

            if (drawAxes)
            {
                GL.PushMatrix();

                GL.LineWidth(2);
                GL.Color3(Color.Red);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(100, 0, 0);
                GL.End();

                GL.Color3(Color.Green);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 100, 0);
                GL.End();

                GL.Color3(Color.Blue);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 0, 100);
                GL.End();
                GL.PopMatrix();
            }
            GL.LineWidth(1);

            GL.Color3(Color.Blue);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            GL.ShadeModel(ShadingModel.Smooth);

            ISceneObject[] parts = null;
            lock (Parts)
            {
                parts = Parts.ToArray();
            }

            foreach (var item in parts)
            {
                if (!item.Visible)
                    continue;

                item.Draw(gpuCtx);
            }


            GL.Disable(EnableCap.Lighting);


            DrawTextOverlay();
            glControl.SwapBuffers();
        }
        GpuDrawingContext gpuCtx = new GpuDrawingContext();

        public List<ISceneObject> Parts = new List<ISceneObject>();
        private void Gl_Paint(object sender, PaintEventArgs e)
        {
            //if (!loaded)
            //  return;
            if (!glControl.Context.IsCurrent)
            {
                glControl.MakeCurrent();
            }

            if (first)
            {
                camera1 = new Camera() { IsOrtho = true, CamTo = new Vector3(0, 0, 0), CamFrom = new Vector3(0, 0, 100), CamUp = new Vector3(0, 1, 0) };
                gpuCtx = new GpuDrawingContext()
                {
                    Camera = camera1,
                    ModelShader = new DefaultModelShader(),
                    TextRenderer = textRenderer
                };

                ViewManager = new DefaultCameraViewManager();
                ViewManager.Attach(evwrapper, camera1);

                // build and compile our shader zprogram
                // ------------------------------------                
                //lightingShader = new Shader("2.2.basic_lighting.vs", "2.2.basic_lighting.fs");
                first = false;
                textRenderer.Init(glControl.Width, glControl.Height);

            }
            Redraw();
        }
        private void DrawMeasureLine(Vector3d snap1, Vector3d snap2)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.LineStipple(1, 0x3F07);
            GL.LineWidth(3);
            GL.Enable(EnableCap.LineStipple);

            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(snap1);
            GL.Vertex3(snap2);
            GL.End();
            GL.LineStipple(1, 0xf8);

            GL.Color3(Color.Blue);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(snap1);
            GL.Vertex3(snap2);
            GL.End();

            GL.PointSize(15);
            GL.Color3(Color.Black);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex3(snap1);
            GL.Vertex3(snap2);
            GL.End();

            GL.PointSize(10);
            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex3(snap1);
            GL.Vertex3(snap2);
            GL.End();

            GL.Disable(EnableCap.LineStipple);
            GL.Enable(EnableCap.DepthTest);
        }

        TextRenderer textRenderer = new TextRenderer();
        private void DrawTextOverlay()
        {
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.DepthTest);

            //textRenderer.RenderText("This is sample text", 25.0f, 25.0f, 1.0f, new Vector3(0.5f, 0.8f, 0.2f));
            //textRenderer.RenderText("(C) LearnOpenGL.com", 10.0f, glControl.Height - 30, 0.5f, new Vector3(0.3f, 0.7f, 0.9f));
            if (hovered != null)
                textRenderer.RenderText("test", 10.0f, glControl.Height - 30, 0.5f, new Vector3(0.3f, 0.7f, 0.9f));

            /*var pos = glControl.PointToClient(Cursor.Position);
            hoverText.Scale = 0.4f;
            hoverText.Position = new Vector2(pos.X, glControl.Height - pos.Y);
            hoverText.Draw(gpuCtx);*/

            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
        }

        MessageFilter mf;
        RibbonMenu menu;

        private void timer1_Tick(object sender, EventArgs e)
        {
            glControl.Invalidate();
        }

        internal void ResetCamera()
        {
            camera1.CamTo = new Vector3(0, 0, 0);
            camera1.CamFrom = new Vector3(0, 0, 100);
            camera1.CamUp = new Vector3(0, 1, 0);
        }

        internal void OpenDxf()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var doc = netDxf.DxfDocument.Load(ofd.FileName);
            int cnt = 0;
            List<Vector3d> vv = new List<Vector3d>();
            Vector3d sum = new Vector3d();
            //Vector3d? first = null;
            foreach (var item in doc.Entities.Solids)
            {

            }
            foreach (var item in doc.Entities.Hatches)
            {
                foreach (var gitem in item.BoundaryPaths)
                {

                    foreach (var d in gitem.Edges)
                    {

                        if (d is HatchBoundaryPath.Polyline pl)
                        {
                            if (pl.IsClosed)
                            {

                            }
                            if (pl.Vertexes.Count() > 4)
                            {

                            }
                        }
                    }
                    foreach (var d in gitem.Entities)
                    {

                    }
                }
            }

            foreach (var item in doc.Entities.Arcs)
            {

                int n = 10;
                var pl = item.ToPolyline2D(n);
                cnt++;
                //if (cnt > 100000)
                //  break;

                var verts = pl.Vertexes;
                List<Vector3d> vvv = new List<Vector3d>();
                var pos = new Vector3d(verts[0].Position.X, verts[0].Position.Y, 0);

                //if (first == null)
                //   first = pos;
                //  pos -= first.Value;
                vvv.Add(pos);
                for (int i = 1; i < verts.Count - 1; i++)
                {
                    netDxf.Entities.Polyline2DVertex vitem = verts[i];
                    pos = new Vector3d(vitem.Position.X, vitem.Position.Y, 0);
                    // pos -= first.Value;
                    sum += pos;

                    vvv.Add(pos);
                    vvv.Add(pos);
                }
                vvv.Add(new Vector3d(verts[^1].Position.X, verts[^1].Position.Y, 0));

                vv.AddRange(vvv.ToArray());
                if (vv.Count > 10000)
                {
                    PolylineGpuObject g = new PolylineGpuObject(vv.ToArray()) { PrimitiveType = PrimitiveType.Lines };
                    PolylineGpuMeshSceneObject sceneObject = new PolylineGpuMeshSceneObject(g);
                    vv.Clear();
                    Parts.Add(sceneObject);
                }


            }
            if (vv.Count > 0)
            {
                PolylineGpuObject g = new PolylineGpuObject(vv.ToArray()) { PrimitiveType = PrimitiveType.Lines };
                PolylineGpuMeshSceneObject sceneObject = new PolylineGpuMeshSceneObject(g);
                vv.Clear();
                Parts.Add(sceneObject);
            }
            sum /= cnt;
            foreach (var item in Parts)
            {
                //item.Matrix.Items.Add(new TranslateTransformChainItem() { Vector = -sum });
            }
            foreach (var item in doc.Entities.Lines)
            {
                int n = 20;
                var verts = new netDxf.Vector3[] { item.StartPoint, item.EndPoint };
                //  if (first == null)
                //  first = new Vector3d(verts[0].X, verts[0].Y, 0);
                PolylineGpuObject g = new PolylineGpuObject(verts.Select(z => new Vector3d(z.X, z.Y, 0)).ToArray()) { PrimitiveType = PrimitiveType.Lines };
                PolylineGpuMeshSceneObject sceneObject = new PolylineGpuMeshSceneObject(g);

                Parts.Add(sceneObject);
            }
        }

        internal void SwitchColorTheme()
        {
            darkMode = !darkMode;
        }

        internal void Settings()
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddBoolField("drawAxes", "Draw axes", drawAxes);
            if (!d.ShowDialog())
                return;

            drawAxes = d.GetBoolField("drawAxes");
        }

        internal void Clear()
        {
            Parts.Clear();
        }
        public void FitToPoints(Vector3d[] pnts, Camera cam, float gap = 10)
        {
            List<Vector2d> vv = new List<Vector2d>();
            foreach (var vertex in pnts)
            {
                var p = MouseRay.Project(vertex.ToVector3(), cam.ProjectionMatrix, cam.ViewMatrix, cam.WorldMatrix, camera1.viewport);
                vv.Add(p.Xy.ToVector2d());
            }

            //prjs->xy coords
            var minx = vv.Min(z => z.X) - gap;
            var maxx = vv.Max(z => z.X) + gap;
            var miny = vv.Min(z => z.Y) - gap;
            var maxy = vv.Max(z => z.Y) + gap;

            var dx = (maxx - minx);
            var dy = (maxy - miny);

            var cx = dx / 2;
            var cy = dy / 2;
            var dir = cam.CamTo - cam.CamFrom;
            //center back to 3d

            var mr = new MouseRay((float)(cx + minx), (float)(cy + miny), cam);
            var v0 = mr.Start;

            cam.CamFrom = v0;
            cam.CamTo = cam.CamFrom + dir;

            var aspect = glControl.Width / (float)(glControl.Height);

            dx /= glControl.Width;
            dx *= camera1.OrthoWidth;
            dy /= glControl.Height;
            dy *= camera1.OrthoWidth;

            cam.OrthoWidth = (float)Math.Max(dx, dy);
        }
        internal void FitAll()
        {
            var pnts = Parts.SelectMany(z => z.GetPoints()).ToArray();
            if (pnts.Any())
                FitToPoints(pnts, camera1);
        }
    }
}
