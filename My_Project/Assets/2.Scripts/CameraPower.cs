using UnityEngine;

public class CameraPower : MonoBehaviour
{
    [Header("Target Screen")]
    [Tooltip("머티리얼을 교체할 카메라 LCD 스크린의 Mesh Renderer")]
    public MeshRenderer lcdScreenRenderer;

    [Header("Materials")]
    [Tooltip("1단계에서 만든 검은색 머티리얼")]
    public Material blackScreenMaterial;
    [Tooltip("1단계에서 만든 스크린샷 머티리얼")]
    public Material onScreenMaterial;

    [Header("Rotation Logic")]
    [Tooltip("이 각도를 초과하면 ON으로 간주합니다. (예: 15)")]
    public float onAngleThreshold = -15f;

    // Z축 회전을 읽도록 고정 (OneGrabRotateTransformer의 Z축 설정과 일치)
    private const RotationAxis axisToRead = RotationAxis.Z;

    private bool isScreenOn = false; // 현재 화면 상태 (중복 교체 방지)

    // (이전 스크립트의 enum)
    private enum RotationAxis { X, Y, Z }


    void Start()
    {
        // [요구사항 1] 처음 시작할 때 검은 화면
        if (lcdScreenRenderer != null && blackScreenMaterial != null)
        {
            lcdScreenRenderer.material = blackScreenMaterial;
            isScreenOn = false;
        }
    }

    void Update()
    {
        if (lcdScreenRenderer == null) return;

        // 1. 현재 Z축 회전 각도를 읽습니다. (0 ~ -30)
        float currentAngle = 0;
        Vector3 eulerAngles = transform.localEulerAngles;

        // [수정] 0~360 범위를 -180~180 범위로 변환
        currentAngle = (eulerAngles.z > 180f) ? eulerAngles.z - 360f : eulerAngles.z;

        // 2. 'ON' 각도 임계값(-15) '미만'인지 확인 (즉, -30에 가까운지)
        if (currentAngle < onAngleThreshold)
        {
            // [수정] ON으로 돌리면 스크린샷 화면
            if (!isScreenOn && onScreenMaterial != null)
            {
                lcdScreenRenderer.material = onScreenMaterial;
                isScreenOn = true;
            }
        }
        else
        {
            // [수정] OFF로 돌리면 (즉, -15 ~ 0 사이면) 검은 화면
            if (isScreenOn && blackScreenMaterial != null)
            {
                lcdScreenRenderer.material = blackScreenMaterial;
                isScreenOn = false;
            }
        }
    }
}