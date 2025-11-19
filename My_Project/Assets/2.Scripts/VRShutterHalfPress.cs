using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System.Collections;
using OVR;

public class VRShutterHalfPress : MonoBehaviour
{
    [Header("Trigger Zone")]
    public Collider halfPressZone;          // 반눌림 Zone (얕은 부분)
    public Transform lensOrigin;            // Raycast 기준점 (카메라 렌즈 위치)
    public float autofocusMaxDistance = 20f;

    [Header("Global Volume")]
    public Volume globalVolume;
    private DepthOfField dof;

    [Header("LCD / Screen State")]
    public CameraScreenController screenController; // ★ LCD 상태 확인용

    [Header("Feedback Settings")]
    public AudioSource sfx;
    public AudioClip afBeep;                // 초점 비프음
    public float hapticStrength = 0.3f;
    public float hapticDuration = 0.05f;

    [Header("LCD Focus UI")]
    public GameObject lcdFocusFrame;        // LCD 위에 올릴 초점 사각형(이미지/쿼드)
    public Text focusInfoText;              // 선택 : 거리 표시용 텍스트
    public float focusUiDuration = 0.7f;    // 프레임이 유지되는 시간(초)

    [Header("Debounce Settings")]
    public float cooldown = 0.2f;           // 반복 방지 시간 (초)
    private float lastFocusTime = -999f;

    private Coroutine focusUICoroutine;

    void Awake()
    {
        // Depth Of Field 가져오기
        if (!globalVolume.profile.TryGet(out dof))
            Debug.LogError("Global Volume에 Depth Of Field Override가 필요합니다.");

        if (halfPressZone == null)
            Debug.LogError("반눌림 Zone Collider를 할당하세요.");

        if (lensOrigin == null)
            Debug.LogWarning("lensOrigin이 비었습니다. Raycast 기준점 필요합니다.");

        // 시작 시 LCD UI 끄기
        if (lcdFocusFrame != null)
            lcdFocusFrame.SetActive(false);
        if (focusInfoText != null)
            focusInfoText.text = "";
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Hand")) return;
        if (Time.time - lastFocusTime < cooldown) return;

        // 반눌림 존 안에 손가락 콜라이더가 들어와 있을 때만
        if (halfPressZone != null && halfPressZone.bounds.Intersects(other.bounds))
        {
            lastFocusTime = Time.time;
            float dist = Autofocus();
            if (dist > 0f)
            {
                ShowFocusUI(dist);
                GiveFeedback();
            }
        }
    }

    /// <summary>
    /// 레이캐스트로 피사체 거리 측정 후 DoF 초점거리 세팅
    /// </summary>
    float Autofocus()
    {
        // 🔒 1) 전원 꺼져 있으면 AF 동작 X
        if (VRPowerManager.Instance == null || !VRPowerManager.Instance.IsPowerOn)
        {
            Debug.Log("[AF] Power OFF – ignore AF.");
            return 0f;
        }

        // 🔒 2) LCD 꺼져 있으면 AF 동작 X
        if (screenController == null || !screenController.IsOn())
        {
            Debug.Log("[AF] LCD OFF – ignore AF.");
            return 0f;
        }

        // 🔒 3) 현재 LCD가 RenderTexture(라이브뷰)일 때만 AF
        Texture currentTex = screenController.GetCurrentTexture();
        if (!(currentTex is RenderTexture))
        {
            Debug.Log("[AF] Not live view (no RenderTexture) – ignore AF.");
            return 0f;
        }

        if (dof == null || lensOrigin == null) return 0f;

        float focusedDistance;

        Ray ray = new Ray(lensOrigin.position, lensOrigin.forward);
        if (Physics.Raycast(ray, out var hit, autofocusMaxDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            focusedDistance = Mathf.Clamp(hit.distance, 0.2f, autofocusMaxDistance);
            dof.mode.value = DepthOfFieldMode.Bokeh;
            dof.focusDistance.value = focusedDistance;

            Debug.Log($"[AF] Focused at {focusedDistance:F2} m");
        }
        else
        {
            // 히트 안되면 기본 거리
            focusedDistance = 5f;
            dof.focusDistance.value = focusedDistance;
        }

        return focusedDistance;
    }

    /// <summary>
    /// LCD 위에 초점 프레임 & 거리 텍스트 잠깐 표시
    /// </summary>
    void ShowFocusUI(float distance)
    {
        if (lcdFocusFrame == null && focusInfoText == null) return;

        if (focusUICoroutine != null)
            StopCoroutine(focusUICoroutine);

        focusUICoroutine = StartCoroutine(FocusUIRoutine(distance));
    }

    IEnumerator FocusUIRoutine(float distance)
    {
        if (lcdFocusFrame != null)
            lcdFocusFrame.SetActive(true);

        if (focusInfoText != null)
            focusInfoText.text = $"{distance:F1} m";

        yield return new WaitForSeconds(focusUiDuration);

        if (lcdFocusFrame != null)
            lcdFocusFrame.SetActive(false);

        if (focusInfoText != null)
            focusInfoText.text = "";

        focusUICoroutine = null;
    }

    /// <summary>
    /// 비프 + 짧은 햅틱
    /// </summary>
    void GiveFeedback()
    {
        if (sfx != null && afBeep != null)
            sfx.PlayOneShot(afBeep);

        StartCoroutine(HapticOnce());
    }

    IEnumerator HapticOnce()
    {
        OVRInput.SetControllerVibration(1f, hapticStrength, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(hapticDuration);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
    }
}
