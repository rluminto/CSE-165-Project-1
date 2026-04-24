using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
[RequireComponent(typeof(XRBaseInteractable))]
public class XRInteractableHighlighter : MonoBehaviour
{
    [Header("Highlight Colors")]
    [SerializeField] private Color hoverColor = new Color(1f, 0.85f, 0.25f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.25f, 1f, 0.65f, 1f);

    [Header("Targets")]
    [SerializeField] private bool autoFindRenderers = true;
    [SerializeField] private Renderer[] targetRenderers;

    private XRBaseInteractable interactable;
    private MaterialPropertyBlock propertyBlock;
    private int hoverCount;
    private int selectCount;

    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        if ((targetRenderers == null || targetRenderers.Length == 0) && autoFindRenderers)
        {
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }

        propertyBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        if (interactable == null)
        {
            return;
        }

        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
        }

        ClearHighlight();
        hoverCount = 0;
        selectCount = 0;
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        hoverCount++;
        ApplyCurrentColor();
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        hoverCount = Mathf.Max(0, hoverCount - 1);
        ApplyCurrentColor();
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        selectCount++;
        ApplyCurrentColor();
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        selectCount = Mathf.Max(0, selectCount - 1);
        ApplyCurrentColor();
    }

    private void ApplyCurrentColor()
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
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer renderer = targetRenderers[i];
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

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer renderer = targetRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.SetPropertyBlock(null);
        }
    }
}
