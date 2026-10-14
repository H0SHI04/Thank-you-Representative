using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableStamp : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Stamp Settings")]
    public bool isApproveStamp; // Check this for the Approve stamp, uncheck for Veto
    public RectTransform policyCard; // Drag your Policy Card UI panel here
    public GameManagerScript gameManager;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startingPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        // Remember where the stamp sits on the desk so it can snap back
        startingPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.8f; // Make it slightly transparent while dragging
        canvasGroup.blocksRaycasts = false; // Allow the mouse to "see" through the stamp to the card below
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move the stamp with the mouse
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check if we dropped the stamp on top of the Policy Card
        if (RectTransformUtility.RectangleContainsScreenPoint(policyCard, Input.mousePosition, eventData.pressEventCamera))
        {
            ApplyStamp();
        }

        // Snap the stamp back to its resting place on the desk
        rectTransform.anchoredPosition = startingPosition;
    }

    private void ApplyStamp()
    {
        if (isApproveStamp)
        {
            gameManager.ApprovePolicy();
        }
        else
        {
            gameManager.VetoPolicy();
        }
    }
}