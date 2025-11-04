using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRButtonAction_DoF : MonoBehaviour
{
    [Header("UI Controller Reference")]
    public GameObject dofCanvas; // 직접 GameObject로 참조 (활성화/비활성화 제어)

    [Header("Settings")]
    public float cooldown = 1.0f;

    private float lastPressTime = 0f;

    public void ToggleDoF()
    {
        // 전원 OFF 상태에서는 작동하지 않음
        if (!VRPowerManager.Instance.IsPowerOn)
        {
            Debug.Log("[DoF Button] Power is OFF — ignored.");
            return;
        }

        // 쿨다운 방지
        if (Time.time - lastPressTime < cooldown)
            return;

        lastPressTime = Time.time;

        if (dofCanvas != null)
        {
            bool newState = !dofCanvas.activeSelf;
            dofCanvas.SetActive(newState);
            Debug.Log($"[DoF Button] DoF Canvas {(newState ? "enabled" : "disabled")}");
        }
        else
        {
            Debug.LogWarning("[DoF Button] dofCanvas reference is missing!");
        }
    }
}
