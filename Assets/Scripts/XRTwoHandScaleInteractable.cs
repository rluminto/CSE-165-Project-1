using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
[RequireComponent(typeof(XRGrabInteractable))]
public class XRTwoHandScaleInteractable : MonoBehaviour
{
    [Header("Resize Mode")]
    [SerializeField] private bool enableResize = true;
    [SerializeField] private Transform rightHandTransform;
    [SerializeField] private Transform leftHandTransform;
    [SerializeField] private float minUniformScale = 0.25f;
    [SerializeField] private float maxUniformScale = 3.5f;
    [SerializeField] private float minControllerDistance = 0.1f;

    private XRGrabInteractable grabInteractable;
    private InputDevice leftControllerDevice;
    private bool isResizing;
    private float startingControllerDistance;
    private Vector3 startingScale;

    private static readonly List<InputDevice> devices = new List<InputDevice>();

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        AutoAssignHandTransforms();
    }

    private void OnEnable()
    {
        AutoAssignHandTransforms();
    }

    private void Update()
    {
        if (!enableResize)
        {
            return;
        }

        AutoAssignHandTransforms();
        EnsureLeftControllerDevice();

        if (!IsLeftTriggerPressed())
        {
            isResizing = false;
            return;
        }

        if (!TryGetControllerDistance(out float currentDistance))
        {
            isResizing = false;
            return;
        }

        if (!isResizing)
        {
            startingControllerDistance = currentDistance;
            startingScale = transform.localScale;
            isResizing = true;
            return;
        }

        float scaleFactor = currentDistance / startingControllerDistance;
        Vector3 newScale = startingScale * scaleFactor;

        float maxComponent = Mathf.Max(newScale.x, Mathf.Max(newScale.y, newScale.z));
        if (maxComponent > maxUniformScale)
        {
            newScale *= maxUniformScale / maxComponent;
        }

        float minComponent = Mathf.Min(newScale.x, Mathf.Min(newScale.y, newScale.z));
        if (minComponent < minUniformScale)
        {
            newScale *= minUniformScale / Mathf.Max(minComponent, 0.0001f);
        }

        transform.localScale = newScale;

        Physics.SyncTransforms();

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
            collider.enabled = true;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.ResetCenterOfMass();
            rb.ResetInertiaTensor();
            rb.WakeUp();
        }


    }

    private void AutoAssignHandTransforms()
    {
        if (rightHandTransform == null)
        {
            GameObject rightHand = GameObject.Find("Right Controller");
            if (rightHand != null)
            {
                rightHandTransform = rightHand.transform;
            }
        }

        if (leftHandTransform == null)
        {
            GameObject leftHand = GameObject.Find("Left Controller");
            if (leftHand != null)
            {
                leftHandTransform = leftHand.transform;
            }
        }
    }

    private void EnsureLeftControllerDevice()
    {
        if (leftControllerDevice.isValid)
        {
            return;
        }

        devices.Clear();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
            devices);

        if (devices.Count > 0)
        {
            leftControllerDevice = devices[0];
        }
    }

    private bool IsLeftTriggerPressed()
    {
        return leftControllerDevice.isValid &&
               leftControllerDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed) &&
               pressed;
    }

    private bool TryGetControllerDistance(out float distance)
    {
        distance = 0f;

        if (rightHandTransform == null || leftHandTransform == null)
        {
            return false;
        }

        if (!IsHeldByRightHand())
        {
            return false;
        }

        distance = Vector3.Distance(rightHandTransform.position, leftHandTransform.position);
        return distance >= minControllerDistance;
    }

    private bool IsHeldByRightHand()
    {
        if (grabInteractable.interactorsSelecting.Count == 0 || rightHandTransform == null)
        {
            return false;
        }

        foreach (var interactor in grabInteractable.interactorsSelecting)
        {
            if (interactor == null || interactor.transform == null)
            {
                continue;
            }

            Transform interactorTransform = interactor.transform;
            if (interactorTransform == rightHandTransform ||
                interactorTransform.IsChildOf(rightHandTransform) ||
                rightHandTransform.IsChildOf(interactorTransform))
            {
                return true;
            }
        }

        return false;
    }
}
