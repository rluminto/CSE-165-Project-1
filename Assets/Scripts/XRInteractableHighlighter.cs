using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[DisallowMultipleComponent]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable))]
public class XRInteractableHighlighter : MonoBehaviour
{
    [Header("Highlight Colors")]
    [SerializeField] private Color hoverColor = new Color(1f, 0.85f, 0.25f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.25f, 1f, 0.65f, 1f);

    [Header("Targets")]
    [SerializeField] private bool autoFindRenderers = true;
    [SerializeField] private Renderer[] targetRenderers;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
    private MaterialPropertyBlock propertyBlock;
    private int hoverCount;
    private int selectCount;

    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        if ((targetRenderers == null || targetRenderers.Length == 0) && autoFindRenderers)
        {
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }

        propertyBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEntered);
        interactable.hoverExited.RemoveListener(OnHoverExited);
        interactable.selectEntered.RemoveListener(OnSelectEntered);
        interactable.selectExited.RemoveListener(OnSelectExited);

        hoverCount = 0;
        selectCount = 0;
        ClearHighlight();
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        hoverCount++;
        RefreshHighlight();
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        hoverCount = Mathf.Max(0, hoverCount - 1);
        RefreshHighlight();
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        selectCount++;
        RefreshHighlight();
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        selectCount = Mathf.Max(0, selectCount - 1);
        RefreshHighlight();
    }

    private void RefreshHighlight()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            return;
        }

        if (selectCount > 0)
        {
            ApplyColor(selectedColor);
            return;
        }

        if (hoverCount > 0)
        {
            ApplyColor(hoverColor);
            return;
        }

        ClearHighlight();
    }

    private void ApplyColor(Color color)
    {
        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer == null)
            {
                continue;
            }

            propertyBlock.Clear();
            propertyBlock.SetColor(ColorProperty, color);
            propertyBlock.SetColor(BaseColorProperty, color);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void ClearHighlight()
    {
        if (targetRenderers == null)
        {
            return;
        }

        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer != null)
            {
                renderer.SetPropertyBlock(null);
            }
        }
    }
}
