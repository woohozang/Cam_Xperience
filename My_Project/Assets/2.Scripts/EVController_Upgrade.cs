using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // URP 사용 시
using OVR; // Oculus Integration
using System.Collections;
using TMPro; // TextMeshPro 네임스페이스

public class EVController_Upgrade : MonoBehaviour
{
    [Header("UI")]
    public Canvas worldCanvas;
    [Space]
    public Slider apertureSlider;
    public Slider shutterSlider;
    public Slider isoSlider;
    [Space]
    [Header("UI Text Display")]
    public TMP_Text apertureText;
    public TMP_Text shutterText;
    public TMP_Text isoText;
    //public TMP_Text evText; // EV 값을 표시할 TextMeshPro UI 요소

    [Header("Target Components")]
    [Tooltip("DoF, Motion Blur, Color Adjustments, Film Grain이 포함된 Volume")]
    public Volume globalVolume;

    // 캐시된 포스트 프로세싱 오버라이드
    private DepthOfField dof;
    private MotionBlur mb;
    private ColorAdjustments color;
    private FilmGrain filmGrain;

    // --- [수정됨] Discrete 값 배열 ---
    [Header("Camera Setting Steps")]
    [Tooltip("조리개 f-stop 값 (10단계)")]
    public float[] apertureValues = { 1.2f, 1.8f, 2.8f, 3.5f, 5.2f, 6.2f, 7.8f, 9.5f, 11f, 13f, 16f };

    [Tooltip("셔터 속도(초 단위) (25단계)")]
    public float[] shutterSpeedValues = {
        20f, 15f, 10f, 8f, 5f, 4f, 3f, 2.5f, 2f, 1.6f, 1.3f, 1f,
        0.8f, 0.5f, 1f/4f, 1f/10f, 1f/25f, 1f/50f, 1f/100f, 1f/200f,
        1f/400f, 1f/800f, 1f/1000f, 1f/2000f, 1f/4000f
    };

    [Tooltip("셔터 속도 표시용 레이블 (25단계)")]
    public string[] shutterSpeedLabels = {
        "20\"", "15\"", "10\"", "8\"", "5\"", "4\"", "3\"", "2.5\"", "2\"", "1.6\"", "1.3\"", "1\"",
        "0.8\"", "0.5\"", "1/4", "1/10", "1/25", "1/50", "1/100", "1/200",
        "1/400", "1/800", "1/1000", "1/2000", "1/4000"
    };

    [Tooltip("ISO 값 (7단계)")]
    public int[] isoValues = { 50, 100, 200, 400, 800, 1500, 3000, 5000, 8000 };

    // [기존 변수 삭제됨]
    // minAperture, maxAperture, minShutterDenominator, maxShutterDenominator, minIso, maxIso 삭제

    [Header("Effect Intensities")]
    [Tooltip("모션 블러의 최대 강도 (셔터가 가장 느릴 때)")]
    public float maxBlurIntensity = 0.8f; // 값 약간 조정
    [Tooltip("ISO가 최대일 때 필름 그레인(노이즈)의 최대 강도")]
    public float maxGrainIntensity = 0.5f;

    [Header("EV Calibration")]
    [Tooltip("이 씬의 '적정 노출' 기준값 (EV). 이 값에서 PostExposure가 0이 됩니다.")]
    public float sceneBaselineEV = 10.6f; // 예: 맑은 날 EV 15 (Sunny 16 rule)

    [Header("MoveHaptic")]
    public float hapticSensitivity = 1.5f; // 이 값은 이제 사용되지 않지만, TriggerHaptic에서 고정된 강도로 사용할 수 있습니다.
    public float hapticDuration = 0.05f;

    private float lastApertureValue;
    private float lastShutterValue;
    private float lastIsoValue;
    private Coroutine hapticCoroutine;

    private static readonly float LOG_BASE_2 = Mathf.Log(2);

