using UnityEngine;

public class RenderTextureSaver : MonoBehaviour
{
    public RenderTexture renderTexture;

    void SaveRenderTextureToFile()
    {
        // Ensure the render texture is active
        RenderTexture.active = renderTexture;

        // Create a new texture with the same dimensions
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);

        // Read the pixels from the render texture into the new texture
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

        // Apply the changes
        texture.Apply();

        // Encode the texture to a PNG file
        byte[] bytes = texture.EncodeToPNG();

        // Save the PNG file
        System.IO.File.WriteAllBytes("Assets/Texture/MyRenderTexture.png", bytes);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveRenderTextureToFile();
        }
    }
}
