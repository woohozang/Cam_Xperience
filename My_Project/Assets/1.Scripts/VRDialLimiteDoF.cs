using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VRDialLimitedDoF : MonoBehaviour
{
    [Header("References")]
    public Transform dial;                // 다이얼 오브젝트
    public Volume postProcessVolume;      // URP Volume
    private DepthOfField dof;

    [Header("DoF Settings")]
    public float minFocus = 0.2f;         // 최소 초점 거리
    public float maxFocus = 10f;          // 최대 초점 거리
    public float minAperture = 1.4f;      // 최소 조리개
    public float maxAperture = 16f;       // 최대 조리개

    [Header("Dial Settings")]
    public float minAngle = 0f;           // 최소 회전 각도
    public float maxAngle = 180f;         // 최대 회전 각도
    public Axis dialAxis = Axis.Z;        // 회전축 선택 (보통 DSLR 다이얼은 Z축)

    private float currentAngle;

    public enum Axis { X, Y, Z }

    void Start()
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out dof);
        }
    }

    void Update()
    {
        if (dof == null || dial == null) return;

        // 현재 회전값 가져오기
        switch (dialAxis)
        {
            case Axis.X: currentAngle = dial.localEulerAngles.x; break;
            case Axis.Y: currentAngle = dial.localEulerAngles.y; break;
            case Axis.Z: currentAngle = dial.localEulerAngles.z; break;
        }

        // Unity 각도는 0~360° 범위 → -180~180° 로 보정
        if (currentAngle > 180f) currentAngle -= 360f;

        // 제한 범위 클램프
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        // 실제 다이얼 Transform에도 제한 적용
        Vector3 euler = dial.localEulerAngles;
        switch (dialAxis)
        {
            case Axis.X: euler.x = currentAngle; break;
            case Axis.Y: euler.y = currentAngle; break;
            case Axis.Z: euler.z = currentAngle; break;
        }
        dial.localEulerAngles = euler;

        // 0~1 정규화
        float t = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);

        // DoF 값 보간
        dof.focusDistance.value = Mathf.Lerp(minFocus, maxFocus, t);
        dof.aperture.value = Mathf.Lerp(minAperture, maxAperture, t);

        Debug.Log($"Dial {dialAxis}: {currentAngle}° | Focus={dof.focusDistance.value:F2} | Aperture={dof.aperture.value:F1}");
    }
}
