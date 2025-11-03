using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VRButtonPressEffect : MonoBehaviour
{
[Header("Button Visual Settings")]
    public Transform buttonVisual;
    public float pressDepth = 0.01f;
    public float returnSpeed = 5f;
     public Vector3 pressDirection = Vector3.down;

    [Header("Haptic Feedback (OVR)")]
    public bool enableHaptics = true;
    public float hapticFrequency = 0.3f;
    public float hapticAmplitude = 0.2f;
    public float hapticDuration = 0.1f;

    private Vector3 initialPosition;
    private bool isPressed = false;

    private void Start()
    {
        if (buttonVisual == null)
        {
            Debug.LogWarning("[VRButtonPressEffect] ButtonVisual not assigned!");
            enabled = false;
            return;
        }
        initialPosition = buttonVisual.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand") && !isPressed)
        {
            isPressed = true;
            StopAllCoroutines();
            StartCoroutine(PressDown());

            // 햅틱 피드백
            if (enableHaptics)
            {
                OVRInput.SetControllerVibration(hapticFrequency, hapticAmplitude, OVRInput.Controller.RTouch);
                Invoke(nameof(StopHaptic), hapticDuration);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand") && isPressed)
        {
            isPressed = false;
            StopAllCoroutines();
            StartCoroutine(ReturnUp());

        }
    }

    private void StopHaptic()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }

    private IEnumerator PressDown()
    {
        Vector3 target = initialPosition + (pressDirection.normalized * pressDepth); // 누르는 방향 로컬 기준으로 적용

        while (Vector3.Distance(buttonVisual.localPosition, target) > 0.001f)
        {
            buttonVisual.localPosition = Vector3.Lerp(
                buttonVisual.localPosition,
                target,
                Time.deltaTime * returnSpeed
            );
            yield return null;
        }
    }

    private IEnumerator ReturnUp()
    {
        while (Vector3.Distance(buttonVisual.localPosition, initialPosition) > 0.001f)
        {
            buttonVisual.localPosition = Vector3.Lerp(
                buttonVisual.localPosition,
                initialPosition,
                Time.deltaTime * returnSpeed
            );
            yield return null;
        }
    }
}
