using UnityEngine;

public class VRButtonAction_Toggle : MonoBehaviour
{
    public CameraScreenController screenController; // LCD ���� ��ũ��Ʈ
    public Texture defaultTexture; // LCD ������ �� �⺻ �̹���
    public Texture liveTexture;    // LCD ���� �� RenderTexture
    public float cooldown = 0.5f;

    private bool isOn = false; // LCD ���� ����
    private float lastPressTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            if (Time.time - lastPressTime < cooldown) return;
            lastPressTime = Time.time;

            if (isOn)
            {
                // LCD ����
                screenController.TurnOff(defaultTexture);
                isOn = false;
                Debug.Log("LCD Off");
            }
            else
            {
                // LCD �ѱ�
                screenController.ShowPhoto(liveTexture);
                isOn = true;
                Debug.Log("LCD On");
            }
        }
    }
}
