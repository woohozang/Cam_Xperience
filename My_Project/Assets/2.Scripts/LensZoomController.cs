using UnityEngine;
using System.Collections; // [추가] 햅틱 코루틴
using OVR; // [추가] 햅틱 입력

public class LensZoomController : MonoBehaviour
{
    [Header("Target Camera")]
    public Camera lensCamera;

    [Header("Zoom (Field of View)")]
    public float wideFoV = 84f;
    public float telephotoFoV = 23f;
    public float defaultFoV = 60f;

    [Header("Rotation Mapping")]
    public float minRotationAngle = -45f;
    public float maxRotationAngle = 45f;
    public enum RotationAxis { X, Y, Z }
    public RotationAxis axisToRead = RotationAxis.Y;

    [Header("Haptics")] // [추가]
    [Tooltip("회전 속도에 따른 햅틱 민감도")]
    public float hapticSensitivity = 0.5f;
    [Tooltip("햅틱 1회당 지속시간")]
    public float hapticDuration = 0.05f;
    [Tooltip("햅틱이 울리기 시작하는 최소 회전 속도 (deg/sec)")]
    public float minRotationSpeedForHaptic = 10f; // [추가]

    // --- Private Variables ---
    private float lastAngle = 0f; // [추가]
    private Coroutine hapticCoroutine; // [추가]

    void Start() // [Awake()에서 Start()로 변경]
    {
        if (lensCamera != null)
        {
            lensCamera.fieldOfView = defaultFoV;
        }

        // --- 초기 각도 설정 (기존과 동일) ---
        float initialNormalizedAngle = Mathf.InverseLerp(wideFoV, telephotoFoV, defaultFoV);
        initialNormalizedAngle = Mathf.Clamp01(initialNormalizedAngle);
        float initialAngle = Mathf.Lerp(minRotationAngle, maxRotationAngle, initialNormalizedAngle);

        Vector3 currentEuler = transform.localEulerAngles;
        switch (axisToRead)
        {
            case RotationAxis.X:
                transform.localEulerAngles = new Vector3(initialAngle, currentEuler.y, currentEuler.z);
                break;
            case RotationAxis.Y:
                transform.localEulerAngles = new Vector3(currentEuler.x, initialAngle, currentEuler.z);
                break;
            case RotationAxis.Z:
                transform.localEulerAngles = new Vector3(currentEuler.x, currentEuler.y, initialAngle);
                break;
        }

        lastAngle = initialAngle;
    }

    void Update()
    {
        if (lensCamera == null) return;

        // 1. 현재 회전 각도를 읽습니다.
        float currentAngle = GetCurrentAxisAngle();

        // --- [수정됨] 햅틱 로직 (Update 안에 통합) ---
        float rotationSpeed = 0f;
        if (Time.deltaTime > 0f)
        {
            // 이전 프레임 대비 각도 변화량(속도) 계산 (초당 각도)
            rotationSpeed = Mathf.Abs(currentAngle - lastAngle) / Time.deltaTime;
        }

        // 속도에 기반해 햅틱 실행
        TriggerHaptic(rotationSpeed);

        // 다음 프레임을 위해 현재 각도 저장
        lastAngle = currentAngle;
        // --- 햅틱 로직 끝 ---


        // 2. 현재 각도를 0.0 ~ 1.0 비율로 정규화
        float normalizedAngle = Mathf.InverseLerp(minRotationAngle, maxRotationAngle, currentAngle);
        normalizedAngle = Mathf.Clamp01(normalizedAngle);

        // 3. FoV 값 계산
        float newFoV = Mathf.Lerp(wideFoV, telephotoFoV, normalizedAngle);

        // 4. 카메라 FoV 업데이트
        lensCamera.fieldOfView = newFoV;
    }

    // [추가] 현재 각도를 읽어오는 헬퍼 함수
    private float GetCurrentAxisAngle()
    {
        float currentAngle = 0;
        Vector3 eulerAngles = transform.localEulerAngles;

        switch (axisToRead)
        {
            case RotationAxis.X:
                currentAngle = (eulerAngles.x > 180f) ? eulerAngles.x - 360f : eulerAngles.x;
                break;
            case RotationAxis.Y:
                currentAngle = (eulerAngles.y > 180f) ? eulerAngles.y - 360f : eulerAngles.y;
                break;
            case RotationAxis.Z:
                currentAngle = (eulerAngles.z > 180f) ? eulerAngles.z - 360f : eulerAngles.z;
                break;
        }
        return currentAngle;
    }

    // --- [추가] 햅틱 헬퍼 함수들 ---
    private void TriggerHaptic(float speed)
    {
        // [수정] 최소 속도(예: 10도/초) 미만이면 햅틱 중지
        if (speed < minRotationSpeedForHaptic)
        {
            StopHaptics();
            return;
        }

        float dynamicAmplitude = Mathf.Clamp01(speed * hapticSensitivity);

        if (hapticCoroutine != null)
        {
            StopCoroutine(hapticCoroutine);
        }
        hapticCoroutine = StartCoroutine(VibrateForDuration(hapticDuration, dynamicAmplitude));
    }

    private void StopHaptics()
    {
        if (hapticCoroutine != null)
        {
            StopCoroutine(hapticCoroutine);
            hapticCoroutine = null;
        }
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }

    private IEnumerator VibrateForDuration(float duration, float amplitude)
    {
        OVRInput.SetControllerVibration(1, amplitude, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        hapticCoroutine = null;
    }
}