    void Awake()
    {
        if (globalVolume == null)
            Debug.LogError("Global Volume 참조가 필요합니다!");

        // 포스트 프로세싱 컴포넌트 가져오기
        globalVolume.profile.TryGet(out dof);
        globalVolume.profile.TryGet(out mb);
        globalVolume.profile.TryGet(out color);
        globalVolume.profile.TryGet(out filmGrain);

        // --- [수정됨] 슬라이더 설정 및 이벤트 연결 ---
        // Unity Inspector에서 이 슬라이더들의 'Whole Numbers'를 true로 설정해야 합니다.
        // Aperture: Min 0, Max 10
        // Shutter:  Min 0, Max 24
        // ISO:      Min 0, Max 9
        InitializeSlider(apertureSlider, OnApertureSliderChanged, apertureValues.Length - 1, ref lastApertureValue);
        InitializeSlider(shutterSlider, OnShutterSliderChanged, shutterSpeedValues.Length - 1, ref lastShutterValue);
        InitializeSlider(isoSlider, OnIsoSliderChanged, isoValues.Length - 1, ref lastIsoValue);

        // --- 초기값 즉시 반영 ---
        UpdateCameraEffects();

        if (worldCanvas != null)
            worldCanvas.gameObject.SetActive(false);
    }

    private void InitializeSlider(Slider slider, UnityEngine.Events.UnityAction<float> listener, int maxValue, ref float lastValueTracker)
    {
        if (slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = maxValue;
            slider.wholeNumbers = true; // 정수 값만 사용하도록 설정
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
        // 값이 변경될 때마다 햅틱 발생 (단계별 "클릭" 느낌)
        if (value != lastApertureValue)
        {
            TriggerHaptic(0.5f); // 0.5f 강도로 햅틱
            lastApertureValue = value;
        }
    }

    private void OnShutterSliderChanged(float value)
    {
        UpdateCameraEffects();
        if (value != lastShutterValue)
        {
            TriggerHaptic(0.5f);
            lastShutterValue = value;
        }
    }

    private void OnIsoSliderChanged(float value)
    {
        UpdateCameraEffects();
        if (value != lastIsoValue)
        {
            TriggerHaptic(0.5f);
            lastIsoValue = value;
        }
    }

    /// <summary>
    /// [핵심 수정] 3개의 슬라이더 '인덱스' 값을 기반으로 실제 값을 찾아 효과를 업데이트합니다.
    /// </summary>
    private void UpdateCameraEffects()
    {
        // 슬라이더에서 현재 인덱스(정수) 가져오기
        int apertureIndex = (apertureSlider != null) ? (int)apertureSlider.value : 3;
        int shutterIndex = (shutterSlider != null) ? (int)shutterSlider.value : 18;
        int isoIndex = (isoSlider != null) ? (int)isoSlider.value : 3;

        // --- 1. 조리개 (Aperture) 계산 및 적용 ---
        float currentAperture = apertureValues[apertureIndex];
        if (dof != null)
        {
            dof.aperture.value = currentAperture;
        }
        if (apertureText != null)
        {
            apertureText.text = $"f/{currentAperture:F1}";
        }

        // --- 2. 셔터 속도 (Shutter Speed) 계산 및 적용 ---
        float currentShutterSpeed = shutterSpeedValues[shutterIndex]; // 값 (초)
        string currentShutterLabel = shutterSpeedLabels[shutterIndex]; // 레이블 (문자열)

        if (mb != null)
        {
            // 셔터 속도가 느릴수록(인덱스가 낮을수록) 모션 블러 강도를 높입니다.
            float shutterT = (float)shutterIndex / (shutterSpeedValues.Length - 1); // 0 (느림) ~ 1 (빠름)
            mb.intensity.value = Mathf.Lerp(maxBlurIntensity, 0, shutterT);
        }
        if (shutterText != null)
        {
            shutterText.text = currentShutterLabel;
        }

        // --- 3. ISO 계산 및 적용 ---
        int currentIso = isoValues[isoIndex];

        if (filmGrain != null)
        {
            // ISO가 높을수록(인덱스가 높을수록) 노이즈 강도를 높입니다.
            float isoT = (float)isoIndex / (isoValues.Length - 1); // 0 (50) ~ 1 (8000)
            filmGrain.intensity.value = Mathf.Lerp(0, maxGrainIntensity, isoT);
        }
        if (isoText != null)
        {
            isoText.text = $"{currentIso}";
        }

        // --- 4. [수정됨] 실제 공식으로 노출(Exposure) 계산 ---
        if (color != null)
        {
            float t = currentShutterSpeed;
            float N = currentAperture;
            float ISO = (float)currentIso;

            // EV100 (ISO 100 기준 EV) 계산: EV = log₂(N² / t)
            float EV100_setting = Log2((N * N) / t);

            // ISO 보정값(스탑 단위) 계산: SV = log₂(ISO / 100)
            float isoCompensation_SV = Log2(ISO / 100.0f);

            // 카메라의 최종 유효 노출값: EffectiveEV = EV100 - SV
            float effectiveEV = EV100_setting - isoCompensation_SV;

            // 씬의 기준 EV(sceneBaselineEV)와 카메라의 유효 EV(effectiveEV)의 차이를 계산
            // 이 차이가 Unity의 Post Exposure 값이 됩니다.
            // postExposure = sceneBaselineEV - effectiveEV
            // (예: 씬(13)이 카메라(12)보다 1스탑 밝으면, postExposure = +1.0 이 되어 이미지를 밝게 보정)
            float postExposureValue = sceneBaselineEV - effectiveEV;

            color.postExposure.value = postExposureValue;

            // [추가] EV 미터기 업데이트
            /*if (evText != null)
            {
                // 사용자가 씬의 기준(sceneBaselineEV)에서 얼마나 벗어났는지 표시합니다.
                // 위에서 계산한 postExposureValue는 '보정'값이므로, 사용자에게는 반대 방향을 표시해야 합니다.
                // (예: postExposureValue가 +1 (씬보다 1스탑 어둡게 찍힘) -> 미터기는 -1 (노출 부족) 표시)
                // 하지만 시뮬레이터에서는 "현재 설정의 EV가 씬의 EV보다 얼마나 높은가/낮은가"를 보여주는 것이 더 직관적일 수 있습니다.
                // 여기서는 HTML 시뮬레이터와 유사하게 "적정 노출(0) 대비 차이"를 표시합니다.
                // HTML 시뮬레이터 로직: 9 - (Av + Tv - Sv) = 9 - effectiveEV
                // 여기서 9는 임의의 기준점입니다. 우리는 sceneBaselineEV를 사용합니다.

                float userEV_Display = sceneBaselineEV - effectiveEV;

                // +- 0.5 스탑 이내면 적정 노출로 간주
                string evStatus;
                if (userEV_Display > 0.5) evStatus = "노출 과다 (+)";
                else if (userEV_Display < -0.5) evStatus = "노출 부족 (-)";
                else evStatus = "적정 노출";

                evText.text = $"EV: {userEV_Display:F1} ({evStatus})";
            }*/
        }
    }

    /// <summary>
    /// Mathf.Log(f, 2) (Log base 2)를 계산하는 헬퍼 함수
    /// </summary>
    private float Log2(float value)
    {
        if (value <= 0) return 0; // 로그 오류 방지
        return Mathf.Log(value) / LOG_BASE_2;
    }

    // --- 햅틱 로직 ---
    // CalculateSpeed는 더 이상 필요하지 않습니다.

    private void TriggerHaptic(float amplitude)
    {
        // 이미 진행 중인 햅틱이 있다면 중지하고 새로 시작
        if (hapticCoroutine != null)
        {
            StopCoroutine(hapticCoroutine);
        }
        hapticCoroutine = StartCoroutine(VibrateForDuration(hapticDuration, amplitude));
    }

    private IEnumerator VibrateForDuration(float duration, float amplitude)
    {
        OVRInput.SetControllerVibration(1, amplitude, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        hapticCoroutine = null;
    }
}