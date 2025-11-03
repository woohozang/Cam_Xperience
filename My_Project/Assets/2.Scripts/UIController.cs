using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using OVR;
using System.Collections;

public class UIController : MonoBehaviour
{
    [Header("UI")]
    public Canvas worldCanvas;
    public Slider focusSlider;

    [Header("Physical Camera")]
    // [추가] 렌더 텍스처를 출력하는 카메라 (예: Camera (1))
    public Camera physicalCamera;

    [Header("PostProcess")]
    public Volume globalVolume;
    private DepthOfField dof;
    // private ColorAdjustments color; // [삭제] 더 이상 수동으로 제어하지 않음

    // [삭제] Exposure Settings 헤더 및 변수들 삭제

    [Header("MoveHaptic")]
    public float hapticSensitivity = 1.5f;
    public float hapticDuration = 0.05f;

    private float lastSliderValue;
    private Coroutine hapticCoroutine;

    void Awake()
    {
        // [추가] Physical Camera 참조 확인
        if (physicalCamera == null)
        {
            Debug.LogError("Physical Camera 참조가 필요합니다! 렌더 텍스처를 출력하는 카메라를 할당해주세요.");
        }

        // Depth of Field 가져오기 (초점 거리 조절을 위해 여전히 필요)
        if (!globalVolume.profile.TryGet(out dof))
            Debug.LogError(" Global Volume에 Depth Of Field Override가 필요합니다.");

        // [삭제] Color Adjustments 가져오는 코드 삭제
        /*
        if (!globalVolume.profile.TryGet(out color))
            Debug.LogError(" Global Volume에 Color Adjustments Override가 필요합니다.");
        */

        // 슬라이더 이벤트 연결
        if (focusSlider != null)
        {
            focusSlider.onValueChanged.AddListener(OnSliderChanged);
            lastSliderValue = focusSlider.value;
        }

        // 시작 시 UI 비활성화
        if (worldCanvas != null)
            worldCanvas.gameObject.SetActive(false);
    }

    public void Toggle()
    {
        bool isActive = !worldCanvas.gameObject.activeSelf;
        worldCanvas.gameObject.SetActive(isActive);
        Debug.Log($"[DialUIController] World UI {(isActive ? "ON" : "OFF")}");
    }

    private void OnSliderChanged(float value)
    {
        if (dof == null) return;

        // --- DoF 및 Physical Camera 조절 (수정됨) ---

        dof.mode.value = DepthOfFieldMode.Bokeh;
        float reversed = 1f - value;

        // 1. 초점 거리는 Volume의 DoF에서 직접 제어
        dof.focusDistance.value = Mathf.Lerp(0.3f, 8f, reversed);

        if (physicalCamera != null)
        {
            // 2. 조리개 값은 Physical Camera 컴포넌트에서 제어
            // 이 값이 변경되면, (HDR/Tonemapping 설정이 올바르다면) 노출이 자동으로 변경됩니다.
            physicalCamera.aperture = Mathf.Lerp(16f, 1.4f, reversed);
        }

        // === Exposure 조절 (삭제됨) ===
        // [기존 color.postExposure 수동 조작 코드 전체 삭제]
        // (HDR, Tonemapping, Physical Camera 자동 노출 시스템이 이 작업을 대신 함)

        // --- 햅틱 조절 (기존과 동일) ---
        float speed = 0f;
        if (Time.deltaTime > 0)
        {
            speed = Mathf.Abs(value - lastSliderValue) / Time.deltaTime;
        }
        float dynamicAmplitude = Mathf.Clamp01(speed * hapticSensitivity);

        if (hapticCoroutine != null)
        {
            StopCoroutine(hapticCoroutine);
        }
        hapticCoroutine = StartCoroutine(VibrateForDuration(hapticDuration, dynamicAmplitude));

        lastSliderValue = value;
    }

    private IEnumerator VibrateForDuration(float duration, float amplitude)
    {
        OVRInput.SetControllerVibration(1, amplitude, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        hapticCoroutine = null;
    }

    // --- [새 함수 추가] ---
    // 다른 스크립트(예: 다이얼 애니메이터)에서 이 함수들을 호출하여
    // ISO와 Shutter Speed를 제어할 수 있습니다.

    /// <summary>
    /// Physical Camera의 ISO 값을 설정합니다.
    /// </summary>
    /// <param name="isoValue">ISO 값 (예: 100, 200, 400)</param>
    public void SetISO(float isoValue)
    {
        if (physicalCamera == null) return;
        physicalCamera.iso = (int)isoValue;
        Debug.Log($"Physical Camera ISO set to: {isoValue}");
    }

    /// <summary>
    /// Physical Camera의 Shutter Speed 값을 설정합니다. (1/n 초 단위)
    /// </summary>
    /// <param name="shutterValue">셔터 스피드 분모 값 (예: 1000 -> 1/1000s)</param>
    public void SetShutterSpeed(float shutterValue)
    {
        if (physicalCamera == null) return;
        // 셔터 스피드는 1/n 초 단위입니다.
        physicalCamera.shutterSpeed = shutterValue;
        Debug.Log($"Physical Camera Shutter Speed set to: 1/{shutterValue}s");
    }
}