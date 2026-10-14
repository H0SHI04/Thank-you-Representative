using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DraggablePhone : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameManagerScript gameManager;
    
    [Header("Rolodex Contacts")]
    public RectTransform grassrootsContact;
    public RectTransform prFirmContact;
    public RectTransform corporateFixerContact;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startingPosition;
    private Coroutine returnCoroutine;

    private Vector3 normalScale = Vector3.one; 
    private Vector3 hoverScale = new Vector3(1.15f, 1.15f, 1.15f); 

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

        UpdateHoverEffect(grassrootsContact, eventData);
        UpdateHoverEffect(prFirmContact, eventData);
        UpdateHoverEffect(corporateFixerContact, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        grassrootsContact.localScale = normalScale;
        prFirmContact.localScale = normalScale;
        corporateFixerContact.localScale = normalScale;

        if (RectTransformUtility.RectangleContainsScreenPoint(grassrootsContact, Input.mousePosition, eventData.pressEventCamera))
        {
            gameManager.CallGrassroots();
        }
        else if (RectTransformUtility.RectangleContainsScreenPoint(prFirmContact, Input.mousePosition, eventData.pressEventCamera))
        {
            gameManager.CallPRFirm();
        }
        else if (RectTransformUtility.RectangleContainsScreenPoint(corporateFixerContact, Input.mousePosition, eventData.pressEventCamera))
        {
            gameManager.CallCorporateFixer();
        }

        returnCoroutine = StartCoroutine(SmoothReturn());
    }

    private void UpdateHoverEffect(RectTransform contactCard, PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(contactCard, Input.mousePosition, eventData.pressEventCamera))
        {
            contactCard.localScale = hoverScale;
        }
        else
        {
            contactCard.localScale = normalScale;
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