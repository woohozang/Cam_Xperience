using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Main Camera (CenterEyeAnchor)를 할당하세요.")]
    public Transform cameraTransform; // 카메라의 방향을 알기 위해 필요

    [Header("Movement Settings")]
    public float moveSpeed = 2.0f;

    [Header("Rotation Settings")]
    public RotationType rotationType = RotationType.Snap;
    public float rotationSpeed = 60.0f;
    public float snapTurnAngle = 45.0f;
    private bool _isReadyToSnapTurn = true;

    public enum RotationType { Smooth, Snap }

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // [핵심 수정] 카메라가 바라보는 방향을 기준으로 하되, Y축(높이)은 무시하여 수평 이동만 하게 함
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // 입력값에 따라 이동 방향 결정
        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

        // 이동 적용
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        Vector2 rotationInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        switch (rotationType)
        {
            case RotationType.Smooth:
                transform.Rotate(0, rotationInput.x * rotationSpeed * Time.deltaTime, 0);
                break;

            case RotationType.Snap:
                if (Mathf.Abs(rotationInput.x) > 0.8f && _isReadyToSnapTurn)
                {
                    float angle = snapTurnAngle * Mathf.Sign(rotationInput.x);
                    transform.Rotate(0, angle, 0);
                    _isReadyToSnapTurn = false;
                }
                else if (Mathf.Abs(rotationInput.x) < 0.2f)
                {
                    _isReadyToSnapTurn = true;
                }
                break;
        }
    }
}