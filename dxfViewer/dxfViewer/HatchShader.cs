using OpenTK.Mathematics;
using System.IO;
using System.Reflection;

namespace dxfViewer
{
    public class HatchShader : Shader
    {
        public HatchShader()
        {
            InitFromResources("hatch.vs", "hatch.fs");
        }
        public void SetParams(float x, float y, float zoom, float[] clr)
        {

         

         
            /*

            GL.Uniform2(Unif_psize, new Vector2(x, y));
            GL.Uniform3(Unif_clr, clr[0], clr[1], clr[2]);
            setFloat()
            GL.Uniform1(Unif_zoom, zoom);*/
            /*GL.Uniform1(Unif_texSDF, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, txtr);*/


        }



    }
}
