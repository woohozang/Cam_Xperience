using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using OVR;
using System.Collections;

public class DialUIController : MonoBehaviour
{
    [Header("UI")]
    public Canvas worldCanvas;
    public Slider focusSlider;

    [Header("PostProcess")]
    public Volume globalVolume;
    private DepthOfField dof;
    private ColorAdjustments color;

    [Header("Exposure Settings")]
    public float baselineAperture = 2.8f; // 기준 조리개값
    public float exposureMultiplier = 1.0f; // 밝기 민감도 조정용

    [Header("MoveHaptic")]
    public float hapticSensitivity = 1.5f; // [추가] 속도 대비 햅틱 민감도
    public float hapticDuration = 0.05f; // [추가] 햅틱 1회당 지속시간 (짧게)

    private float lastSliderValue; // [추가] 이전 프레임의 슬라이더 값
    private Coroutine hapticCoroutine; // [추가] 햅틱 코루틴 제어용



    void Awake()
    {
        // Depth of Field 가져오기
        if (!globalVolume.profile.TryGet(out dof))
            Debug.LogError(" Global Volume에 Depth Of Field Override가 필요합니다.");

        // Color Adjustments 가져오기
        if (!globalVolume.profile.TryGet(out color))
            Debug.LogError(" Global Volume에 Color Adjustments Override가 필요합니다. (Post Exposure 포함)");

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

        dof.mode.value = DepthOfFieldMode.Bokeh;
        //OVRInput.SetControllerVibration(1, Amplitude, OVRInput.Controller.RTouch);
        // 슬라이더 방향 반전 (value=1 → 가까운 물체)
        float reversed = 1f - value;

        // === DoF 조절 ===
        dof.focusDistance.value = Mathf.Lerp(0.3f, 8f, reversed); // 가까움~멀리
        dof.aperture.value = Mathf.Lerp(16f, 1.4f, reversed);     // 깊은 DOF~얕은 DOF

        // === Exposure 조절 ===
        if (color != null)
        {
            float apertureValue = dof.aperture.value;

            // f/1.4~f/16 → EV 2.1~−5.0 선형 매핑
            float t = Mathf.InverseLerp(1.4f, 16f, apertureValue);
            float exposureEV = Mathf.Lerp(2.1f, -5.0f, t);

            color.postExposure.value = exposureEV;
        }

        // 햅틱 조절
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
        // 햅틱 시작 (오른쪽 컨트롤러)
        OVRInput.SetControllerVibration(1, amplitude, OVRInput.Controller.RTouch);

        // 지정된 시간(duration)만큼 대기
        yield return new WaitForSeconds(duration);

        // 햅틱 중지
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        hapticCoroutine = null;
    }
}
