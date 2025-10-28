using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// 다이얼 1클릭(애니메이션 1회)마다 Global Volume의 DoF/APERTURE와 Exposure를 증분
public class DialStepPostFXDriver : MonoBehaviour
{
    [Header("Target Volume (Global)")]
    public Volume globalVolume;

    [Header("증분 설정")]
    [Range(0.01f, 1f)] public float step = 0.25f; // 한 번 돌 때 0.25씩
    public bool wrapAround = true;                // 1.0을 넘으면 0으로 순환

    [Header("매핑 범위 (필요하면 수정)")]
    // Aperture는 얕은 심도(1.4) ~ 깊은 심도(16) 범위. Lerp 방향은 원하는 느낌대로 조정
    public float apertureMin = 16f;
    public float apertureMax = 1.4f;

    // PostExposure는 -5.0 ~ 2.1로 요청 범위 세팅
    public float exposureMin = -5.0f;
    public float exposureMax = 2.1f;

    [Tooltip("정규화 상태값 (0~1). 애니메이션 1회마다 step만큼 이동")]
    [Range(0f, 1f)] public float t = 0f;

    // 캐시
    private DepthOfField dof;
    private ColorAdjustments color;

    void Awake()
    {
        if (globalVolume == null)
        {
            Debug.LogError("[DialStepPostFXDriver] Global Volume이 비어 있습니다.");
            enabled = false; return;
        }

        var profile = globalVolume.profile;
        if (!profile.TryGet(out dof))
            Debug.LogError("Global Volume에 Depth Of Field Override를 추가하고 체크(활성)하세요.");
        if (!profile.TryGet(out color))
            Debug.LogError("Global Volume에 Color Adjustments Override를 추가하고 체크(활성)하세요.");

        Apply(); // 초기값 반영
    }

    // ── 애니메이션 이벤트에서 호출할 메서드들 ──────────────────────────────
    // 회전 애니메이션이 1회 완료될 때 Animation Event로 StepUp() 호출
    public void StepUp() { Step(+step); }
    public void StepDown() { Step(-step); }

    private void Step(float delta)
    {
        float newT = t + delta;

        if (wrapAround)
        {
            if (newT > 1f) newT -= 1f;
            if (newT < 0f) newT += 1f;
        }
        else
        {
            newT = Mathf.Clamp01(newT);
        }

        t = newT;
        Apply();
    }

    // 현재 t(0~1)를 실제 Aperture/Exposure 값으로 매핑하여 Global Volume에 반영
    private void Apply()
    {
        if (dof != null)
        {
            dof.mode.value = DepthOfFieldMode.Bokeh;
            dof.aperture.value = Mathf.Lerp(apertureMin, apertureMax, t);
        }

        if (color != null)
        {
            color.postExposure.value = Mathf.Lerp(exposureMin, exposureMax, t);
        }
    }
}
