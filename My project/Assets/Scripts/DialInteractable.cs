using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRSimpleInteractable))]
public class DialInteractable : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public Axis rotationAxis = Axis.Z;
    public Transform pivot;           // ȸ�� �߽� (null�̸� �ڱ� transform)
    public float minAngle = 0f;
    public float maxAngle = 300f;
    public int detents = 12;          // ��Ĭ ���� �� (�����̾��� 3~6, ���δ��̾��� 20~30 ����)

    public UnityEvent<int> OnTick;    // +1 / -1 �� ĭ �̵� �̺�Ʈ
    public UnityEvent<float> OnAngle; // 0..1 ����ȭ�� ���� ��ȭ(�ɼ�)

    XRSimpleInteractable _si;
    IXRSelectInteractor _interactor;
    bool _held;
    Vector3 _startVec;
    float _startAngle, _angle;
    int _lastStep;

    void Awake()
    {
        _si = GetComponent<XRSimpleInteractable>();
        _si.selectEntered.AddListener(OnSelectEntered);
        _si.selectExited.AddListener(OnSelectExited);
        if (pivot == null) pivot = transform;
        _lastStep = AngleToStep(_angle);
    }

    void Update()
    {
        if (!_held || _interactor == null) return;

        Vector3 axisW = AxisWorld();
        Vector3 v = VectorToInteractorOnPlane(axisW).normalized;
        float delta = Vector3.SignedAngle(_startVec, v, axisW);
        SetAngle(Mathf.Clamp(_startAngle + delta, minAngle, maxAngle));
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        _interactor = args.interactorObject;
        _held = true;
        Vector3 axisW = AxisWorld();
        _startVec = VectorToInteractorOnPlane(axisW).normalized;
        _startAngle = _angle;
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        _held = false;
        _interactor = null;
        // ���� ����� ����Ʈ ������ ����
        float t = Mathf.Round(Mathf.InverseLerp(minAngle, maxAngle, _angle) * (detents - 1)) / (detents - 1);
        SetAngle(Mathf.Lerp(minAngle, maxAngle, t));
    }

    void SetAngle(float a)
    {
        _angle = a;
        transform.localRotation = Quaternion.AngleAxis(_angle, AxisLocal());
        OnAngle?.Invoke(Mathf.InverseLerp(minAngle, maxAngle, _angle));

        int step = AngleToStep(_angle);
        if (step != _lastStep)
        {
            int dir = step > _lastStep ? 1 : -1;
            // ���� ����
            if (Mathf.Abs(step - _lastStep) > detents / 2) dir = -dir;

            _lastStep = step;
            OnTick?.Invoke(dir);

            // ��ƽ
            if (_interactor is XRBaseControllerInteractor c)
                c.xrController.SendHapticImpulse(0.15f, 0.06f);
        }
    }

    int AngleToStep(float a)
    {
        float t = Mathf.InverseLerp(minAngle, maxAngle, a);
        return Mathf.RoundToInt(t * (detents - 1));
    }

    Vector3 VectorToInteractorOnPlane(Vector3 axisW)
    {
        Transform attach = _interactor.GetAttachTransform(_si);
        Vector3 p = attach ? attach.position : ((Component)_interactor).transform.position;
        Vector3 v = p - (pivot ? pivot.position : transform.position);
        return Vector3.ProjectOnPlane(v, axisW);
    }

    Vector3 AxisLocal() =>
        rotationAxis == Axis.X ? Vector3.right :
        rotationAxis == Axis.Y ? Vector3.up : Vector3.forward;

    Vector3 AxisWorld() =>
        (pivot ? pivot.TransformDirection(AxisLocal()) : transform.TransformDirection(AxisLocal()));
}