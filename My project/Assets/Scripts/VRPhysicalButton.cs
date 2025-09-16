using UnityEngine;

public class VRPhysicalButton : MonoBehaviour
{
    public CameraScreenController screenController; // ȭ�� ���� ��ũ��Ʈ
    public Texture photoTexture;
    private bool isPhotoShown = false;
    private bool isPressed = false;

    private void OnTriggerEnter(Collider other)
    {
        // �հ��� �ݶ��̴��� Button�� ������
        if (other.CompareTag("Hand") && !isPressed)
        {
            isPressed = true;
            ToggleScreen();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // �հ����� ������ ����� �ٽ� ���� �� �ֵ��� �ʱ�ȭ
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
