using UnityEngine;
using System.Collections;

public class VRButtonAction_Snapshot : MonoBehaviour
{
    [Header("Screen References")]
    public CameraScreenController screenController; // LCD Controller
    public MeshRenderer worldQuadRenderer;          // World Quad
    public MeshRenderer screenRenderer;             // LCD Screen (2D Surface)

    [Header("Textures")]
    public Texture defaultWorldTexture;             // Default texture on world screen
    public Texture defaultLCDTexture;               // Default texture when LCD idle

    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip shutterSound;

    [Header("Settings")]
    public float cooldown = 0.5f;
    public float lcdPhotoDuration = 2f;
    public float flashDuration = 0.1f;

    private float lastPressTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
     if (!other.CompareTag("Hand")) return;

        // 쿨다운 방지
        if (Time.time - lastPressTime < cooldown) return;
        lastPressTime = Time.time;

        // 전원 꺼져 있으면 촬영 불가
        if (!VRPowerManager.Instance.IsPowerOn)
        {
            Debug.Log("[Snapshot] Power OFF — capture ignored.");
            return;
        }

        // 셔터 사운드
        if (audioSource != null && shutterSound != null)
            audioSource.PlayOneShot(shutterSound, 0.8f);

        // 현재 LCD가 켜져 있는지 확인
        if (!screenController.IsOn())
        {
            Debug.Log("[Snapshot] LCD is OFF — cannot capture.");
            return;
        }

        // 현재 화면 텍스처 가져오기
        Texture current = screenController.GetCurrentTexture();

        if (current is RenderTexture rt)
        {
            int width = rt.width;
            int height = rt.height;
            Texture2D snapshot = new Texture2D(width, height, TextureFormat.RGBA32, false, true);

            RenderTexture.active = rt;
            snapshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            snapshot.Apply();
            RenderTexture.active = null;

            snapshot.filterMode = FilterMode.Point;
            snapshot.wrapMode = TextureWrapMode.Clamp;

            // World Quad와 LCD 표시
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
    private IEnumerator ShowPhotoOnLCD(Texture snapshot)
    {
        // 플래시 효과 (촬영 순간)
        StartCoroutine(LCDFlashEffect());

        // LCD에 사진 표시
        screenRenderer.material.mainTexture = snapshot;

        // 2초간 유지
        yield return new WaitForSeconds(lcdPhotoDuration);

        // 기본 화면 복귀
        screenRenderer.material.mainTexture = defaultLCDTexture;
    }
    
        private IEnumerator LCDFlashEffect()
    {
        if (screenRenderer == null) yield break;

        // 원래 색상 백업
        Material mat = screenRenderer.material;
        Color originalColor = mat.color;

        // 밝게 (플래시)
        mat.color = Color.white;
        yield return new WaitForSeconds(flashDuration);

        // 원래 색상 복원
        mat.color = originalColor;
    }
}
