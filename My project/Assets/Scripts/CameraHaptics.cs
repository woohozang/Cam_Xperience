using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CameraHaptics : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // 왼손인지 확인
        var interactor = args.interactorObject as XRDirectInteractor;
        if (interactor != null && interactor.name.Contains("LeftHand"))
        {
            // 왼손 컨트롤러 가져오기
            var controller = interactor.GetComponent<XRBaseControllerInteractor>()?.xrController;
            if (controller != null)
            {
                // 진동: 강도 0.5, 지속시간 0.2초
                controller.SendHapticImpulse(0.5f, 0.2f);
            }
        }
    }
}
