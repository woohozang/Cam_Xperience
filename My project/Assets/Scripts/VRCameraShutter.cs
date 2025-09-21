using UnityEngine;

public class VRButtonAction_Snapshot : MonoBehaviour
{
    public CameraScreenController screenController; // DSLR LCD
    public MeshRenderer worldQuadRenderer;          // 월드에 띄울 Quad
    public Texture defaultWorldTexture;             // 사진 없을 때 기본 이미지
    public float cooldown = 0.5f;

    private float lastPressTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            if (Time.time - lastPressTime < cooldown) return;
            lastPressTime = Time.time;

            // LCD 꺼져 있으면 무시
            if (!screenController.IsOn())
            {
                Debug.Log("LCD Off → 촬영 불가");
                return;
            }

            Texture current = screenController.GetCurrentTexture();

            if (current is RenderTexture rt)
            {
                //  해상도 높이기 (기본 rt 크기보다 더 크게도 가능)
                int width = rt.width;
                int height = rt.height;

                Texture2D snapshot = new Texture2D(width, height, TextureFormat.RGB24, false, false);

                RenderTexture.active = rt;
                snapshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                snapshot.Apply();
                RenderTexture.active = null;

                //  텍스처 선명도 / 번짐 방지 설정
                snapshot.filterMode = FilterMode.Point;   // 픽셀 그대로
                snapshot.wrapMode = TextureWrapMode.Clamp;

                RenderTexture.active = null;

                // Quad에 적용
                worldQuadRenderer.material.mainTexture = snapshot;
            }
            else if (current != null)
            {
                worldQuadRenderer.material.mainTexture = current;
            }
            else
            {
                worldQuadRenderer.material.mainTexture = defaultWorldTexture;
            }
        }
    }
}
