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

    private readonly List<CanvasGroup> groups = new();
    private Coroutine current;
    private bool isVisible = false;

    void Awake()
    {
        groups.Clear();
        foreach (var go in dialogRoots)
        {
            if (!go) continue;

            // CanvasGroup 보장
            var cg = go.GetComponent<CanvasGroup>();
            if (!cg) cg = go.AddComponent<CanvasGroup>();

            // 활성화는 유지(알파만 0)해야 페이드가 보임
            go.SetActive(true);

            cg.alpha = startHidden ? 0f : 1f;
            cg.interactable = !startHidden;
            cg.blocksRaycasts = !startHidden;

            groups.Add(cg);
        }

        isVisible = !startHidden;
    }

    void Update()
    {
        // A 버튼 → Fade In
        if (OVRInput.GetDown(OVRInput.Button.One))
            Show();

        // X 버튼 → Fade Out
        if (OVRInput.GetDown(OVRInput.Button.Three))
            Hide();
    }

    public void Show() => Toggle(true);
    public void Hide() => Toggle(false);

    private void Toggle(bool show)
    {
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(FadeAll(show));
    }

    private IEnumerator FadeAll(bool show)
    {
        // 비디오가 있다면 상태 맞춰주기(선택 사항)
        foreach (var g in groups)
        {
            var vp = g.GetComponentInChildren<VideoPlayer>(true);
            if (vp)
            {
                if (show) vp.Play();
                else vp.Pause();
            }
        }

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = fadeCurve.Evaluate(Mathf.Clamp01(t / fadeDuration));
            float a = show ? k : 1f - k;

            foreach (var g in groups)
                g.alpha = a;

            yield return null;
        }

        foreach (var g in groups)
        {
            g.alpha = show ? 1f : 0f;
            g.interactable = show;
            g.blocksRaycasts = show;
        }

        isVisible = show;
        current = null;
    }
}
