using UnityEngine;

public class VRButtonAction_Toggle : MonoBehaviour
{
    public CameraScreenController screenController; // ��ũ�� ��Ʈ�� ��ũ��Ʈ
    public Texture defaultTexture; // �⺻ �̹���
    public Texture photoTexture;   // �Կ� ȭ��
    public float cooldown = 0.5f;

    private bool isPhotoShown = false; // ���� ���� ����
    private float lastPressTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand")) // �հ����� Hand �±� �ٿ��� ��
        {
            if (Time.time - lastPressTime < cooldown) return;
            lastPressTime = Time.time;

            if (isPhotoShown)
            {
                // �⺻ �̹����� ����
                screenController.ShowPhoto(defaultTexture);
                isPhotoShown = false;
            }
            else
            {
                // �Կ� �̹����� ��ȯ
                screenController.ShowPhoto(photoTexture);
                isPhotoShown = true;
            }
        }
    }
}
