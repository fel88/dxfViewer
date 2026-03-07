using OpenTK.Mathematics;

namespace dxfViewer
{
    public class GpuDrawingContext
    {
        public Camera Camera;
        
        public Color ModelColor = Color.FromArgb(255, 128, 64);
        Color LightColor = Color.FromArgb(255, 255, 255);
        

        public TextRenderer TextRenderer;

        public Shader ModelShader;
        public void SetModelShader()
        {
            ModelShader.use();
            // be sure to activate shader when setting uniforms/drawing objects
            ModelShader.use();
            
            ModelShader.setVec3("viewPos", Camera.CameraFrom);

            // light properties
            Vector3 color = new Vector3
            {

                X = ModelColor.R / 255.0f,
                Y = ModelColor.G / 255.0f,
                Z = ModelColor.B / 255.0f
            };


            ModelShader.setVec3("color", color);            

            
            // view/projection transformations       
            ModelShader.setMat4("projection", Camera.ProjectionMatrix);
            ModelShader.setMat4("view", Camera.ViewMatrix);

            // world transformation
            Matrix4 model = Matrix4.Identity;
            ModelShader.setMat4("model", model);
        }
    }
}
