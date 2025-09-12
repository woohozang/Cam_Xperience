using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class DialRotationTransformer : MonoBehaviour, ITransformer
{
    private HandGrabInteractor _interactor;
    private Quaternion _initialLocalRotation;
    private Quaternion _grabbedHandRotation;

    public Vector3 rotationAxis = Vector3.up; // Y축 기준 회전
    public float minAngle = 0f;
    public float maxAngle = 270f;

    public void Initialize(IGrabbable grabbable)
    {
        _interactor = grabbable as HandGrabInteractor;
        _initialLocalRotation = transform.localRotation;
        _grabbedHandRotation = _interactor.transform.rotation;
    }

    public void BeginTransform()
    {
        _grabbedHandRotation = _interactor.transform.rotation;
        _initialLocalRotation = transform.localRotation;
    }

    public void UpdateTransform()
    {
        // 손의 회전 변화량 계산
        Quaternion delta = Quaternion.Inverse(_grabbedHandRotation) * _interactor.transform.rotation;
        float angle;
        Vector3 axis;
        delta.ToAngleAxis(out angle, out axis);

        // 축 정렬 (양수/음수 보정)
        float signedAngle = Vector3.Dot(axis, rotationAxis) * angle;

        // 다이얼 로컬 회전 적용
        float clamped = Mathf.Clamp(signedAngle, minAngle, maxAngle);
        transform.localRotation = Quaternion.AngleAxis(clamped, rotationAxis) * _initialLocalRotation;
    }

    public void EndTransform() { }
}
