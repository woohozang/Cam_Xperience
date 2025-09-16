using UnityEngine;

public class DialZonePinchTrigger : MonoBehaviour
{
    public GameObject pinchPoseObject; // RightHandPinchPose ������ (Hierarchy�� �ִ� ������Ʈ)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand")) // �����տ� "Hand" �±� �ٿ��ּ���
        {
            Debug.Log("Enter Dial Zone �� Pinch Pose ON");
            pinchPoseObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            Debug.Log("Exit Dial Zone �� Pinch Pose OFF");
            pinchPoseObject.SetActive(false);
        }
    }
}
