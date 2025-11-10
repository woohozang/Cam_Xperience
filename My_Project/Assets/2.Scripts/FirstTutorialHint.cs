using UnityEngine;

/// <summary>
/// UI를 위아래(또는 원하는 축)로 부드럽게 떠다니게 만드는 스크립트.
/// 월드/스크린 모든 캔버스에서 동작. CanvasGroup 페이드와도 독립.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class FirstTutorialHint : MonoBehaviour
{
    [Tooltip("움직일 축 (기본: 위아래)")]
    public Vector3 axis = new Vector3(0f, 1f, 0f);

    [Tooltip("진폭 (월드 스페이스라면 0.03~0.06 추천)")]
    public float amplitude = 0.04f;

    [Tooltip("초당 왕복 빈도 (0.5~1.0 부드럽게)")]
    public float frequency = 0.6f;

    [Tooltip("개별 오브젝트 간 위상 차 (랜덤 시작 추천)")]
    public float phaseOffset = 0f;

    [Tooltip("TimeScale의 영향 없이 움직일지")]
    public bool useUnscaledTime = true;

    [Header("Optional: 살짝 기울기/스케일 펄스")]
    public float tiltDegrees = 0f;      // 0~3 정도
    public float scalePulse = 0f;        // 0~0.05 정도

    RectTransform rt;
    Vector3 baseAnchoredPos3D;
    Quaternion baseRot;
    Vector3 baseScale;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        baseAnchoredPos3D = rt.anchoredPosition3D;
        baseRot = rt.localRotation;
        baseScale = rt.localScale;

        // 약간의 랜덤 위상으로 같아 보이지 않게
        if (Mathf.Approximately(phaseOffset, 0f))
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void OnEnable()
    {
        // 외부에서 위치를 바꿨으면 기준점 갱신
        baseAnchoredPos3D = rt.anchoredPosition3D;
    }

    void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;
        float w = Mathf.PI * 2f * frequency;
        float s = Mathf.Sin(w * t + phaseOffset);

        // 위치
        Vector3 offset = axis.normalized * amplitude * s;
        rt.anchoredPosition3D = baseAnchoredPos3D + offset;

        // 선택: 살짝 기울기/스케일
        if (!Mathf.Approximately(tiltDegrees, 0f))
            rt.localRotation = baseRot * Quaternion.Euler(0f, 0f, tiltDegrees * s);
        if (!Mathf.Approximately(scalePulse, 0f))
            rt.localScale = baseScale * (1f + scalePulse * s);
    }
}
