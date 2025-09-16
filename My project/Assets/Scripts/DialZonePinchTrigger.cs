using UnityEngine;

public class DialZonePinchTrigger : MonoBehaviour
{
    public GameObject pinchPoseObject; // RightHandPinchPose 프리팹 (Hierarchy에 있는 오브젝트)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand")) // 오른손에 "Hand" 태그 붙여주세요
        {
            Debug.Log("Enter Dial Zone → Pinch Pose ON");
            pinchPoseObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            Debug.Log("Exit Dial Zone → Pinch Pose OFF");
            pinchPoseObject.SetActive(false);
        }
    }
}
