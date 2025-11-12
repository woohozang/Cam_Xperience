using UnityEngine;

public class LensZoomController : MonoBehaviour
{
    [Header("Target Camera")]
    public Camera lensCamera;

    [Header("Zoom (Field of View)")]
    [Tooltip("줌 아웃(넓은 화각)일 때의 FoV. (예: 84도)")]
    public float wideFoV = 84f;
    [Tooltip("줌 인(좁은 화각)일 때의 FoV. (예: 23도)")]
    public float telephotoFoV = 23f;

    [Tooltip("시작 시 적용할 기본 FoV 값")]
    public float defaultFoV = 60f; // [추가]

    [Header("Rotation Mapping")]
    [Tooltip("OneGrabRotateTransformer의 Min Angle (-45)과 일치")]
    public float minRotationAngle = -45f;
    [Tooltip("OneGrabRotateTransformer의 Max Angle (45)과 일치")]
    public float maxRotationAngle = 45f;

    public enum RotationAxis { X, Y, Z }
    [Tooltip("OneGrabRotateTransformer에서 회전을 허용한 축 (Y 또는 Z)")]
    public RotationAxis axisToRead = RotationAxis.Y;

    void Start() // [추가된 함수]
    {
        if (lensCamera != null)
        {
            // 1. 시작 시 카메라 FoV를 기본값 60으로 설정
            lensCamera.fieldOfView = defaultFoV;
        }

        // 2. [중요] 60 FoV에 해당하는 링의 '초기 각도'를 계산
        // 60 FoV가 0.0~1.0 사이의 어느 비율인지 계산 (InverseLerp)
        float initialNormalizedAngle = Mathf.InverseLerp(wideFoV, telephotoFoV, defaultFoV);
        initialNormalizedAngle = Mathf.Clamp01(initialNormalizedAngle);

        // 그 비율에 해당하는 실제 회전 각도를 계산 (Lerp)
        float initialAngle = Mathf.Lerp(minRotationAngle, maxRotationAngle, initialNormalizedAngle);

        // 3. 링(이 스크립트가 붙은 오브젝트)의 로컬 회전 값을 설정
        // (이래야 Update()가 첫 프레임부터 60 FoV를 읽음)
        Vector3 currentEuler = transform.localEulerAngles;
        switch (axisToRead)
        {
            case RotationAxis.X:
                transform.localEulerAngles = new Vector3(initialAngle, currentEuler.y, currentEuler.z);
                break;
            case RotationAxis.Y:
                transform.localEulerAngles = new Vector3(currentEuler.x, initialAngle, currentEuler.z);
                break;
            case RotationAxis.Z:
                transform.localEulerAngles = new Vector3(currentEuler.x, currentEuler.y, initialAngle);
                break;
        }
    }

    void Update()
    {
        if (lensCamera == null) return;

        // 1. 현재 설정된 축의 회전 각도를 읽습니다.
        float currentAngle = 0;
        Vector3 eulerAngles = transform.localEulerAngles;

        switch (axisToRead)
        {
            case RotationAxis.X:
                currentAngle = (eulerAngles.x > 180f) ? eulerAngles.x - 360f : eulerAngles.x;
                break;
            case RotationAxis.Y:
                currentAngle = (eulerAngles.y > 180f) ? eulerAngles.y - 360f : eulerAngles.y;
                break;
            case RotationAxis.Z:
                currentAngle = (eulerAngles.z > 180f) ? eulerAngles.z - 360f : eulerAngles.z;
                break;
        }

        // 2. 현재 각도를 0.0 ~ 1.0 비율로 정규화합니다.
        float normalizedAngle = Mathf.InverseLerp(minRotationAngle, maxRotationAngle, currentAngle);
        normalizedAngle = Mathf.Clamp01(normalizedAngle);

        // 3. 0~1 비율을 사용하여 FoV 값을 계산
        float newFoV = Mathf.Lerp(wideFoV, telephotoFoV, normalizedAngle);

        // 4. 렌즈 카메라의 Field of View 값을 업데이트합니다.
        lensCamera.fieldOfView = newFoV;
    }
}