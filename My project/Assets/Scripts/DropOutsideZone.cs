using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DropOutsideZone : MonoBehaviour
{
    [Header("잡히는 대상")]
    public XRGrabInteractable grab;   // 카메라 XRGrabInteractable
    [Header("잡기 허용 범위(GrabZone 콜라이더)")]
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
            // 현재 손(인터랙터) 위치가 GrabZone 바운즈 안에 있는지 검사
            if (!grabZone.bounds.Contains(currentInteractor.transform.position))
            {
                grab.interactionManager.SelectExit(currentInteractor, grab);
                currentInteractor = null;
                Debug.Log("GrabZone 벗어나 자동 해제");
            }
        }
    }
}
