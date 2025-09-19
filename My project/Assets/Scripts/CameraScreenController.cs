using UnityEngine;

public class CameraScreenController : MonoBehaviour
{
    public MeshRenderer screenRenderer;

    private Texture currentTexture;
    private bool isOn = false; // LCD ÄÑÁü ¿©ºÎ

    public void ShowPhoto(Texture tex)
    {
        screenRenderer.material.mainTexture = tex;
        currentTexture = tex;
        isOn = true;
    }

    public void TurnOff(Texture defaultTex)
    {
        screenRenderer.material.mainTexture = defaultTex;
        currentTexture = null;
        isOn = false;
    }

    public Texture GetCurrentTexture()
    {
        return currentTexture;
    }

    public bool IsOn()
    {
        return isOn;
    }
}
