using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DropOutsideZone : MonoBehaviour
{
    [Header("������ ���")]
    public XRGrabInteractable grab;   // ī�޶� XRGrabInteractable
    [Header("��� ��� ����(GrabZone �ݶ��̴�)")]
    public Collider grabZone;

    XRBaseInteractor currentInteractor;

    void OnEnable()
    {
        if (grab != null)
        {
            grab.selectEntered.AddListener(OnGrab);
            grab.selectExited.AddListener(OnRelease);
        }
    }

    void OnDisable()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnGrab);
            grab.selectExited.RemoveListener(OnRelease);
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject as XRBaseInteractor;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        currentInteractor = null;
    }

    void Update()
    {
        if (currentInteractor != null && grabZone != null)
        {
            // ���� ��(���ͷ���) ��ġ�� GrabZone �ٿ��� �ȿ� �ִ��� �˻�
            if (!grabZone.bounds.Contains(currentInteractor.transform.position))
            {
                grab.interactionManager.SelectExit(currentInteractor, grab);
                currentInteractor = null;
                Debug.Log("GrabZone ��� �ڵ� ����");
            }
        }
    }
}
