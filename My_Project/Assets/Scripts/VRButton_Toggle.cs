using UnityEngine;

public class VRButton_Toggle : MonoBehaviour
{
    public DialUIController uiController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand")) // 손가락 오브젝트 태그
        {
            uiController.ToggleUI(); //  이제 오류 안 남
        }
    }
}
