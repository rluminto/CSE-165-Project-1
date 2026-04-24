using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
[RequireComponent(typeof(XRGrabInteractable))]
public class XRTwoHandScaleInteractable : MonoBehaviour
{
    [Header("Two-Hand Scale")]
    [SerializeField] private bool enableTwoHandScale = true;
    [SerializeField] private float minUniformScale = 0.25f;
    [SerializeField] private float maxUniformScale = 3.5f;
    [SerializeField] private float minHandDistance = 0.02f;

    private XRGrabInteractable grabInteractable;
    private bool isScaling;
    private float startingHandDistance;
    private Vector3 startingScale;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        if (grabInteractable == null)
        {
            return;
        }

        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }

        isScaling = false;
    }

    private void Update()
    {
        if (!enableTwoHandScale || grabInteractable == null)
        {
            return;
        }

        if (grabInteractable.interactorsSelecting.Count < 2)
        {
            isScaling = false;
            return;
        }

        float currentHandDistance = GetCurrentHandDistance();
        if (currentHandDistance < minHandDistance)
        {
            return;
        }

        if (!isScaling)
        {
            BeginScaling(currentHandDistance);
            return;
        }

        float scaleFactor = currentHandDistance / startingHandDistance;
        Vector3 newScale = startingScale * scaleFactor;

        float largestComponent = Mathf.Max(newScale.x, Mathf.Max(newScale.y, newScale.z));
        if (largestComponent > maxUniformScale)
        {
            newScale *= maxUniformScale / largestComponent;
        }

        float smallestComponent = Mathf.Min(newScale.x, Mathf.Min(newScale.y, newScale.z));
        if (smallestComponent < minUniformScale)
        {
            newScale *= minUniformScale / Mathf.Max(0.0001f, smallestComponent);
        }

        transform.localScale = newScale;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (grabInteractable != null && grabInteractable.interactorsSelecting.Count >= 2)
        {
            float currentHandDistance = GetCurrentHandDistance();
            if (currentHandDistance >= minHandDistance)
            {
                BeginScaling(currentHandDistance);
            }
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (grabInteractable == null)
        {
            return;
        }

        if (grabInteractable.interactorsSelecting.Count >= 2)
        {
            float currentHandDistance = GetCurrentHandDistance();
            if (currentHandDistance >= minHandDistance)
            {
                BeginScaling(currentHandDistance);
            }
        }
        else
        {
            isScaling = false;
        }
    }

    private void BeginScaling(float currentHandDistance)
    {
        isScaling = true;
        startingHandDistance = currentHandDistance;
        startingScale = transform.localScale;
    }

    private float GetCurrentHandDistance()
    {
        if (grabInteractable == null || grabInteractable.interactorsSelecting.Count < 2)
        {
            return 0f;
        }

        Transform firstHand = grabInteractable.interactorsSelecting[0].transform;
        Transform secondHand = grabInteractable.interactorsSelecting[1].transform;

        if (firstHand == null || secondHand == null)
        {
            return 0f;
        }

        return Vector3.Distance(firstHand.position, secondHand.position);
    }
}
