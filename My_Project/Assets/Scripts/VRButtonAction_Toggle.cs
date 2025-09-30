using UnityEngine;

public class VRButtonAction_Toggle : MonoBehaviour
{
    public CameraScreenController screenController; // LCD 제어 스크립트
    public Texture defaultTexture; // LCD 꺼졌을 때 기본 이미지
    public Texture liveTexture;    // LCD 켰을 때 RenderTexture
    public float cooldown = 0.5f;

    private bool isOn = false; // LCD 현재 상태
    private float lastPressTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            if (Time.time - lastPressTime < cooldown) return;
            lastPressTime = Time.time;

            if (isOn)
            {
                // LCD 끄기
                screenController.TurnOff(defaultTexture);
                isOn = false;
                Debug.Log("LCD Off");
            }
            else
            {
                // LCD 켜기
                screenController.ShowPhoto(liveTexture);
                isOn = true;
                Debug.Log("LCD On");
            }
        }
    }
}
