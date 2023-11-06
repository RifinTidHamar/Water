using UnityEngine;

public class DepthTextureRenderer : MonoBehaviour
{
    public Material depthMaterial; // Create a material using a shader that uses the depth texture.

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (depthMaterial != null)
        {
            Graphics.Blit(src, dest, depthMaterial);
        }
    }
}
