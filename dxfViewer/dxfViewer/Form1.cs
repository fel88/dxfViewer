using netDxf.Collections;
using netDxf.Entities;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Diagnostics;
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
            glControl.MouseMove += GlControl_MouseMove;
            glControl.MouseWheel += GlControl_MouseWheel;
            tableLayoutPanel1.Controls.Add(glControl, 0, 1);
            glControl.Dock = DockStyle.Fill;
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);


        }
        const int MinimalArcDivider = 10;

        private void GlControl_MouseWheel(object? sender, MouseEventArgs e)
        {
            dirty = true;
        }

        private void GlControl_MouseMove(object? sender, MouseEventArgs e)
        {
            dirty = true;
        }

        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                // if (pick != null)
                {
                    middleDrag = true;
                    //  startMeasurePick = pick;
                    dirty = true;
                }
            }

            //CurrentTool.MouseDown(e);
        }
        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            middleDrag = false;
            dirty = true;
            //CurrentTool.MouseUp(e);
        }
        bool middleDrag = false;
        //IntersectInfo startMeasurePick = null;
        GLControl glControl;
        bool drawAxes = true;
        bool DrawHatches = true;
        bool FillHatchesOnLoad = true;
        bool SolidHatchFilling = false;
        Vector3d lastHovered;
        object hovered;
        Matrix4d hoveredMatrix;
        public EventWrapperGlControl evwrapper;
        public Camera camera1;
        public CameraViewManager ViewManager;
        bool first = true;

        bool darkMode = true;
        bool dirty = true;
        void Redraw()
        {

            foreach (var p in Parts.OfType<HatchGpuMeshSceneObject>())
            {
                p.Fill = SolidHatchFilling;
            }

            //gpuCtx.CurrentHatchColor = SolidHatchFilling ? gpuCtx.ModelColor : gpuCtx.HatchColor;
            gpuCtx.HatchShader.use();
            gpuCtx.HatchShader.setVec3("bgColor", darkMode ? new Vector3(0, 0, 0) : new Vector3(1, 1, 1));
            gpuCtx.ResetShader();
            ViewManager.Update();


            GL.ClearColor(darkMode ? Color.Black : Color.White);
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
                /*GL.Begin(PrimitiveType.Quads);
                GL.Color3(Color.Black);
                GL.Vertex3(-glControl.Width / 2, -glControl.Height / 2, zz);
                GL.Vertex3(glControl.Width / 2, -glControl.Height / 2, zz);
                GL.Color3(Color.Black);
                GL.Vertex3(glControl.Width / 2, glControl.Height / 2, zz);
                GL.Vertex3(-glControl.Width / 2, glControl.Height, zz);
                GL.End();*/
            }
            else
            {
                gpuCtx.ModelColor = Color.Black;
                /*GL.Begin(PrimitiveType.Quads);
                //GL.Color3(Color.LightBlue);
                GL.Color3(Color.White);
                GL.Vertex3(-glControl.Width / 2, -glControl.Height / 2, zz);
                GL.Vertex3(glControl.Width / 2, -glControl.Height / 2, zz);
                //GL.Color3(Color.AliceBlue);
                GL.Color3(Color.White);
                GL.Vertex3(glControl.Width / 2, glControl.Height / 2, zz);
                GL.Vertex3(-glControl.Width / 2, glControl.Height, zz);
                GL.End();*/
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
            //GL.Enable(EnableCap.Light0);

            //GL.ShadeModel(ShadingModel.Smooth);
            //gpuCtx.SetModelShader();
            //gpuCtx.SetHatchShader();

            ISceneObject[] parts = null;
            lock (Parts)
            {
                parts = Parts.ToArray();
            }

            foreach (var item in parts)
            {
                if (!item.Visible)
                    continue;

                if (item is HatchGpuMeshSceneObject && !DrawHatches)
                    continue;

                item.Draw(gpuCtx);
            }


            GL.Disable(EnableCap.Lighting);


            DrawTextOverlay();
            glControl.SwapBuffers();
            dirty = false;
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
                    Control = glControl,
                    Camera = camera1,
                    ModelShader = new DefaultModelShader(),
                    HatchShader = new HatchShader(),
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

            var pos = glControl.PointToClient(Cursor.Position);
            var dx = gpuCtx.Camera.OrthoWidth;
            var zoom = glControl.Width / gpuCtx.Camera.OrthoWidth;

            var cam = gpuCtx.Camera;
            var p = MouseRay.UnProject(new Vector3(pos.X, pos.Y, 0), cam.ProjectionMatrix, cam.ViewMatrix, new Size(camera1.viewport[2], camera1.viewport[3]));


            textRenderer.RenderText(gpuCtx, $"X: {Math.Round(p.X, 2)}", 0, glControl.Height - textRenderer.FontSize, 1.0f, new Vector3(0.5f, 0.8f, 0.2f));
            textRenderer.RenderText(gpuCtx, $"Y: {Math.Round(p.Y, 2)}", 0, glControl.Height - textRenderer.FontSize*2, 1.0f, new Vector3(0.5f, 0.8f, 0.2f));
            //textRenderer.RenderText("(C) LearnOpenGL.com", 10.0f, glControl.Height - 30, 0.5f, new Vector3(0.3f, 0.7f, 0.9f));
            if (hovered != null)
                textRenderer.RenderText(gpuCtx, "test", 10.0f, glControl.Height - 30, 0.5f, new Vector3(0.3f, 0.7f, 0.9f));

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

            toolStripProgressBar1.Value = progressValue;
            if (dirty)
            {
                glControl.Invalidate();
            }
        }

        internal void ResetCamera()
        {
            camera1.CamTo = new Vector3(0, 0, 0);
            camera1.CamFrom = new Vector3(0, 0, 100);
            camera1.CamUp = new Vector3(0, 1, 0);
            dirty = true;
        }

        internal async void OpenDxf()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            Parts.Clear();
            dirty = true;
            LoadDxf(ofd.FileName);
            GC.Collect(2);
            GC.WaitForPendingFinalizers();
        }

        PolylineGpuObject Create(Vector2d[] arr1)
        {
            PolylineGpuObject g = null;
            glControl.Invoke(() =>
            {
                g = new PolylineGpuObject(arr1) { PrimitiveType = PrimitiveType.Lines };
            });
            return g;
        }
        public static double RemoveThreshold = 10e-5;
        public static double ClosingThreshold = 10e-2;

        public static LocalContour[] ConnectElements(Vector2d[][] elems)
        {
            List<LocalContour> ret = new List<LocalContour>();

            List<Vector2d> pp = new List<Vector2d>();
            List<Vector2d[]> last = new List<Vector2d[]>();
            last.AddRange(elems);

            while (last.Any())
            {
                if (pp.Count == 0)
                {
                    pp.AddRange(last.First());

                    last.RemoveAt(0);
                }
                else
                {
                    var ll = pp.Last();
                    var f1 = last.OrderBy(z => Math.Min((z[0] - ll).Length, (z[1] - ll).Length)).First();

                    var dist = Math.Min((f1[0] - ll).Length, (f1[1] - ll).Length);
                    if (dist > ClosingThreshold)
                    {
                        ret.Add(new LocalContour() { Points = pp.ToList() });
                        pp.Clear();

                        continue;
                    }

                    last.Remove(f1);
                    if ((f1[0] - ll).Length < (f1[1] - ll).Length)
                    {
                        pp.AddRange(f1.Skip(1));
                        //pp.Add(f1.End);
                    }
                    else
                    {
                        f1 = f1.Reverse().ToArray();
                        pp.AddRange(f1.Skip(1));
                        //pp.Add(f1.Start);
                    }
                }
            }
            if (pp.Any())
            {
                ret.Add(new LocalContour() { Points = pp.ToList() });
            }
            return ret.ToArray();
        }

        void AddDxf(netDxf.DxfDocument doc)
        {
            List<ISceneObject> toAdd = new List<ISceneObject>();
            int cnt = 0;
            List<Vector2d> vv = new List<Vector2d>();

            //Vector3d? first = null;
            var total = doc.Entities.All.Count();
            UpdateProgressBar(0, total);
            foreach (var item in doc.Entities.All)
            {
                var nm = item.GetType().Name;

            }
            const int MinimalCircleDivider = 18;


            List<Vector2d> accum = new List<Vector2d>();

            foreach (var item in doc.Entities.Hatches)
            {

                progressValue = cnt++;

                foreach (var gitem in item.BoundaryPaths)
                {

                    List<Vector2d> segments = new List<Vector2d>();
                    foreach (var d in gitem.Edges)
                    {
                        if (d is HatchBoundaryPath.Polyline pl)
                        {
                            List<Vector2d> accum0 = new List<Vector2d>();
                            var arr1 = pl.Vertexes.Select(z => new Vector2d(z.X, z.Y)).ToArray();
                            arr1 = StripToLines(arr1);
                            accum0.AddRange(arr1);
                            if (pl.IsClosed)
                            {
                                accum0.Add(arr1[^1]);
                                accum0.Add(arr1[0]);
                            }
                            segments.AddRange(accum0);

                        }
                        else if (d is HatchBoundaryPath.Line ll)
                        {
                            List<Vector2d> accum0 = new List<Vector2d>();

                            var verts = new netDxf.Vector2[] { ll.Start, ll.End };

                            var arr1 = verts.Select(z => new Vector2d(z.X, z.Y)).ToArray();

                            accum0.AddRange(arr1);
                            segments.AddRange(accum0);

                        }
                        else if (d is HatchBoundaryPath.Arc arc)
                        {
                            List<Vector2d> accum0 = new List<Vector2d>();

                            Arc arc1 = new Arc(arc.Center, arc.Radius, arc.StartAngle, arc.EndAngle);

                            var sweep = arc.EndAngle - arc.StartAngle;
                            if (sweep < 0)
                                sweep += 360;
                            var len = Math.PI * 2 * arc.Radius * (sweep / 360.0);
                            var qty = len / PolylinePrecisionDivider;
                            qty = qty < MinimalArcDivider ? MinimalArcDivider : qty;
                            var pl0 = arc1.ToPolyline2D((int)qty);
                            List<Vector2d> vvv = new List<Vector2d>();
                            if ((Math.Abs(sweep) - 360.0) < 1e-6)
                            {
                                Circle circle = new Circle(arc.Center, arc.Radius);
                                pl0 = circle.ToPolyline2D((int)qty);
                                var verts = pl0.Vertexes;
                                var list = verts.Select(z => new Vector2d(z.Position.X, z.Position.Y)).ToList();
                                list.Add(list[0]);
                                var arr1 = list.ToArray();
                                arr1 = StripToLines(arr1);
                                vvv = arr1.ToList();
                            }
                            else
                            {
                                var verts = pl0.Vertexes;
                                var list = verts.Select(z => new Vector2d(z.Position.X, z.Position.Y)).ToList();
                                if (pl0.IsClosed)
                                {
                                    list.Add(list[0]);
                                }
                                var arr1 = list.ToArray();
                                arr1 = StripToLines(arr1);
                                vvv = arr1.ToList();
                            }



                            accum0.AddRange(vvv.ToArray());
                            segments.AddRange(accum0);

                            //if (accum.Count > 10000)
                            //{
                            //    var arr1 = accum.ToArray();
                            //    PolylineGpuObject g = Create(arr1);
                            //    PolylineGpuMeshSceneObject sceneObject = new PolylineGpuMeshSceneObject(g) { Color = new Vector3d(255, 0, 0) }; 
                            //    sceneObject.CalcBbox(arr1);

                            //    accum.Clear();
                            //    toAdd.Add(sceneObject);
                            //}

                        }
                        else
                        {

                        }
                    }
                    foreach (var d in gitem.Entities)
                    {

                    }
                    if (FillHatchesOnLoad)
                    {
                        //2. try connect segments to closed loop
                        var segments0 = new List<Vector2d[]>();
                        for (int i = 1; i < segments.Count; i += 2)
                        {
                            segments0.Add([segments[i - 1], segments[i]]);
                        }
                        var contours = ConnectElements(segments0.ToArray());
                        if (contours.Length == 1 && contours[0].IsClosed())
                        {
                            // triagnulate of success
                            var tr = TrianglesGpuObject.TriangulateWithHoles([contours[0].Points.ToArray()], []);
                            HatchGpuMeshSceneObject s = null;
                            glControl.Invoke(() =>
                            {
                                s = new HatchGpuMeshSceneObject(new TrianglesGpuObject(tr.SelectMany(z => z).ToArray()))
                                {

                                    FillColor = new Vector3d(255, 128, 128),
                                    Color = new Vector3d(255, 0, 0)

                                };
                            });
                            s.CalcBbox(segments.ToArray());
                            toAdd.Add(s);
                        }
                        else
                        {
                            accum.AddRange(segments);

                        }
                    }
                    else
                    {
                        accum.AddRange(segments);

                    }
                    if (accum.Count > 10000)
                    {
                        PolylineGpuObject g = Create(accum.ToArray());
                        PolylineGpuMeshSceneObject sceneObject = new PolylineGpuMeshSceneObject(g) { Color = new Vector3d(255, 0, 0) };
                        sceneObject.CalcBbox(accum.ToArray());
                        accum.Clear();
                        vv.Clear();
                        toAdd.Add(sceneObject);
                    }
                }
            }

            if (accum.Any())
            {
                PolylineGpuObject g = Create(accum.ToArray());
                PolylineGpuMeshSceneObject sceneObject = new PolylineGpuMeshSceneObject(g) { Color = new Vector3d(255, 0, 0) };
                sceneObject.CalcBbox(accum.ToArray());
                accum.Clear();
                vv.Clear();
                toAdd.Add(sceneObject);
            }

            foreach (var item in doc.Entities.Circles)
            {
                progressValue = cnt++;


                var len = Math.PI * 2 * item.Radius;
                var qty = len / PolylinePrecisionDivider;
                qty = qty < MinimalCircleDivider ? MinimalCircleDivider : qty;
                var verts = item.ToPolyline2D((int)qty);
                var list = verts.Vertexes.Select(z => new Vector2d(z.Position.X, z.Position.Y)).ToList();
                list.Add(list[0]);
                var arr1 = list.ToArray();
                arr1 = StripToLines(arr1);
                PolylineGpuObject g = Create(arr1);
                PolylineGpuMeshSceneObject sceneObject = new PolylineGpuMeshSceneObject(g);
                sceneObject.CalcBbox(arr1);

                vv.Clear();
                toAdd.Add(sceneObject);
            }
            foreach (var item in doc.Entities.Arcs)
            {
                var sweep = item.EndAngle - item.StartAngle;
                if (sweep < 0)
                    sweep += 360;
                var len = Math.PI * 2 * item.Radius * (sweep / 360.0);
                var qty = len / PolylinePrecisionDivider;
                qty = qty < MinimalArcDivider ? MinimalArcDivider : qty;
                var pl = item.ToPolyline2D((int)qty);
                progressValue = cnt++;

                //if (cnt > 100000)
                //  break;

                var verts = pl.Vertexes;
                List<Vector2d> vvv = new List<Vector2d>();
                var pos = new Vector2d(verts[0].Position.X, verts[0].Position.Y);

                //if (first == null)
                //   first = pos;
                //  pos -= first.Value;
                vvv.Add(pos);
                for (int i = 1; i < verts.Count - 1; i++)
                {
                    netDxf.Entities.Polyline2DVertex vitem = verts[i];
                    pos = new Vector2d(vitem.Position.X, vitem.Position.Y);


                    vvv.Add(pos);
                    vvv.Add(pos);
                }
                vvv.Add(new Vector2d(verts[^1].Position.X, verts[^1].Position.Y));

                vv.AddRange(vvv.ToArray());
                if (vv.Count > 10000)
                {
                    var arr1 = vv.ToArray();
                    PolylineGpuObject g = Create(arr1);
                    PolylineGpuMeshSceneObject sceneObject = new PolylineGpuMeshSceneObject(g);
                    sceneObject.CalcBbox(arr1);

                    vv.Clear();
                    toAdd.Add(sceneObject);
                }


            }
            if (vv.Count > 0)
            {
                PolylineGpuObject g = Create(vv.ToArray());
                PolylineGpuMeshSceneObject sceneObject = new PolylineGpuMeshSceneObject(g);
                sceneObject.CalcBbox(vv.ToArray());

                vv.Clear();
                toAdd.Add(sceneObject);
            }

            foreach (var item in doc.Entities.Polylines2D)
            {
                progressValue = cnt++;

                var verts = item.Vertexes.ToArray();

                var arr1 = verts.Select(z => new Vector2d(z.Position.X, z.Position.Y)).ToList();
                if (item.IsClosed)
                {
                    arr1.Add(arr1[0]);
                }
                arr1 = StripToLines(arr1).ToList();
                accum.AddRange(arr1);
                if (accum.Count > 10000)
                {
                    PolylineGpuObject g = Create(accum.ToArray());
                    PolylineGpuMeshSceneObject sceneObject = new PolylineGpuMeshSceneObject(g);
                    sceneObject.CalcBbox(accum.ToArray());
                    accum.Clear();
                    vv.Clear();
                    toAdd.Add(sceneObject);
                }
            }
            foreach (var item in doc.Entities.Lines)
            {
                progressValue = cnt++;

                var verts = new netDxf.Vector3[] { item.StartPoint, item.EndPoint };

                var arr1 = verts.Select(z => new Vector2d(z.X, z.Y)).ToArray();
                accum.AddRange(arr1);
                if (accum.Count > 10000)
                {
                    PolylineGpuObject g = Create(accum.ToArray());
                    PolylineGpuMeshSceneObject sceneObject = new PolylineGpuMeshSceneObject(g);
                    sceneObject.CalcBbox(accum.ToArray());
                    accum.Clear();
                    vv.Clear();
                    toAdd.Add(sceneObject);
                }
            }
            if (accum.Count > 0)
            {
                PolylineGpuObject g = Create(accum.ToArray());
                PolylineGpuMeshSceneObject sceneObject = new PolylineGpuMeshSceneObject(g);
                sceneObject.CalcBbox(accum.ToArray());
                accum.Clear();
                vv.Clear();
                toAdd.Add(sceneObject);
            }
            Parts.AddRange(toAdd);
            dirty = true;
        }

        int progressValue = 0;
        private void UpdateProgressBar(int cnt, int total)
        {
            statusStrip1.Invoke(() =>
            {
                toolStripProgressBar1.Maximum = total;
                toolStripProgressBar1.Value = cnt;
            });
        }

        private Vector2d[] StripToLines(IReadOnlyList<Vector2d> arr1)
        {
            List<Vector2d> ret = new List<Vector2d>();
            ret.Add(arr1[0]);
            for (int i = 1; i < arr1.Count - 1; i++)
            {
                ret.Add(arr1[i]);
                ret.Add(arr1[i]);
            }
            ret.Add(arr1[^1]);
            return ret.ToArray();
        }

        double PolylinePrecisionDivider = 0.5;//mm        
        internal void SwitchColorTheme()
        {
            darkMode = !darkMode;
            dirty = true;
        }

        internal void Settings()
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddBoolField("drawAxes", "Draw axes", drawAxes);
            d.AddBoolField("DrawHatches", "Draw hatches", DrawHatches);
            d.AddBoolField("FillHatchesOnLoad", "FillHatchesOnLoad", FillHatchesOnLoad);
            d.AddOptionsField("HatchFillType", "Hatch fill type", ["solid", "hatch"], SolidHatchFilling ? 0 : 1);
            d.AddNumericField("PolylinePrecisionDivider", "PolylinePrecisionDivider", PolylinePrecisionDivider);

            if (!d.ShowDialog())
                return;

            DrawHatches = d.GetBoolField("DrawHatches");
            drawAxes = d.GetBoolField("drawAxes");
            FillHatchesOnLoad = d.GetBoolField("FillHatchesOnLoad");
            SolidHatchFilling = d.GetOptionsFieldIdx("HatchFillType") == 0;

            PolylinePrecisionDivider = d.GetNumericField("PolylinePrecisionDivider");
            dirty = true;
        }

        internal void Clear()
        {
            Parts.Clear();
            dirty = true;
        }

        public void FitToPoints(Vector2d[] pnts, Camera cam, float gap = 10)
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
            if (!pnts.Any())
                return;

            FitToPoints(pnts, camera1);
            dirty = true;
        }

        internal void Centrify()
        {
            List<Vector2d> centers = new List<Vector2d>();
            foreach (var item in Parts)
            {
                var center = new Vector2d();
                foreach (var pitem in item.BBox)
                {
                    center += pitem;
                }
                if (item.BBox.Length > 0)
                    center /= item.BBox.Length;
                centers.Add(center);

            }
            Vector2d center0 = new Vector2d();
            foreach (var item in centers)
            {
                center0 += item;
            }
            center0 /= centers.Count;
            foreach (var item in Parts)
            {
                item.Matrix = Matrix4d.CreateTranslation(-center0.X, -center0.Y, 0);

                // item.Offset = -new Vector2d(center0.X, center0.Y);
            }
            dirty = true;
        }

        internal async void ImportDxf()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            dirty = true;
            await LoadDxf(ofd.FileName);
            GC.Collect(2);
            GC.WaitForPendingFinalizers();
        }

        private async Task LoadDxf(string path)
        {
            netDxf.DxfDocument doc = null;
            toolStripStatusLabel1.Text = $"Loading : {path}";
            progressValue = 0;
            toolStripProgressBar1.Maximum = 100;
            toolStripProgressBar1.Value = 0;

            toolStripProgressBar1.Visible = true;
            Stopwatch sw = Stopwatch.StartNew();
            await Task.Run(() =>
            {

                doc = netDxf.DxfDocument.Load(path);
                AddDxf(doc);


            });
            sw.Stop();
            double ms = sw.ElapsedMilliseconds;
            toolStripProgressBar1.Visible = false;
            string end = "ms";
            if (ms > 1000)
            {
                ms /= 1000.0;
                end = "s";
            }
            toolStripStatusLabel1.Text = $"Done loading : {path}, loading time: {Math.Round(ms, 2)}{end}";
        }
    }
}
