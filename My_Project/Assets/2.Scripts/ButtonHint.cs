using System.Collections;
using UnityEngine;

public class ControlsHintOnce : MonoBehaviour
{
    [Header("Target")]
    public CanvasGroup hintGroup;         // 안내 패널의 CanvasGroup

    [Header("Fade")]
    public float fadeInDuration = 0.35f;
    public float fadeOutDuration = 0.25f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Persistence")]
    public bool rememberAcrossRuns = false;   // true면 다음 실행 때도 안 뜸
    public string prefsKey = "ControlsHintShown";

    bool isShown = false;
    Coroutine anim;

    void Start()
    {
        if (!hintGroup) { Debug.LogWarning("ControlsHintOnce: hintGroup is not set."); return; }

        // 다음 실행에서도 안 보이게 하려면 PlayerPrefs 체크
        if (rememberAcrossRuns && PlayerPrefs.GetInt(prefsKey, 0) == 1)
        {
            hintGroup.alpha = 0f;
            hintGroup.interactable = false;
            hintGroup.blocksRaycasts = false;
            return;
        }

        // 이번 세션 첫 시작에만 표시
        ShowHint();
    }

    void Update()
    {
        // A 버튼 누르면 1회성으로 사라짐
        if (isShown && OVRInput.GetDown(OVRInput.Button.One))
        {
            HideHintOnce();
        }
    }

    public void ShowHint()
    {
        if (anim != null) StopCoroutine(anim);
        hintGroup.gameObject.SetActive(true);
        hintGroup.interactable = false;
        hintGroup.blocksRaycasts = false;
        anim = StartCoroutine(Fade(hintGroup, hintGroup.alpha, 1f, fadeInDuration));
        isShown = true;
    }

    public void HideHintOnce()
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(Fade(hintGroup, hintGroup.alpha, 0f, fadeOutDuration, () =>
        {
            hintGroup.gameObject.SetActive(false);
            isShown = false;
            if (rememberAcrossRuns)
            {
                PlayerPrefs.SetInt(prefsKey, 1);
                PlayerPrefs.Save();
            }
        }));
    }

    IEnumerator Fade(CanvasGroup g, float a0, float a1, float dur, System.Action onDone = null)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = curve.Evaluate(Mathf.Clamp01(t / dur));
            g.alpha = Mathf.Lerp(a0, a1, k);
            yield return null;
        }
        g.alpha = a1;
        onDone?.Invoke();
    }
}
