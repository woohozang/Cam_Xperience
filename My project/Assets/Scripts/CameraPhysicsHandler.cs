using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class CameraPhysicsHandler : MonoBehaviour
{
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        rb.isKinematic = true; // ¼Õ¿¡ µé·ÈÀ» ¶§ ¹°¸® ²¨Áü
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        rb.isKinematic = false; // ³õÀ¸¸é Áß·Â ¹Þ¾Æ¼­ ¶³¾îÁü
    }
}
