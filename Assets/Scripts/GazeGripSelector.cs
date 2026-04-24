using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[DisallowMultipleComponent]
public class GazeGripSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XRGazeInteractor gazeInteractor;
    [SerializeField] private InputActionReference leftGripAction;

    [Header("Behavior")]
    [SerializeField] private bool autoEnableAction = true;

    private bool isSelecting;

    private void OnEnable()
    {
        if (leftGripAction != null && leftGripAction.action != null)
        {
            leftGripAction.action.performed += OnGripPressed;
            leftGripAction.action.canceled += OnGripReleased;

            if (autoEnableAction && !leftGripAction.action.enabled)
            {
                leftGripAction.action.Enable();
            }
        }
    }

    private void OnDisable()
    {
        if (leftGripAction != null && leftGripAction.action != null)
        {
            leftGripAction.action.performed -= OnGripPressed;
            leftGripAction.action.canceled -= OnGripReleased;
        }

        if (isSelecting && gazeInteractor != null)
        {
            gazeInteractor.EndManualInteraction();
        }

        isSelecting = false;
    }

    private void OnGripPressed(InputAction.CallbackContext context)
    {
        if (isSelecting || gazeInteractor == null)
        {
            return;
        }

        var hovered = gazeInteractor.interactablesHovered;
        for (int i = 0; i < hovered.Count; i++)
        {
            if (hovered[i] is IXRSelectInteractable selectable)
            {
                gazeInteractor.StartManualInteraction(selectable);
                isSelecting = true;
                return;
            }
        }
    }

    private void OnGripReleased(InputAction.CallbackContext context)
    {
        if (!isSelecting || gazeInteractor == null)
        {
            return;
        }

        gazeInteractor.EndManualInteraction();
        isSelecting = false;
    }
}
