using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DialUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject worldUI;        // World UI Canvas
    public Slider progressSlider;     // 슬라이더
    public TextMeshProUGUI valueText; // 값 표시 텍스트

    [Header("Post Processing")]
    public Volume globalVolume;       // Global Volume (씬에 있는거 드래그)
    private DepthOfField dof;         // Depth of Field 컴포넌트

    private bool isVisible = false;

    void Start()
    {
        if (worldUI != null)
            worldUI.SetActive(false);

        // Global Volume에서 DoF 가져오기
        if (globalVolume != null)
        {
            if (globalVolume.profile.TryGet(out DepthOfField depthOfField))
            {
                dof = depthOfField;
            }
            else
            {
                Debug.LogWarning("Global Volume에 DepthOfField Override가 없습니다!");
            }
        }

        // 슬라이더 이벤트 연결
        if (progressSlider != null)
            progressSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    //  버튼으로 UI 토글
    public void ToggleUI()
    {
        isVisible = !isVisible;
        if (worldUI != null)
            worldUI.SetActive(isVisible);
    }

    //  슬라이더 값 변경 시 실행
    public void OnSliderChanged(float value)
    {
        if (valueText != null)
            valueText.text = $"Aperture: {value:F2}";

        if (dof != null)
        {
            // 슬라이더 값 → DoF Aperture (f-stop 값)
            dof.aperture.Override(value);
        }
    }
}
