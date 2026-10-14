using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableClutter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public RectTransform trashCan; 
    [HideInInspector] public GameManagerScript gameManager; // Added reference
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling(); 

        if (gameManager != null) gameManager.PlaySound(gameManager.paperRustleSound);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (trashCan != null && RectTransformUtility.RectangleContainsScreenPoint(trashCan, Input.mousePosition, eventData.pressEventCamera))
        {
            if (gameManager != null) gameManager.PlaySound(gameManager.paperRustleSound); // Rustle into trash
            Destroy(gameObject);
        }
    }
}