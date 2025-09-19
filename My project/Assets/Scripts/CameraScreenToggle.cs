using UnityEngine;

public class CameraScreenToggle : MonoBehaviour
{
    public MeshRenderer lcdRenderer;       // DSLR LCD MeshRenderer
    public Texture defaultTexture;         // �⺻ ȭ�� (���� ���� �̹���)
    public RenderTexture liveTexture;      // DSLR ī�޶� RenderTexture
    public float cooldown = 0.5f;
    private bool isOn = false;
    private float lastPressTime = 0f;
    public bool IsOn => isOn;  // ���� ��ư���� ���� ����

    public void ToggleLCD()
    {
        isOn = !isOn;

        if (Time.time - lastPressTime < cooldown) return;
        lastPressTime = Time.time;
        if (isOn)
        {
            lcdRenderer.material.mainTexture = liveTexture;
            Debug.Log(" LCD ON: �ǽð� ������ ǥ��");
        }
        else
        {
            lcdRenderer.material.mainTexture = defaultTexture;
            Debug.Log(" LCD OFF: �⺻ ȭ������ ��ȯ");
        }
    }
}
