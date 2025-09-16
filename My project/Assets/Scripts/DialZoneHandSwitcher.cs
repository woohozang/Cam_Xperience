using UnityEngine;

public class DialZoneHandSwitcher : MonoBehaviour
{
    public GameObject normalHand;     // OVRRightHandVisual
    public GameObject pinchHand;      // RightHandPinchPose


    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand")) // �����տ� "Hand" �±� �ޱ�
        {
            Debug.Log("Enter Dial Zone �� Switch to Pinch Hand");

            normalHand.SetActive(false);
            pinchHand.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            Debug.Log("Exit Dial Zone �� Switch back to Normal Hand");

            normalHand.SetActive(true);
            pinchHand.SetActive(false);
        }
    }
}
