using UnityEngine;

public class ButtonDebugToggle : MonoBehaviour
{
    public DialUIController uiController;

    private void OnTriggerEnter(Collider other)
    {
        bool isHand = other.CompareTag("Hand") ||
                      other.GetComponentInParent<OVRHand>() != null ||
                      other.GetComponentInParent<OVRSkeleton>() != null;

        Debug.Log($"[ButtonDebug] Trigger by: {other.name}, isHand={isHand}");

        if (isHand && uiController != null)
        {
            uiController.Toggle();
            Debug.Log("[ButtonDebug] Toggle() called.");
        }
        else if (uiController == null)
        {
            Debug.LogWarning("[ButtonDebug] uiController is NULL!");
        }
    }
}
