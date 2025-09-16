using UnityEngine;

public class ShowCameraScreen : MonoBehaviour
{
    public GameObject screenQuad;
    private bool isVisible = false;

    public void ToggleScreen()
    {
        isVisible = !isVisible;
        screenQuad.SetActive(isVisible);
    }
}