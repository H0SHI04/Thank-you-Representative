using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DraggableStamp : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Stamp Settings")]
    public bool isApproveStamp; 
    public RectTransform policyCard; 
    public GameManagerScript gameManager;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startingPosition;
    private Coroutine returnCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        startingPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (returnCoroutine != null) StopCoroutine(returnCoroutine);
        
        canvasGroup.alpha = 0.8f; 
        canvasGroup.blocksRaycasts = false; 
        transform.SetAsLastSibling();
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (RectTransformUtility.RectangleContainsScreenPoint(policyCard, Input.mousePosition, eventData.pressEventCamera))
        {
            ApplyStamp();
        }

        returnCoroutine = StartCoroutine(SmoothReturn());
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

    private IEnumerator SmoothReturn()
    {
        float elapsedTime = 0f;
        float duration = 0.15f; 
        Vector2 currentPos = rectTransform.anchoredPosition;

        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(currentPos, startingPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = startingPosition;
    }
}