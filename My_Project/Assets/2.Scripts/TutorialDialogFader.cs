using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TutorialDialogFader : MonoBehaviour
{
    [Tooltip("페이드 시킬 다이얼로그 루트(각 루트에 CanvasGroup 필요)")]
    public List<GameObject> dialogRoots = new();

    [Header("Fade Settings")]
    public float fadeDuration = 0.4f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool startHidden = true;

    // (NEW) A로 한 번 띄워야 X가 동작하도록 하는 플래그
    [Header("Interaction Guard")]
    [Tooltip("A 버튼으로 한 번 Show 하기 전까지 X(Hide)를 무시합니다.")]
    public bool requireArmingWithA = true;

    private readonly List<CanvasGroup> groups = new();
    private Coroutine current;
    private bool isVisible = false;
    private bool armed = false; // (NEW)

    // (선택) 중복 토글 방지용 쿨다운
    private float lastToggleTime = -999f;
    public float toggleCooldown = 0.12f;

    void Awake()
    {
        groups.Clear();
        foreach (var go in dialogRoots)
        {
            if (!go) continue;

            var cg = go.GetComponent<CanvasGroup>();
            if (!cg) cg = go.AddComponent<CanvasGroup>();

            go.SetActive(true);

            cg.alpha = startHidden ? 0f : 1f;
            cg.interactable = !startHidden;
            cg.blocksRaycasts = !startHidden;

            groups.Add(cg);
        }

        isVisible = !startHidden;
        // 시작부터 보이는 상태라면 X 허용, 숨김이면 A로 무장하기 전까지 X 차단
        armed = isVisible || !requireArmingWithA;
    }

    void Update()
    {
        // --- A(오른손)로 Show ---
        bool rightTriggerHeld = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.1f;
        if (!rightTriggerHeld && OVRInput.GetDown(OVRInput.RawButton.A, OVRInput.Controller.RTouch))
        {
            Show();
            armed = true; // (NEW) A로 띄우면 X 허용
        }

        // --- X(왼손)로 Hide (무장 전이면 무시) ---
        if (armed)
        {
            bool leftTriggerHeld = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) > 0.1f;
            if (!leftTriggerHeld && OVRInput.GetDown(OVRInput.RawButton.X, OVRInput.Controller.LTouch))
            {
                Hide();
            }
        }
    }

    public void Show() => Toggle(true);
    public void Hide() => Toggle(false);

    private bool CanToggle()
    {
        return (Time.unscaledTime - lastToggleTime) > toggleCooldown;
    }

    private void Toggle(bool show)
    {
        // (NEW) 상태 동일이면 조기 종료 → 플래시 방지
        if (show == isVisible) return;

        if (!CanToggle()) return;

        if (current != null) StopCoroutine(current);
        current = StartCoroutine(FadeAll(show));
        lastToggleTime = Time.unscaledTime;
    }

    private IEnumerator FadeAll(bool show)
    {
        // VideoPlayer 동기화
        foreach (var g in groups)
        {
            var vp = g.GetComponentInChildren<VideoPlayer>(true);
            if (vp)
            {
                if (show) vp.Play();
                else vp.Pause();
            }
        }

        // (NEW) 각 그룹의 현재 알파에서 목표 알파로 보간 → 플래시 제거
        float[] startA = new float[groups.Count];
        float targetA = show ? 1f : 0f;
        for (int i = 0; i < groups.Count; i++)
            startA[i] = groups[i].alpha;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = fadeCurve.Evaluate(Mathf.Clamp01(t / fadeDuration));

            for (int i = 0; i < groups.Count; i++)
                groups[i].alpha = Mathf.Lerp(startA[i], targetA, k);

            yield return null;
        }

        foreach (var g in groups)
        {
            g.alpha = targetA;
            g.interactable = show;
            g.blocksRaycasts = show;
        }

        isVisible = show;
        current = null;
    }
}
