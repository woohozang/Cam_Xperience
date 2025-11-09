using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // URP 네임스페이스
using OVR;
using System.Collections;
using TMPro; // TextMeshPro 네임스페이스

public class EVController : MonoBehaviour
{
    [Header("UI")]
    public Canvas worldCanvas;
    [Space]
    //public Slider focusSlider; // [주석 처리됨]
    public Slider apertureSlider;
    public Slider shutterSlider;
    public Slider isoSlider;
    [Space]
    [Header("UI Text Display")]
    public TMP_Text apertureText;
    public TMP_Text shutterText;
    public TMP_Text isoText;

    [Header("Target Components")]
    [Tooltip("DoF, Motion Blur, Color Adjustments, Film Grain이 포함된 Volume")]
    public Volume globalVolume;

    // 캐시된 포스트 프로세싱 오버라이드
    private DepthOfField dof;
    private MotionBlur mb;
    private ColorAdjustments color;
    private FilmGrain filmGrain;

    [Header("Slider Ranges")]
    // ... [focusSlider 관련 주석 처리] ...
    // public float minFocus = 0.3f;
    // public float maxFocus = 10f;
    [Space]
    public float minAperture = 16f;  // f/16 (슬라이더 값 0)
    public float maxAperture = 1.4f; // f/1.4 (슬라이더 값 1)
    [Space]
    [Tooltip("셔터 스피드 분모 값 (예: 4000 = 1/4000초)")]
    public float minShutterDenominator = 4000f; // (슬라이더 값 0)
    [Tooltip("셔터 스피드 분모 값 (예: 1 = 1초)")]
    public float maxShutterDenominator = 1f;    // (슬라이더 값 1)
    [Space]
    public int minIso = 100; // (슬라이더 값 0)
    public int maxIso = 12800; // [값 수정] (슬라이더 값 1)

    // [삭제] Manual Exposure Settings (EV Stops) 헤더 및 변수들 삭제

    [Header("Effect Intensities")]
    [Tooltip("모션 블러의 최대 강도 (셔터가 가장 느릴 때)")]
    public float maxBlurIntensity = 1.0f;
    [Tooltip("ISO가 최대일 때 필름 그레인(노이즈)의 최대 강도")]
    public float maxGrainIntensity = 0.5f;

    [Header("EV Calibration")]
    [Tooltip("이 씬의 '적정 노출' 기준값 (EV). 이 값에서 PostExposure가 0이 됩니다.")]
    public float sceneBaselineEV = 13f; // [추가] (예: 맑은 날 f/16, 1/125s, ISO 100)

    [Header("MoveHaptic")]
    // ... (기존 햅틱 변수들) ...
    public float hapticSensitivity = 1.5f;
    public float hapticDuration = 0.05f;

    // ... (기존 private 변수들) ...
    private float lastApertureValue;
    private float lastShutterValue;
    private float lastIsoValue;
    private Coroutine hapticCoroutine;

    // [추가] 로그(log) 계산을 위한 상수 (성능 최적화)
    private static readonly float LOG_BASE_2 = Mathf.Log(2);

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

        if (!globalVolume.profile.TryGet(out filmGrain))
            Debug.LogWarning("Global Volume에 Film Grain Override가 없습니다. ISO 노이즈 효과가 적용되지 않습니다.");

        // --- 슬라이더 이벤트 연결 및 초기값 설정 ---
        InitializeSlider(apertureSlider, OnApertureSliderChanged, ref lastApertureValue);
        InitializeSlider(shutterSlider, OnShutterSliderChanged, ref lastShutterValue);
        InitializeSlider(isoSlider, OnIsoSliderChanged, ref lastIsoValue);

        // --- 초기값 즉시 반영 ---
        UpdateCameraEffects(); // 통합된 함수 호출

