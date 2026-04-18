using FxEngine;
using FxEngine.Cameras;
using OpenTK.Mathematics;

namespace dxfViewer
{
    public class DefaultCameraViewManager : CameraViewManager
    {
        public override void Update()
        {
            var dir = Camera.Eye - Camera.Target;
            var cv = dir;
            var moveVec = new Vector3d(cv.X, cv.Y, 0).Normalized();
            var a1 = Vector3d.Cross(Camera.Up, cv.Normalized()); ;
            //var moveVecTan = new Vector3(-moveVec.Y, moveVec.X, );
            var moveVecTan = a1.Normalized();
            moveVec = Vector3d.Cross(a1.Normalized(), cv.Normalized()).Normalized();


            var pos = CursorPosition;
            var zoom = 360f / (Camera.Eye - Camera.Target).Length;

            {
                if (drag2)
                {
                    zoom = Control.Width / Camera.OrthoWidth;

                    var dx = moveVecTan * ((startPosX - pos.X) / zoom) + moveVec * ((startPosY - pos.Y) / zoom);
                    Camera.Eye = cameraFromStart + dx;
                    Camera.Target = cameraToStart + dx;
                }
                if (false && drag)
                {
                    //rotate here
                    float kk = 3;
                    //cameraToStart = new Vector3(cameraToStart.X, 0, 0);
                    //Camera.CamTo = cameraToStart;
                    Vector3d v1 = cameraFromStart - cameraToStart;

                    var m1 = Matrix3d.CreateFromAxisAngle(Vector3d.Cross(v1, cameraUpStart), -(startPosY - pos.Y) / 180f / kk * (float)Math.PI);
                    //var m1 = Matrix3.CreateFromAxisAngle(Vector3.UnitX, -(startPosY - pos.Y) / 180f / kk * (float)Math.PI);
                    var m2 = Matrix3d.CreateFromAxisAngle(cameraUpStart, -(startPosX - pos.X) / 180f / kk * (float)Math.PI);
                    //var m2 = Matrix3.CreateFromAxisAngle(Vector3.UnitZ, -(startPosX - pos.X) / 180f / kk * (float)Math.PI);

                    v1 *= m1;
                    v1 *= m2;
                    var up1 = cameraUpStart;

                    //up1 *= m1;
                    //up1 *= m2;
                    //Camera.CamUp = up1;

                    Camera.Eye = cameraToStart + v1;
                    var dx = startPosX - pos.X;

                }

            }
        }

        public float AlongRotate = 0;
        public Camera Camera;
        public override void Attach(EventWrapperGlControl control, Camera camera)
        {
            base.Attach(control, camera);
            Camera = camera;
            control.MouseUpAction = Control_MouseUp;
            control.MouseDownAction = Control_MouseDown;
            control.KeyUpUpAction = Control_KeyUp;
            control.KeyDownAction = Control_KeyDown;
            control.MouseWheelAction = Control_MouseWheel;
        }

        private void Control_MouseWheel(object sender, MouseEventArgs e)
        {
            float zoomK = 20;
            var cur = Control.PointToClient(Cursor.Position);
            Control.MakeCurrent();
            //MouseRay.UpdateMatrices();
            MouseRay mr = new MouseRay(cur.X, cur.Y, Camera);
            //MouseRay mr0 = new MouseRay(Control.Width / 2, Control.Height / 2, Camera);

            var camera = Camera;
            if (camera.IsOrtho)
            {
                var shift = mr.Start - Camera.Eye;
                shift.Normalize();
                var old = camera.OrthoWidth / Control.Width;
                if (e.Delta > 0)
                {
                    camera.OrthoWidth /= 1.2f;
                    //var pxn = new Vector2(cur.X,cur.Y)-(new Vector2(Control.Width/2,Control.Height/2));
                    Camera cam2 = new Camera();
                    cam2.Eye = camera.Eye;
                    cam2.Target = camera.Target;
                    cam2.Up = camera.Up;
                    cam2.OrthoWidth = camera.OrthoWidth;
                    cam2.IsOrtho = camera.IsOrtho;

                    cam2.UpdateMatricies(Control.Size);
                    MouseRay mr2 = new MouseRay(cur.X, cur.Y, cam2);

                    //var a1 = pxn * camera.OrthoWidth / Control.Width;
                    var diff = mr.Start - mr2.Start;


                    shift *= diff.Length;
                    camera.Eye += shift;
                    camera.Target += shift;
                }
                else
                {
                    camera.OrthoWidth *= 1.2f;
                    /*var pxn = new Vector2(cur.X, cur.Y) - (new Vector2(Control.Width / 2, Control.Height / 2));

                    var a1 = pxn * camera.OrthoWidth / Control.Width;*/
                    Camera cam2 = new Camera();
                    cam2.Eye = camera.Eye;
                    cam2.Target = camera.Target;
                    cam2.Up = camera.Up;
                    cam2.OrthoWidth = camera.OrthoWidth;
                    cam2.IsOrtho = camera.IsOrtho;

                    cam2.UpdateMatricies(Control.Size);
                    MouseRay mr2 = new MouseRay(cur.X, cur.Y, cam2);

                    var diff = mr.Start - mr2.Start;
                    shift *= diff.Length;
                    camera.Eye -= shift;
                    camera.Target -= shift;
                }

                return;
            }
            if (
                Control.ClientRectangle.IntersectsWith(new Rectangle(Control.PointToClient(Cursor.Position),
                    new System.Drawing.Size(1, 1))))
            {
                var dir = mr.Dir;
                dir.Normalize();
                if (e.Delta > 0)
                {
                    camera.Eye += dir * zoomK;
                    camera.Target += dir * zoomK;
                }
                else
                {
                    camera.Eye -= dir * zoomK;
                    camera.Target -= dir * zoomK;
                }
            }
        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift)
            {
                lshift = true;
            }
        }

        private void Control_KeyUp(object sender, KeyEventArgs e)
        {
            lshift = false;
        }

        

        public virtual void Control_MouseDown(object sender, MouseEventArgs e)
        {
            Control.MakeCurrent();

            var pos = CursorPosition;
            startPosX = pos.X;
            startPosY = pos.Y;
            cameraFromStart = Camera.Eye;
            cameraToStart = Camera.Target;
            cameraUpStart = Camera.Up;

            if (e.Button == MouseButtons.Right)
            {

                var mr = new MouseRay(pos.X, pos.Y, Camera);
                var d1 = Camera.Eye - Camera.Target;

                //var plane1 : forw
                var crs1 = Vector3d.Cross(cameraUpStart, d1);
                var z1 = Vector3d.UnitZ;



                drag = true;


            }

            if (e.Button == MouseButtons.Left)
            {
                drag2 = true;
                //Camera.CamTo=Drawer.tubec
            }
        }

        protected bool lshift = false;
        protected float startShiftX;
        protected float startShiftY;
        protected float startPosX;
        protected float startPosY;
        protected Vector3d cameraFromStart;
        protected Vector3d cameraToStart;
        protected Vector3d cameraUpStart;
        public PointF CursorPosition
        {
            get
            {
                return Control.PointToClient(Cursor.Position);
            }
        }
        protected bool drag = false;
        protected bool drag2 = false;

        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
            drag2 = false;
        }
    }
}
