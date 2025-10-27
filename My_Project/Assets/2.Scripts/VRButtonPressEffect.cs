using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

public class VRButtonPressEffect : MonoBehaviour
{
    public Transform buttonVisual;
    public float pressDepth = 0.01f; // 눌릴 거리 (1cm)
    public float returnSpeed = 5f;

    private Vector3 initialPosition;
    private bool isPressed = false;

    private void Start()
    {
        initialPosition = buttonVisual.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand") && !isPressed)
        {
            isPressed = true;
            StopAllCoroutines();
            StartCoroutine(PressDown());
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

    private System.Collections.IEnumerator PressDown()
    {
        Vector3 target = initialPosition - new Vector3(0, pressDepth, 0);
        while (Vector3.Distance(buttonVisual.localPosition, target) > 0.001f)
        {
            buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, target, Time.deltaTime * returnSpeed);
            yield return null;
        }
    }

    private System.Collections.IEnumerator ReturnUp()
    {
        while (Vector3.Distance(buttonVisual.localPosition, initialPosition) > 0.001f)
        {
            buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, initialPosition, Time.deltaTime * returnSpeed);
            yield return null;
        }
    }
}
