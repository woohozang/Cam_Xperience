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
    [Space]
    //public Slider focusSlider;
    public Slider apertureSlider;
    public Slider shutterSlider;
    public Slider isoSlider;

    [Header("Target Components")]
    [Tooltip("DoF, Motion Blur, Color Adjustments가 포함된 Volume")]
    public Volume globalVolume;

    // 캐시된 포스트 프로세싱 오버라이드
    private DepthOfField dof;
    private MotionBlur mb;
    private ColorAdjustments color;

    [Header("Slider Ranges")]
    public float minFocus = 0.3f;  // 최소 초점 거리 (m)
    public float maxFocus = 10f; // 최대 초점 거리 (m)
    [Space]
    public float minAperture = 16f;  // 최소 조리개 (f/16)
    public float maxAperture = 1.4f; // 최대 조리개 (f/1.4)
    [Space]
    [Tooltip("셔터 스피드 분모 값 (예: 4000 = 1/4000초)")]
    public float minShutterDenominator = 4000f; // 최대 속도
    [Tooltip("셔터 스피드 분모 값 (예: 1 = 1초)")]
    public float maxShutterDenominator = 1f;    // 최소 속도
    [Space]
    public int minIso = 100;
    public int maxIso = 6400;

    [Header("Manual Exposure Settings (EV Stops)")]
    [Tooltip("Aperture(f/16 -> f/1.4)가 밝기에 기여하는 EV 범위")]
    public Vector2 apertureEvRange = new Vector2(-5.0f, 2.1f);
    [Tooltip("Shutter(1/4000s -> 1s)가 밝기에 기여하는 EV 범위")]
    public Vector2 shutterEvRange = new Vector2(-4.0f, 4.0f);
    [Tooltip("ISO(100 -> 6400)가 밝기에 기여하는 EV 범위 (ISO 100이 0 EV 기준)")]
    public Vector2 isoEvRange = new Vector2(0.0f, 6.0f); // 100 -> 6400 = 6 스탑
    [Tooltip("모션 블러의 최대 강도 (셔터가 가장 느릴 때)")]
    public float maxBlurIntensity = 1.0f;


    [Header("MoveHaptic")]
    public float hapticSensitivity = 1.5f;
    public float hapticDuration = 0.05f;

    // 각 슬라이더의 이전 값을 저장
    //private float lastFocusValue;
    private float lastApertureValue;
    private float lastShutterValue;
    private float lastIsoValue;

    private Coroutine hapticCoroutine;

    void Awake()
    {
        // --- 컴포넌트 참조 확인 ---
        if (globalVolume == null)
            Debug.LogError("Global Volume 참조가 필요합니다!");

        if (!globalVolume.profile.TryGet(out dof))
            Debug.LogError("Global Volume에 Depth Of Field Override가 필요합니다.");

        if (!globalVolume.profile.TryGet(out mb))
            Debug.LogError("Global Volume에 Motion Blur Override가 필요합니다.");

        if (!globalVolume.profile.TryGet(out color))
            Debug.LogError("Global Volume에 Color Adjustments Override가 필요합니다.");

        // --- 슬라이더 이벤트 연결 및 초기값 설정 ---
        //InitializeSlider(focusSlider, OnFocusSliderChanged, ref lastFocusValue);
        InitializeSlider(apertureSlider, OnApertureSliderChanged, ref lastApertureValue);
        InitializeSlider(shutterSlider, OnShutterSliderChanged, ref lastShutterValue);
        InitializeSlider(isoSlider, OnIsoSliderChanged, ref lastIsoValue);

        // --- 초기값 즉시 반영 ---
        //if (focusSlider) OnFocusSliderChanged(focusSlider.value);
        if (apertureSlider) OnApertureSliderChanged(apertureSlider.value);
        if (shutterSlider) OnShutterSliderChanged(shutterSlider.value);
        if (isoSlider) OnIsoSliderChanged(isoSlider.value);

        // 시작 시 UI 비활성화
        if (worldCanvas != null)
            worldCanvas.gameObject.SetActive(false);
    }

    // 슬라이더 초기화 헬퍼 함수
    private void InitializeSlider(Slider slider, UnityEngine.Events.UnityAction<float> listener, ref float lastValueTracker)
    {
        if (slider != null)
        {
            slider.onValueChanged.AddListener(listener);
            lastValueTracker = slider.value;
        }
    }

    public void Toggle()
    {
        bool isActive = !worldCanvas.gameObject.activeSelf;
        worldCanvas.gameObject.SetActive(isActive);
        Debug.Log($"[DialUIController] World UI {(isActive ? "ON" : "OFF")}");
    }

    // --- 1. 초점 슬라이더 로직 ---
    /*private void OnFocusSliderChanged(float value)
    {
        if (dof != null)
        {
            dof.focusDistance.value = Mathf.Lerp(minFocus, maxFocus, value);
        }
        TriggerHaptic(CalculateSpeed(value, ref lastFocusValue));
        lastFocusValue = value;
    }*/

    // --- 2. 조리개 슬라이더 로직 ---
    private void OnApertureSliderChanged(float value)
    {
        if (dof != null)
        {
            // DoF 파라미터 제어
            dof.aperture.value = Mathf.Lerp(minAperture, maxAperture, value);
        }
        UpdateManualExposure(); // 노출(밝기) 업데이트
        TriggerHaptic(CalculateSpeed(value, ref lastApertureValue));
        lastApertureValue = value;
    }

    // --- 3. 셔터 스피드 슬라이더 로직 ---
    private void OnShutterSliderChanged(float value)
    {
        if (mb != null)
        {
            // Motion Blur 파라미터 제어 (0=빠름=블러 없음, 1=느림=블러 최대)
            mb.intensity.value = Mathf.Lerp(0, maxBlurIntensity, value);
        }
        UpdateManualExposure(); // 노출(밝기) 업데이트
        TriggerHaptic(CalculateSpeed(value, ref lastShutterValue));
        lastShutterValue = value;
    }

    // --- 4. ISO 슬라이더 로직 ---
    private void OnIsoSliderChanged(float value)
    {
        UpdateManualExposure(); // 노출(밝기) 업데이트
        TriggerHaptic(CalculateSpeed(value, ref lastIsoValue));
        lastIsoValue = value;
    }

    /// <summary>
    /// [핵심] 3개의 슬라이더 값을 읽어 최종 노출(밝기)을 계산하고 적용합니다.
    /// </summary>
    private void UpdateManualExposure()
    {
        if (color == null) return;

        // 각 슬라이더의 현재 값(0.0 ~ 1.0)을 가져옵니다.
        float apertureT = (apertureSlider != null) ? apertureSlider.value : 0.5f;
        float shutterT = (shutterSlider != null) ? shutterSlider.value : 0.5f;
        float isoT = (isoSlider != null) ? isoSlider.value : 0.5f;

        // 각 슬라이더의 EV 기여도를 계산합니다.
        // Aperture: 0(f/16, 어두움) -> 1(f/1.4, 밝음)
        float apertureEV = Mathf.Lerp(apertureEvRange.x, apertureEvRange.y, apertureT);

        // Shutter: 0(1/4000s, 어두움) -> 1(1s, 밝음)
        float shutterEV = Mathf.Lerp(shutterEvRange.x, shutterEvRange.y, shutterT);

        // ISO: 0(100, 어두움) -> 1(6400, 밝음)
        float isoEV = Mathf.Lerp(isoEvRange.x, isoEvRange.y, isoT);

        // 3개의 EV 값을 모두 합산하여 Post Exposure에 적용
        color.postExposure.value = apertureEV + shutterEV + isoEV;
    }


    // --- 햅틱 로직 (모든 슬라이더가 공유) ---
    private float CalculateSpeed(float newValue, ref float lastValue)
    {
        if (Time.deltaTime > 0)
        {
            return Mathf.Abs(newValue - lastValue) / Time.deltaTime;
        }
        return 0f;
    }

    private void TriggerHaptic(float speed)
    {
        float dynamicAmplitude = Mathf.Clamp01(speed * hapticSensitivity);

        if (hapticCoroutine != null)
        {
            StopCoroutine(hapticCoroutine);
        }
        hapticCoroutine = StartCoroutine(VibrateForDuration(hapticDuration, dynamicAmplitude));
    }

    private IEnumerator VibrateForDuration(float duration, float amplitude)
    {
        OVRInput.SetControllerVibration(1, amplitude, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        hapticCoroutine = null;
    }
}