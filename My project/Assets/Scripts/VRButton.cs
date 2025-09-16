using UnityEngine;

public class VRButtonAction_Toggle : MonoBehaviour
{
    public CameraScreenController screenController; // 스크린 컨트롤 스크립트
    public Texture defaultTexture; // 기본 이미지
    public Texture photoTexture;   // 촬영 화면
    public float cooldown = 0.5f;

    private bool isPhotoShown = false; // 현재 상태 저장
    private float lastPressTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand")) // 손가락에 Hand 태그 붙여야 함
        {
            if (Time.time - lastPressTime < cooldown) return;
            lastPressTime = Time.time;

            if (isPhotoShown)
            {
                // 기본 이미지로 복귀
                screenController.ShowPhoto(defaultTexture);
                isPhotoShown = false;
            }
            else
            {
                // 촬영 이미지로 전환
                screenController.ShowPhoto(photoTexture);
                isPhotoShown = true;
            }
        }
    }
}
