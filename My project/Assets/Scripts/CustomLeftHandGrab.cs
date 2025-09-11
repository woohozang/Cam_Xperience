using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class CustomLeftHandGrab : XRDirectInteractor
{
    public InputActionProperty gripAction;
    public InputActionProperty triggerAction;

    public override bool CanSelect(XRBaseInteractable interactable)
    {
        bool grip = gripAction.action.ReadValue<float>() > 0.5f;
        bool trigger = triggerAction.action.ReadValue<float>() > 0.5f;

        // Grip + Trigger ���ÿ� ������ ���� Grab ����
        return grip && trigger && base.CanSelect(interactable);
        Debug.Log($"Grip={grip}, Trigger={trigger}, Base.CanSelect={base.CanSelect(interactable)}");
    }
}
