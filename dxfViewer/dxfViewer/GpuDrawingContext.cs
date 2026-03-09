using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace dxfViewer
{
    public class GpuDrawingContext
    {
        public GpuDrawingContext()
        {

        }
        public Camera Camera;

        public Control Control;

        public Color ModelColor = Color.FromArgb(255, 128, 64);
        public Color HatchColor = Color.FromArgb(255, 0, 0);


        public TextRenderer TextRenderer;

        public Shader ModelShader;
        public HatchShader HatchShader;

        public void SetHatchShader()
        {
            CurrentShader = HatchShader;
            HatchShader.use();
            // be sure to activate shader when setting uniforms/drawing objects
            //   ModelShader.use();


            HatchShader.SetColor(HatchColor.ToVector3());


            SetCameraToShader();
        }

        public void SetCameraToShader()
        {

            // view/projection transformations       
            CurrentShader.setMat4("projection", Camera.ProjectionMatrix);
            CurrentShader.setMat4("view", Camera.ViewMatrix);

            // world transformation
            Matrix4 model = Matrix4.Identity;
            CurrentShader.setMat4("model", model);
        }
        public Shader CurrentShader;
        public void SetModelShader()
        {
            CurrentShader = ModelShader;
            ModelShader.use();
            // be sure to activate shader when setting uniforms/drawing objects
            //   ModelShader.use();


            // light properties
            Vector3 color = new Vector3
            {

                X = ModelColor.R / 255.0f,
                Y = ModelColor.G / 255.0f,
                Z = ModelColor.B / 255.0f
            };


            ModelShader.setVec3("color", color);


            //ResetShader();
            SetCameraToShader();
        }

        internal void ResetShader()
        {
            GL.UseProgram(0);
        }
    }
}
