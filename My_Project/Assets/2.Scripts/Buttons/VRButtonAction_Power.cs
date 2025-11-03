using UnityEngine;


public class VRButtonAction_Power : MonoBehaviour
{
    [Header("LCD Control References")]
    public CameraScreenController screenController;  // LCD 화면 제어용
    public Texture defaultTexture;                   // 꺼졌을 때 기본 화면
    public Texture liveTexture;                      // 켜졌을 때 렌더 텍스처
    public float cooldown = 0.5f;                    // 버튼 연속 입력 방지 시간

    [Header("UI Elements (Optional)")]
    public GameObject[] uiElementsToDisable; // 전원 OFF 시 닫을 UI들

    private bool isOn = false;
    private float lastPressTime = 0f;


    public void TogglePower()
    {
        if (Time.time - lastPressTime < cooldown)
            return;

        lastPressTime = Time.time;
        isOn = !isOn;

        if (isOn)
        {
            screenController.ShowPhoto(liveTexture);
        }
        else
        {
            screenController.TurnOff(defaultTexture);
            TurnOffUIElements();
        }

        VRPowerManager.Instance.TogglePower(isOn);
    }

    private void TurnOffUIElements()
    {
        if (uiElementsToDisable == null) return;

        foreach (var ui in uiElementsToDisable)
        {
            if (ui != null && ui.activeSelf)
            {
                ui.SetActive(false);
                Debug.Log($"[Power] UI '{ui.name}' turned off.");
            }
        }
    }
}
