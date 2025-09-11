using System.Collections.Generic;
using UnityEngine;

public class CameraGrabZone : MonoBehaviour
{
    public GameObject cameraObject;
    public LeftHandInput leftHandInput;

    [HideInInspector] public bool isHandInside = false; // 외부에서 접근 가능
    private Transform grabbingHand = null;
    private bool isGrabbing = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LeftHand"))
        {
            isHandInside = true;
            grabbingHand = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LeftHand"))
        {
            isHandInside = false;
            grabbingHand = null;
        }
    }

    void Update()
    {
        if (isHandInside && grabbingHand != null)
        {
            if (leftHandInput.IsGrabPressed() && !isGrabbing)
            {
                cameraObject.transform.SetParent(grabbingHand);
                cameraObject.transform.localPosition = Vector3.zero;
                cameraObject.transform.localRotation = Quaternion.identity;
                isGrabbing = true;
            }
            else if (!leftHandInput.IsGrabPressed() && isGrabbing)
            {
                cameraObject.transform.SetParent(null);
                isGrabbing = false;
            }
        }
    }
}
