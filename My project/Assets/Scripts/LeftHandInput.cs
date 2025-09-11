using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class LeftHandInput : MonoBehaviour
{
    private InputDevice leftHand;

    void Start()
    {
        var leftHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
        if (leftHandDevices.Count > 0) leftHand = leftHandDevices[0];
    }

    public bool IsGrabPressed()
    {
        if (!leftHand.isValid) return false;

        bool triggerPressed = false;
        bool gripPressed = false;

        leftHand.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed);
        leftHand.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed);

        return triggerPressed && gripPressed;
    }
}
