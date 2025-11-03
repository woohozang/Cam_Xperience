using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPowerManager : MonoBehaviour
{
    public static VRPowerManager Instance { get; private set; }

    public bool IsPowerOn { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TogglePower(bool state)
    {
        IsPowerOn = state;
        Debug.Log($"[VRPowerManager] Power {(state ? "ON" : "OFF")}");
    }
}
