using UnityEngine;
using Oculus.Interaction;              // Interactable, InteractableState
using Oculus.Interaction.HandGrab;     // HandGrabInteractable

[RequireComponent(typeof(HandGrabInteractable))]
[RequireComponent(typeof(Animator))]
public class MetaButtonToggleAnimator : MonoBehaviour
{
    [SerializeField] private string boolParameterName = "IsRotated"; // Animator Bool 파라미터명

    private HandGrabInteractable _handGrab;
    private Animator _anim;
    private InteractableState _lastState;

    // 현재 다이얼이 '잡혀서 선택된 상태'인지 여부
    private bool IsSelected =>
        _handGrab != null && _handGrab.State == InteractableState.Select;

    private void Awake()
    {
        _handGrab = GetComponent<HandGrabInteractable>();
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // 다이얼을 실제로 잡고 있을 때만 A 버튼(오른손 RTouch)으로 토글
        if (IsSelected && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            bool cur = _anim.GetBool(boolParameterName);
            _anim.SetBool(boolParameterName, !cur);
        }

        // 선택 해제 시 원위치로 돌리고 싶다면 주석 해제
        // if (_lastState == InteractableState.Select && _handGrab.State != InteractableState.Select)
        // {
        //     _anim.SetBool(boolParameterName, false);
        // }

        _lastState = _handGrab.State;
    }
}
