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
        // �޼����� Ȯ��
        var interactor = args.interactorObject as XRDirectInteractor;
        if (interactor != null && interactor.name.Contains("LeftHand"))
        {
            // �޼� ��Ʈ�ѷ� ��������
            var controller = interactor.GetComponent<XRBaseControllerInteractor>()?.xrController;
            if (controller != null)
            {
                // ����: ���� 0.5, ���ӽð� 0.2��
                controller.SendHapticImpulse(0.5f, 0.2f);
            }
        }
    }
}
