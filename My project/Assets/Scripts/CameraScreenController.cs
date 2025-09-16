using UnityEngine;

public class CameraScreenController : MonoBehaviour
{
    [Header("LCD Mesh Renderer")]
    public MeshRenderer screenRenderer;

    [Header("기본 텍스처")]
    public Texture defaultTexture;

    public void ShowPhoto(Texture photoTex)
    {
        if (screenRenderer != null)
        {
            screenRenderer.material.mainTexture = photoTex;
        }
    }

    public void ResetScreen()
    {
        if (screenRenderer != null)
        {
            screenRenderer.material.mainTexture = defaultTexture;
        }
    }
}