        // 시작 시 UI 비활성화
        if (worldCanvas != null)
            worldCanvas.gameObject.SetActive(false);
    }

    // ... (InitializeSlider, Toggle, 슬라이더 OnChanged 함수들 기존과 동일) ...

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

    private void OnApertureSliderChanged(float value)
    {
        UpdateCameraEffects();
        TriggerHaptic(CalculateSpeed(value, ref lastApertureValue));
        lastApertureValue = value;
    }

    private void OnShutterSliderChanged(float value)
    {
        UpdateCameraEffects();
        TriggerHaptic(CalculateSpeed(value, ref lastShutterValue));
        lastShutterValue = value;
    }

    private void OnIsoSliderChanged(float value)
    {
        UpdateCameraEffects();
        TriggerHaptic(CalculateSpeed(value, ref lastIsoValue));
        lastIsoValue = value;
    }


    /// <summary>
    /// [핵심 수정] 3개의 슬라이더 값을 '실제 물리 공식'으로 계산하여 모든 효과를 업데이트합니다.
    /// </summary>
    private void UpdateCameraEffects()
    {
        float apertureT = (apertureSlider != null) ? apertureSlider.value : 0.5f;
        float shutterT = (shutterSlider != null) ? shutterSlider.value : 0.5f;
        float isoT = (isoSlider != null) ? isoSlider.value : 0.5f;

        // --- 1. 조리개 (Aperture) 계산 및 적용 ---
        // (f/16 (0) -> f/1.4 (1))
        float currentAperture = Mathf.Lerp(minAperture, maxAperture, apertureT);
        if (dof != null)
        {
            dof.aperture.value = currentAperture;
        }
        if (apertureText != null)
        {
            apertureText.text = $"F{currentAperture:F1}";
        }

        // --- 2. 셔터 속도 (Shutter Speed) 계산 및 적용 ---
        // (1/4000s (0) -> 1s (1))
        float logMinShutter = Mathf.Log10(minShutterDenominator);
        float logMaxShutter = Mathf.Log10(maxShutterDenominator);
        float currentShutterDenom = Mathf.Pow(10, Mathf.Lerp(logMinShutter, logMaxShutter, shutterT));

        if (mb != null)
        {
            mb.intensity.value = Mathf.Lerp(0, maxBlurIntensity, shutterT);
        }
        if (shutterText != null)
        {
            if (currentShutterDenom <= 1f)
            {
                shutterText.text = $"{currentShutterDenom:F1}s";
            }
            else
            {
                shutterText.text = $"1/{Mathf.RoundToInt(currentShutterDenom)}";
            }
        }

        // --- 3. ISO 계산 및 적용 ---
        // (100 (0) -> 12800 (1))
        float logMinIso = Mathf.Log10(minIso);
        float logMaxIso = Mathf.Log10(maxIso);
        float currentIso = Mathf.Pow(10, Mathf.Lerp(logMinIso, logMaxIso, isoT));

        if (filmGrain != null)
        {
            filmGrain.intensity.value = Mathf.Lerp(0, maxGrainIntensity, isoT);
        }
        if (isoText != null)
        {
            isoText.text = $"{Mathf.RoundToInt(currentIso)}";
        }

        // --- 4. [수정됨] 실제 공식으로 노출(Exposure) 계산 ---
        if (color != null)
        {
            // 실제 셔터 속도 (초 단위)
            float t = 1.0f / currentShutterDenom;
            // 실제 조리개 값 (f-number)
            float N = currentAperture;
            // 실제 ISO 값
            float ISO = currentIso;

            // EV100 (ISO 100 기준 EV) 계산: EV = log₂(N² / t)
            float EV100 = Log2((N * N) / t);

            // ISO를 반영한 최종 EV 계산: EV_final = EV100 + log₂(ISO / 100)
            float totalEV = EV100 + Log2(ISO / 100.0f);

            // 씬의 기준 EV(13)를 빼서 Post Exposure 값을 보정
            // (예: 씬 기준 13EV에서 0 EV(변화 없음)가 되도록 함)
            color.postExposure.value = totalEV - sceneBaselineEV;
        }
    }

    /// <summary>
    /// Mathf.Log(f, 2) (Log base 2)를 계산하는 헬퍼 함수
    /// </summary>
    private float Log2(float value)
    {
        return Mathf.Log(value) / LOG_BASE_2;
    }


    // --- 햅틱 로직 (기존과 동일) ---
    // ... (CalculateSpeed, TriggerHaptic, VibrateForDuration 함수들) ...
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