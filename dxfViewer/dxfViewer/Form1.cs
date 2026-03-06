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
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.LightBlue);
            GL.Vertex3(-glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Vertex3(glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Color3(Color.AliceBlue);
            GL.Vertex3(glControl.Width / 2, glControl.Height / 2, zz);
            GL.Vertex3(-glControl.Width / 2, glControl.Height, zz);
            GL.End();

            GL.PushMatrix();
            GL.Translate(camera1.viewport[2] / 2 - 50, -camera1.viewport[3] / 2 + 50, 0);
            GL.Scale(0.5, 0.5, 0.5);

            var mtr = camera1.ViewMatrix;
            var q = mtr.ExtractRotation();
            var mtr3 = Matrix4.CreateFromQuaternion(q);
            GL.MultMatrix(ref mtr3);
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

            GL.Color3(Color.Blue);
            GL.Enable(EnableCap.Lighting);
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
                camera1 = new Camera() { IsOrtho = true };
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
    }
}
