using UnityEngine;
using System.Collections;
using UnityEngine.Rendering; // AsyncGPUReadback을 위해 추가

public class DevelopSnapshot : MonoBehaviour
{
    [Header("Screen References")]
    public CameraScreenController screenController; // LCD Controller
    [Tooltip("스냅샷을 렌더링할 원본 카메라 (라이브 피드를 생성하는 카메라)")]
    public Camera renderingCamera; // 고해상도 캡처를 위해 렌더링할 카메라 참조
    public MeshRenderer worldQuadRenderer;          // World Quad
    public MeshRenderer screenRenderer;             // LCD Screen (2D Surface)

    [Header("Textures")]
    public Texture defaultWorldTexture;             // Default texture on world screen
    public Texture defaultLCDTexture;               // Default texture when LCD idle

    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip shutterSound;

    [Header("Settings")]
    [Tooltip("고해상도 스냅샷 너비")]
    public int snapshotWidth = 1920;
    [Tooltip("고해상도 스냅샷 높이")]
    public int snapshotHeight = 1080;
    [Tooltip("고해상도 스냅샷 안티 앨리어싱 (1, 2, 4, 8)")]
    public int snapshotAntiAliasing = 4;
    public float cooldown = 1.0f;
    public float lcdPhotoDuration = 2f;
    public float flashDuration = 0.1f;

    private float lastPressTime = 0f;
    private bool isCapturing = false; // 중복 캡처 방지 플래그
    private Texture2D lastSnapshotTexture = null; // 이전 스냅샷 텍스처 메모리 관리를 위해 추가

    private void OnTriggerEnter(Collider other)
    {
        // ... 기존 코드 ...
        if (!other.CompareTag("Hand")) return;

        // 쿨다운 및 중복 실행 방지
        if (Time.time - lastPressTime < cooldown) return;
        if (isCapturing) return; // 이미 캡처 중이면 반환
        lastPressTime = Time.time;

        // 전원 꺼져 있으면 촬영 불가
        if (!VRPowerManager.Instance.IsPowerOn)
        {
            Debug.Log("[Snapshot] Power OFF — capture ignored.");
            return;
        }

        // 현재 LCD가 켜져 있는지 확인
        if (!screenController.IsOn())
        {
            Debug.Log("[Snapshot] LCD is OFF — cannot capture.");
            return;
        }

        // 렌더링 카메라가 할당되었는지 확인
        if (renderingCamera == null)
        {
            Debug.LogError("[Snapshot] Rendering Camera가 할당되지 않았습니다. 고해상도 캡처가 불가능합니다.");
            return;
        }

        // 셔터 사운드
        if (audioSource != null && shutterSound != null)
            audioSource.PlayOneShot(shutterSound, 0.8f);

        // 고해상도 캡처 코루틴 시작
        StartCoroutine(CaptureHighResSnapshot());
    }

    /// <summary>
    /// 고해상도 스냅샷을 캡처하고 비동기식으로 읽어옵니다. (VR 스터터링 방지)
    /// </summary>
    private IEnumerator CaptureHighResSnapshot()
    {
        isCapturing = true;

        // 1. 현재 카메라의 원본 RenderTexture 가져오기 (포맷 정보 등 참조용)
        Texture currentTexture = screenController.GetCurrentTexture();
        if (!(currentTexture is RenderTexture rt))
        {
            Debug.LogError("[Snapshot] 현재 텍스처가 RenderTexture가 아닙니다.");
            isCapturing = false;
            yield break;
        }

        // --- 수정된 부분 1: 렌더 텍스처 포맷 결정 ---
        // 카메라가 HDR을 지원하면 DefaultHDR (보통 ARGBHalf)을, 아니면 Default (RGBA32)를 사용합니다.
        RenderTextureFormat format = renderingCamera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;

        // 2. 고해상도 임시 RenderTexture 생성 (포맷은 원본과 맞추고, AA 적용)
        // RenderTexture highResRT = RenderTexture.GetTemporary(snapshotWidth, snapshotHeight, 24, rt.format); // 기존 코드
        RenderTexture highResRT = RenderTexture.GetTemporary(snapshotWidth, snapshotHeight, 24, format); // 수정된 코드
        highResRT.antiAliasing = Mathf.Clamp(snapshotAntiAliasing, 1, 8); // 1, 2, 4, 8 값만 유효

        // ... 기존 코드 ...
        // 3. 카메라의 타겟을 임시 RT로 변경하고 수동 렌더링
        RenderTexture originalTarget = renderingCamera.targetTexture;
        renderingCamera.targetTexture = highResRT;
        renderingCamera.Render();
        renderingCamera.targetTexture = originalTarget; // 렌더링 후 즉시 원상 복구

        // 4. 비동기 GPU Readback 요청
        // Readback 완료 시 OnReadbackComplete 콜백 함수 실행
        AsyncGPUReadback.Request(highResRT, 0, request => OnReadbackComplete(request, highResRT, format)); // 포맷 정보 전달

        // 코루틴은 즉시 종료. 실제 처리는 콜백(OnReadbackComplete)에서 이어짐
        yield return null;
    }

