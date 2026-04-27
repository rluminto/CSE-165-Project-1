using UnityEngine;
using UnityEngine.InputSystem;

using UnityEngine.XR.Interaction.Toolkit.Interactors;

[DisallowMultipleComponent]
public class GazeGripSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XRGazeInteractor gazeInteractor;
    [SerializeField] private InputActionReference leftGripAction;

    private bool isSelecting;

    private void OnEnable()
    {
        if (leftGripAction == null || leftGripAction.action == null)
        {
            Debug.LogWarning("GazeGripSelector: Left grip action is missing.");
            return;
        }

        leftGripAction.action.performed += OnGripPressed;
        leftGripAction.action.canceled += OnGripReleased;
        leftGripAction.action.Enable();
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

        foreach (UnityEngine.XR.Interaction.Toolkit.Interactables.IXRHoverInteractable hovered in gazeInteractor.interactablesHovered)
        {
            if (hovered is UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable selectable)
            {
                // Grip confirms selection for the object currently under gaze.
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
