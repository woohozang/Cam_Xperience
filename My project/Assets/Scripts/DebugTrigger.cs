using UnityEngine;

public class DebugTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Hand] Entered {other.name}");
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[Hand] Exited {other.name}");
    }
}
