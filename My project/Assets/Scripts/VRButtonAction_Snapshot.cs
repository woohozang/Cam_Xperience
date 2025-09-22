using UnityEngine;
using System.Collections;

public class VRButtonAction_Snapshot : MonoBehaviour
{
    public CameraScreenController screenController; // DSLR LCD
    public MeshRenderer worldQuadRenderer;          // ���忡 ��� Quad
    public MeshRenderer ScreenRenderer; //��ũ���� 2�ʵ��� ���
    public Texture defaultWorldTexture;             // ���� ���� �� �⺻ �̹���
    public Texture defaultLCDTexture;
    public float cooldown = 0.5f;
    public float lcdPhotoDuration = 2f;

    private float lastPressTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            if (Time.time - lastPressTime < cooldown) return;
            lastPressTime = Time.time;

            // LCD ���� ������ ����
            if (!screenController.IsOn())
            {
                Debug.Log("LCD Off �� �Կ� �Ұ�");
                return;
            }

            Texture current = screenController.GetCurrentTexture();

            if (current is RenderTexture rt)
            {
                //  �ػ� ���̱� (�⺻ rt ũ�⺸�� �� ũ�Ե� ����)
                int width = rt.width;
                int height = rt.height;

                Texture2D snapshot = new Texture2D(width, height, TextureFormat.RGB24, false, false);

                RenderTexture.active = rt;
                snapshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                snapshot.Apply();
                RenderTexture.active = null;

                //  �ؽ�ó ���� / ���� ���� ����
                snapshot.filterMode = FilterMode.Point;   // �ȼ� �״��
                snapshot.wrapMode = TextureWrapMode.Clamp;

                RenderTexture.active = null;

                // Quad�� ����
                worldQuadRenderer.material.mainTexture = snapshot;
                StartCoroutine(ShowPhotoOnLCD(snapshot));
            }
            else if (current != null)
            {
                worldQuadRenderer.material.mainTexture = current;
                StartCoroutine(ShowPhotoOnLCD(current));
            }
            else
            {
                worldQuadRenderer.material.mainTexture = defaultWorldTexture;
            }
        }
    }
    private IEnumerator ShowPhotoOnLCD(Texture snapshot)
    {
        // ���� ���
        ScreenRenderer.material.mainTexture = snapshot;

        // 2�� ����
        yield return new WaitForSeconds(lcdPhotoDuration);

        // �⺻ �̹����� ���� (LCD ���� ����)
        ScreenRenderer.material.mainTexture = defaultLCDTexture;
    }
}
