using UnityEngine;

public class CameraSnapZone : MonoBehaviour
{
    [Header("ī�޶� ���� ��ġ")]
    public Transform snapPoint;

    private GameObject currentCamera;
    private bool isSnapped = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Camera") && !isSnapped)
        {
            // �׻� Rigidbody�� �޸� ��Ʈ(Camera_Test)�� ������
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
        // ���� ����� ���� ����
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

        // ��ġ/ȸ�� ���߱�
        rb.transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);

        // ���� ���� (�߷� X, �������� ����)
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
                rb.isKinematic = false; // �ٽ� ���� �� �ֵ��� ����
            }
        }
        isSnapped = false;
    }
}
