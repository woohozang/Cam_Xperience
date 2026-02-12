using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// [RequireComponent(typeof(Collider))]
/// </summary>
public class SliderFingerDrag : MonoBehaviour
{
    [Header("References")]
    public Slider slider;                     // 대상 슬라이더
    public RectTransform trackRect;           // Slider의 트랙 RectTransform (보통 Slider의 RectTransform)
    public Canvas worldCanvas;                // 이 UI가 붙은 World Space Canvas

    [Header("Filtering")]
    public string handTag = "Hand";           // 손가락 콜라이더 Tag
    public float deadZone = 0.0f;             // 원하면 중앙 데드존(cm) 같은 튜닝용

    Camera _uiCam;

    void Awake()
    {
        if (worldCanvas == null)
            worldCanvas = GetComponentInParent<Canvas>();
        if (worldCanvas != null)
            _uiCam = worldCanvas.worldCamera;    // World Space 변환에 사용할 카메라
        if (_uiCam == null)
            _uiCam = Camera.main;                // 없으면 메인카메라
        if (slider == null)
            slider = GetComponentInParent<Slider>();
        if (trackRect == null && slider != null)
            trackRect = slider.GetComponent<RectTransform>();
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(handTag)) return;

        // 손가락 월드 위치 → 트랙 로컬 좌표로 변환
        Vector3 worldPos = other.bounds.center; // 콜라이더 중심점 사용(안 흔들림)
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(_uiCam, worldPos);
        Vector2 localOnTrack;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(trackRect, screenPos, _uiCam, out localOnTrack))
            return;

        // 트랙의 가로 방향으로 0..1 정규화 (Slider가 Left-To-Right 가정)
        float halfW = trackRect.rect.width * 0.5f;
        float t = Mathf.InverseLerp(-halfW + deadZone, halfW - deadZone, localOnTrack.x);
        t = Mathf.Clamp01(t);

        // 슬라이더 값 갱신
        float newValue = Mathf.Lerp(slider.minValue, slider.maxValue, t);
        // 드래그 중 UI 반짝이는 걸 막고 싶으면 SetValueWithoutNotify 로 바꿔도 됨
        slider.value = newValue;
    }
}
