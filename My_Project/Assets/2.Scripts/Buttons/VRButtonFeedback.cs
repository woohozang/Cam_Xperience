using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;


//-------------버튼을 인터랙션(눌림, 사운드, 햅틱)------------//
public class VRButtonFeedback : MonoBehaviour
{
    [Header("Button Visual Settings")]
    public Transform buttonVisual;
    public float pressDepth = 0.01f;
    public float returnSpeed = 5f;
    public Vector3 pressDirection = Vector3.down;

    [Header("Haptic Feedback")]
    public bool enableHaptics = true;
    public float hapticFrequency = 0.3f;
    public float hapticAmplitude = 0.2f;
    public float hapticDuration = 0.1f;


    [Header("Button Action Event")]
    public UnityEvent onButtonPressed;

    private Vector3 initialPosition;
    private bool isPressed = false;
    private bool isLocked = false;

    private void Start()
    {
        if (buttonVisual != null)
            initialPosition = buttonVisual.localPosition;
        else
            Debug.LogWarning($"{name}: ButtonVisual이 지정되지 않았습니다.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Hand") || other.CompareTag("LeftHand")) && !isPressed && !isLocked)
        {
            isPressed = true;
            isLocked = true;
            StopAllCoroutines();
            StartCoroutine(PressDown());


            //햅틱 피드백
            if (enableHaptics)
            {
                var controller = other.CompareTag("LeftHand") ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
                OVRInput.SetControllerVibration(hapticFrequency, hapticAmplitude, controller);
                Invoke(nameof(StopHaptic), hapticDuration);
            }

            //버튼 기능 호출
            onButtonPressed?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Hand") || other.CompareTag("LeftHand")) && isPressed)
        {
            isPressed = false;
            StopAllCoroutines();
            StartCoroutine(ReturnUp());
        }
    }

    private void StopHaptic()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }

    private IEnumerator PressDown()
    {
        Vector3 target = initialPosition + (pressDirection.normalized * pressDepth);
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

        // 잠금 해제, 딜레이로 연속 입력 방지함
        yield return new WaitForSeconds(0.1f);
        isLocked = false;
    }
}
