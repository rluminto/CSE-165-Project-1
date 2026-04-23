using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject binPrefab;
    public GameObject bedPrefab;

    [Header("Spawn Origin")]
    public Transform spawnOrigin;   // Drag Right Controller here
    public bool useMainCameraIfMissing = true;

    [Header("Spawn Settings")]
    public float spawnDistance = 1.5f;
    public float spawnUpOffset = 0.75f;
    public bool keepObjectsUpright = true;

    [Header("Floor Snap")]
    public bool snapToFloor = true;
    public LayerMask floorMask;     // Optional: assign Floor layer if you make one
    public float floorRayStartHeight = 3f;
    public float floorRayLength = 10f;
    public float surfaceOffset = 0.01f;

    [Header("Editor Test Keys")]
    public bool enableKeyboardTesting = true;
    public KeyCode spawnBinKey = KeyCode.Alpha1;
    public KeyCode spawnBedKey = KeyCode.Alpha2;

    private void Start()
    {
        if (spawnOrigin == null && useMainCameraIfMissing && Camera.main != null)
        {
            spawnOrigin = Camera.main.transform;
            Debug.LogWarning("SpawnManager: spawnOrigin was not assigned, so Camera.main is being used.");
        }
    }

    private void Update()
    {
        // Temporary editor testing
        if (!enableKeyboardTesting) return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                SpawnBin();
            }

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                SpawnBed();
            }
        }
    }

    public void SpawnBin()
    {
        SpawnObject(binPrefab);
    }

    public void SpawnBed()
    {
        SpawnObject(bedPrefab);
    }

    private void SpawnObject(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("SpawnManager: Prefab is missing.");
            return;
        }

        if (spawnOrigin == null)
        {
            Debug.LogError("SpawnManager: No spawn origin assigned.");
            return;
        }

        Vector3 forward = spawnOrigin.forward;
        Vector3 flatForward = new Vector3(forward.x, 0f, forward.z).normalized;

        // If controller/camera is pointing almost straight up/down, fall back to world forward
        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = Vector3.forward;
        }

        Vector3 spawnPos = spawnOrigin.position + flatForward * spawnDistance + Vector3.up * spawnUpOffset;

        Quaternion spawnRot;
        if (keepObjectsUpright)
        {
            spawnRot = Quaternion.LookRotation(flatForward, Vector3.up);
        }
        else
        {
            spawnRot = spawnOrigin.rotation;
        }

        GameObject spawned = Instantiate(prefab, spawnPos, spawnRot);

        if (snapToFloor)
        {
            SnapSpawnedObjectToFloor(spawned, spawnPos);
        }
    }

    private void SnapSpawnedObjectToFloor(GameObject obj, Vector3 approxSpawnPos)
    {
        if (obj == null) return;

        Vector3 rayStart = approxSpawnPos + Vector3.up * floorRayStartHeight;
        Ray ray = new Ray(rayStart, Vector3.down);

        RaycastHit hit;
        bool hitSomething;

        if (floorMask.value == 0)
        {
            hitSomething = Physics.Raycast(ray, out hit, floorRayLength);
        }
        else
        {
            hitSomething = Physics.Raycast(ray, out hit, floorRayLength, floorMask);
        }

        if (!hitSomething) return;

        float halfHeight = GetObjectHalfHeight(obj);
        Vector3 pos = obj.transform.position;
        pos.y = hit.point.y + halfHeight + surfaceOffset;
        obj.transform.position = pos;
    }

    private float GetObjectHalfHeight(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();

        if (colliders.Length == 0)
        {
            return 0.5f; // fallback if no collider found
        }

        Bounds bounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }

        return bounds.extents.y;
    }
}