    /// <summary>
    /// AsyncGPUReadback이 완료되면 호출되는 콜백 (메인 스레드에서 실행됨)
    /// </summary>
    private void OnReadbackComplete(AsyncGPUReadbackRequest request, RenderTexture tempRT, RenderTextureFormat sourceFormat)
    {
        if (request.hasError)
        {
            Debug.LogError("[Snapshot] GPU Readback 실패!");
        }
        else if (request.done)
        {
            // ... 기존 코드 ...
            // 1. 이전 스냅샷 텍스처가 있다면 메모리 해제 (메모리 누수 방지)
            if (lastSnapshotTexture != null)
            {
                Destroy(lastSnapshotTexture);
            }

            // --- 수정된 부분 2: Texture2D 포맷 결정 ---
            // 렌더 텍스처 포맷에 맞춰 Texture2D 포맷을 선택합니다.
            // DefaultHDR은 보통 RGBAHalf에 해당합니다.
            TextureFormat snapshotFormat;
            if (sourceFormat == RenderTextureFormat.DefaultHDR)
            {
                snapshotFormat = TextureFormat.RGBAHalf; // 16-bit float (HDR)
            }
            else
            {
                snapshotFormat = TextureFormat.RGBA32; // 8-bit (SDR)
            }

            // 2. 새 Texture2D 생성 및 GPU 데이터 로드
            // Texture2D snapshot = new Texture2D(request.width, request.height, TextureFormat.RGBA32, false, true); // 기존 코드
            Texture2D snapshot = new Texture2D(request.width, request.height, snapshotFormat, false, true); // 수정된 코드 (true = linear color space)

            snapshot.LoadRawTextureData(request.GetData<byte>());
            snapshot.Apply(false, false); // 밉맵 X, CPU에서 다시 읽을 수 없음 (성능 최적화)

            // 3. 화질 개선을 위해 필터 모드 변경
            snapshot.filterMode = FilterMode.Bilinear; // Point 대신 Bilinear 사용
            snapshot.wrapMode = TextureWrapMode.Clamp;

            // ... 기존 코드 ...
            lastSnapshotTexture = snapshot; // 새 텍스처를 마지막 텍스처로 저장

            // 4. World Quad와 LCD에 결과물 적용
            worldQuadRenderer.material.mainTexture = snapshot;
            StartCoroutine(ShowPhotoOnLCD(snapshot));
        }

        // ... 기존 코드 ...
        // 5. 사용이 끝난 임시 RenderTexture 해제
        RenderTexture.ReleaseTemporary(tempRT);

        // 6. 캡처 플래그 해제
        isCapturing = false;
    }

    private IEnumerator ShowPhotoOnLCD(Texture snapshot)
    {
        // ... 기존 코드 ...
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
        // ... (기존 코드와 동일) ...
        // ... 기존 코드 ...
        if (screenRenderer == null) yield break;

        // 원래 색상 백업
        Material mat = screenRenderer.material;
        Color originalColor = mat.color;

        // ... 기존 코드 ...
        // 밝게 (플래시)
        mat.color = Color.white;
        yield return new WaitForSeconds(flashDuration);

        // 원래 색상 복원
        mat.color = originalColor;
    }

    // 게임 오브젝트 파괴 시 마지막 텍스처도 파괴하여 메모리 누수 방지
    private void OnDestroy()
    {
        // ... 기존 코드 ...
        if (lastSnapshotTexture != null)
        {
            Destroy(lastSnapshotTexture);
        }
    }
}