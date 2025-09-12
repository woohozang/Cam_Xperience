using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class DialRotationTransformer : MonoBehaviour, ITransformer
{
    private HandGrabInteractor _interactor;
    private Quaternion _initialLocalRotation;
    private Quaternion _grabbedHandRotation;

    public Vector3 rotationAxis = Vector3.up; // Y�� ���� ȸ��
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
        // ���� ȸ�� ��ȭ�� ���
        Quaternion delta = Quaternion.Inverse(_grabbedHandRotation) * _interactor.transform.rotation;
        float angle;
        Vector3 axis;
        delta.ToAngleAxis(out angle, out axis);

        // �� ���� (���/���� ����)
        float signedAngle = Vector3.Dot(axis, rotationAxis) * angle;

        // ���̾� ���� ȸ�� ����
        float clamped = Mathf.Clamp(signedAngle, minAngle, maxAngle);
        transform.localRotation = Quaternion.AngleAxis(clamped, rotationAxis) * _initialLocalRotation;
    }

    public void EndTransform() { }
}
