using UnityEngine;
using UnityEngine.UI;

public class VRSliderHandle : MonoBehaviour
{
    public Slider uiSlider;       // 연결할 Unity UI Slider
    public Transform handle;      // 움직이는 핸들 (자기 자신)
    public float minX = -0.05f;   // 최소 위치
    public float maxX = 0.05f;    // 최대 위치

    void Update()
    {
        float t = Mathf.InverseLerp(minX, maxX, handle.localPosition.x);
        uiSlider.value = t;
    }
}
