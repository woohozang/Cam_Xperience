using UnityEngine;

public class CameraScreenController : MonoBehaviour
{
    [Header("LCD Mesh Renderer")]
    public MeshRenderer screenRenderer;

    [Header("�⺻ �ؽ�ó")]
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
