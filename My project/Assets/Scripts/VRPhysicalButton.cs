using UnityEngine;

public class VRPhysicalButton : MonoBehaviour
{
    public CameraScreenController screenController; // 화면 제어 스크립트
    public Texture photoTexture;
    private bool isPhotoShown = false;
    private bool isPressed = false;

    private void OnTriggerEnter(Collider other)
    {
        // 손가락 콜라이더가 Button에 닿으면
        if (other.CompareTag("Hand") && !isPressed)
        {
            isPressed = true;
            ToggleScreen();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 손가락이 범위를 벗어나면 다시 누를 수 있도록 초기화
        if (other.CompareTag("Hand"))
        {
            isPressed = false;
        }
    }

    private void ToggleScreen()
    {
        if (isPhotoShown)
        {
            screenController.ResetScreen();
            isPhotoShown = false;
        }
        else
        {
            screenController.ShowPhoto(photoTexture);
            isPhotoShown = true;
        }
    }
}
