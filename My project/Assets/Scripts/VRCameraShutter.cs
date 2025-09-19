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

            // LCD가 꺼져 있으면 무시
            if (!screenController.IsOn())
            {
                Debug.Log("LCD Off → 촬영 불가");
                return;
            }

            Texture current = screenController.GetCurrentTexture();

            if (current is RenderTexture rt)
            {
                Texture2D snapshot = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                RenderTexture.active = rt;
                snapshot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                snapshot.Apply();
                RenderTexture.active = null;

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
