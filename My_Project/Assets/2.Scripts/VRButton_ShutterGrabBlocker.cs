using UnityEngine;

public class VRButton_ShutterGrabBlocker : MonoBehaviour
{
    [Header("오른손 그랩 오브젝트")]
    public GameObject rightHandGrabInteractable;

    [Header("Enter 시 비활성화, Exit 시 활성화 여부")]
    public bool autoEnableOnExit = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hand")) return;

        // 오른손일 경우만 처리하려면 추가
        if (!other.name.Contains("Right")) return;

        if (rightHandGrabInteractable != null)
        {
            rightHandGrabInteractable.SetActive(false);
            Debug.Log("[GrabBlocker] Right Hand Grab disabled (in shutter area).");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Hand")) return;
        if (!other.name.Contains("Right")) return;

        if (autoEnableOnExit && rightHandGrabInteractable != null)
        {
            rightHandGrabInteractable.SetActive(true);
            Debug.Log("[GrabBlocker] Right Hand Grab ENABLED (exit shutter area).");
        }
    }
}
