using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DialUIController : MonoBehaviour
{
    [Header("UI")]
    public Canvas worldCanvas;
    public Slider focusSlider;

    [Header("PostProcess")]
    public Volume globalVolume;
    private DepthOfField dof;

    void Awake()
    {
        if (globalVolume.profile.TryGet(out dof) == false)
            Debug.LogError("Depth Of Field override가 Global Volume에 필요합니다.");

        if (focusSlider != null)
            focusSlider.onValueChanged.AddListener(OnSliderChanged);

        // 시작 시 숨기기
        worldCanvas.gameObject.SetActive(false);
    }

    public void Toggle()
    {
        bool isActive = !worldCanvas.gameObject.activeSelf;
        worldCanvas.gameObject.SetActive(isActive);
        Debug.Log($"[DialUIController] World UI {(isActive ? "ON" : "OFF")}");
    }

    /*private void OnSliderChanged(float value)
    {
        if (dof == null) return;

        dof.mode.value = DepthOfFieldMode.Bokeh;
        dof.focusDistance.value = Mathf.Lerp(0.2f, 10f, value);
        dof.aperture.value = Mathf.Lerp(16f, 1.4f, value);
    }*/

    private void OnSliderChanged(float value)
    {
        if (dof == null) return;

        dof.mode.value = DepthOfFieldMode.Bokeh;

        // 슬라이더 방향 반전 (value=1 일 때 가까운 물체에 초점)
        float reversed = 1f - value;

        // 초점 거리 / 조리개 매핑 조정
        dof.focusDistance.value = Mathf.Lerp(0.3f, 8f, reversed);  // 가까운~먼 거리
        dof.aperture.value = Mathf.Lerp(16f, 1.4f, reversed);      // 조리개 값 (깊은 DOF얕은 DOF)
        dof.focusDistance.value = Mathf.Lerp(dof.focusDistance.value, Mathf.Lerp(0.3f, 8f, reversed), Time.deltaTime * 5f);

    }
}
