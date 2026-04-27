using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SpawnManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject binPrefab;
    public GameObject bedPrefab;

    [Header("Input")]
    public InputActionReference spawnBinAction;
    public InputActionReference spawnBedAction;

    [Header("Spawn")]
    public Transform spawnOrigin;
    public float spawnDistance = 1.5f;
    public float spawnUpOffset = 0.75f;
    public bool keepObjectsUpright = true;

    [Header("Floor Snap")]
    public bool snapToFloor = true;
    public LayerMask floorMask;
    public float floorRayStartHeight = 3f;
    public float floorRayLength = 10f;
    public float surfaceOffset = 0.01f;

    private void OnEnable()
    {
        RegisterAction(spawnBinAction, OnSpawnBin);
        RegisterAction(spawnBedAction, OnSpawnBed);
    }

    private void OnDisable()
    {
        UnregisterAction(spawnBinAction, OnSpawnBin);
        UnregisterAction(spawnBedAction, OnSpawnBed);
    }

    public void SpawnBin()
    {
        SpawnObject(binPrefab);
    }

    public void SpawnBed()
    {
        SpawnObject(bedPrefab);
    }

    private void OnSpawnBin(InputAction.CallbackContext context)
    {
        SpawnBin();
    }

    private void OnSpawnBed(InputAction.CallbackContext context)
    {
        SpawnBed();
    }

    private void SpawnObject(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("SpawnManager: Missing prefab.");
            return;
        }

        if (spawnOrigin == null)
        {
            Debug.LogError("SpawnManager: Missing spawn origin.");
            return;
        }

        Vector3 flatForward = GetFlatForward(spawnOrigin.forward);
        Vector3 spawnPosition = spawnOrigin.position + flatForward * spawnDistance + Vector3.up * spawnUpOffset;

        Quaternion spawnRotation = keepObjectsUpright
            ? Quaternion.LookRotation(flatForward, Vector3.up)
            : spawnOrigin.rotation;

        GameObject spawnedObject = Instantiate(prefab, spawnPosition, spawnRotation);

        ConfigureInteractable(spawnedObject);

        if (snapToFloor)
        {
            SnapToFloor(spawnedObject, spawnPosition);
        }
    }

    private void ConfigureInteractable(GameObject obj)
    {
        XRGrabInteractable grabInteractable = GetOrAddComponent<XRGrabInteractable>(obj);
        grabInteractable.selectMode = InteractableSelectMode.Multiple;
        grabInteractable.allowGazeInteraction = true;
        grabInteractable.allowGazeSelect = true;

        GetOrAddComponent<XRInteractableHighlighter>(obj);
        GetOrAddComponent<XRTwoHandScaleInteractable>(obj);

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void SnapToFloor(GameObject obj, Vector3 approximatePosition)
    {
        Vector3 rayStart = approximatePosition + Vector3.up * floorRayStartHeight;
        Ray ray = new Ray(rayStart, Vector3.down);

        int mask = floorMask.value == 0 ? Physics.DefaultRaycastLayers : floorMask.value;
        RaycastHit[] hits = Physics.RaycastAll(ray, floorRayLength, mask, QueryTriggerInteraction.Ignore);

        if (hits.Length == 0)
        {
            return;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            if (hit.collider.transform.IsChildOf(obj.transform))
            {
                continue;
            }

            if (hit.collider.CompareTag("InteractableItems"))
            {
                continue;
            }

            float bottomOffset = GetBottomOffsetFromPivot(obj);
            Vector3 position = obj.transform.position;
            position.y = hit.point.y + bottomOffset + surfaceOffset;
            obj.transform.position = position;

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            return;
        }
    }

    private float GetBottomOffsetFromPivot(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();

        if (colliders.Length == 0)
        {
            return 0.5f;
        }

        Bounds bounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }

        return Mathf.Max(0.01f, obj.transform.position.y - bounds.min.y);
    }

    private static Vector3 GetFlatForward(Vector3 forward)
    {
        Vector3 flatForward = new Vector3(forward.x, 0f, forward.z).normalized;
        return flatForward.sqrMagnitude < 0.001f ? Vector3.forward : flatForward;
    }

    private static void RegisterAction(InputActionReference actionReference, Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        actionReference.action.performed += callback;
        actionReference.action.Enable();
    }

    private static void UnregisterAction(InputActionReference actionReference, Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        actionReference.action.performed -= callback;
        actionReference.action.Disable();
    }

    private static T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        return component != null ? component : obj.AddComponent<T>();
    }
}
