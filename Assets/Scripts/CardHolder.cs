using UnityEngine;
using UnityEngine.EventSystems;

public class CardHolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform child;
    [SerializeField] private float moveAmount = 100f;

    private Vector2 originalPosition;
    private bool hovering;
    private bool dragging;

    public void Initialize()
    {
        if (child == null)
            child = transform.GetChild(0).GetComponent<RectTransform>();

        originalPosition = child.anchoredPosition;
    }

    private void Update()
    {
        bool shouldMoveUp = hovering || dragging;

        child.anchoredPosition = shouldMoveUp
            ? originalPosition + Vector2.up * moveAmount
            : originalPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void SetDragging(bool value)
    {
        dragging = value;
    }

    public void Consume()
    {
        GameObject.FindGameObjectWithTag("CardsManager").GetComponent<CardsManager>().subtractCard();
        
        Destroy(gameObject);
    }
}
