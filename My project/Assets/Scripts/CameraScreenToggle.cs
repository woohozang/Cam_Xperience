using UnityEngine;

public class CameraScreenToggle : MonoBehaviour
{
    public MeshRenderer lcdRenderer;       // DSLR LCD MeshRenderer
    public Texture defaultTexture;         // 기본 화면 (꺼짐 상태 이미지)
    public RenderTexture liveTexture;      // DSLR 카메라 RenderTexture
    public float cooldown = 0.5f;
    private bool isOn = false;
    private float lastPressTime = 0f;
    public bool IsOn => isOn;  // 셔터 버튼에서 참조 가능

    public void ToggleLCD()
    {
        isOn = !isOn;

        if (Time.time - lastPressTime < cooldown) return;
        lastPressTime = Time.time;
        if (isOn)
        {
            lcdRenderer.material.mainTexture = liveTexture;
            Debug.Log(" LCD ON: 실시간 렌더링 표시");
        }
        else
        {
            lcdRenderer.material.mainTexture = defaultTexture;
            Debug.Log(" LCD OFF: 기본 화면으로 전환");
        }
    }
}
