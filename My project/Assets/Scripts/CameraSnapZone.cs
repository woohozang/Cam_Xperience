using UnityEngine;

public class CameraSnapZone : MonoBehaviour
{
    [Header("카메라가 붙을 위치")]
    public Transform snapPoint;

    private GameObject currentCamera;
    private bool isSnapped = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Camera") && !isSnapped)
        {
            // 항상 Rigidbody가 달린 루트(Camera_Test)를 가져옴
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                currentCamera = rb.gameObject;
                SnapToPoint(rb);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 영역 벗어나면 상태 해제
        if (other.attachedRigidbody != null &&
            other.attachedRigidbody.gameObject == currentCamera)
        {
            currentCamera = null;
            isSnapped = false;
        }
    }

    private void SnapToPoint(Rigidbody rb)
    {
        if (rb == null) return;

        // 위치/회전 맞추기
        rb.transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);

        // 물리 고정 (중력 X, 움직이지 않음)
        rb.isKinematic = true;

        isSnapped = true;
    }

    public void ReleaseFromSnap()
    {
        if (currentCamera != null)
        {
            Rigidbody rb = currentCamera.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // 다시 잡을 수 있도록 해제
            }
        }
        isSnapped = false;
    }
}
