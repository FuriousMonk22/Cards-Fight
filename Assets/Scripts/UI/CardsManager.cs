using UnityEngine;
using UnityEngine.InputSystem;

public class CardsManager : MonoBehaviour
{
    private GameObject cardPrefab;
    private int cardCount;
    private int cardLimit = 8;

    private void Start()
    {
        cardPrefab = Resources.Load<GameObject>("UI/card");
        cardPrefab.GetComponent<CreatureCard>().SetCreature("Flamingo");
    }

    private void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            CreateCard();
        }
    }

    private void CreateCard()
    {
        if (cardCount >= cardLimit) return;

        // Create CardHolder object
        GameObject cardHolder = new GameObject("CardHolder");

        // Add it as a child of this object
        cardHolder.transform.SetParent(transform, false);

        // Add RectTransform and set size
        RectTransform rect = cardHolder.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(126f, 186f);

        // Add CardHolder component
        cardHolder.AddComponent<CardHolder>();

        // Add the card prefab as a child
        if (cardPrefab != null)
        {
            GameObject card = Instantiate(cardPrefab, cardHolder.transform, false);

            RectTransform cardRect = card.GetComponent<RectTransform>();

            if (cardRect != null)
            {
                cardRect.anchoredPosition = Vector2.zero;
                cardRect.localScale = Vector3.one;
            }

            cardHolder.GetComponent<CardHolder>().Initialize();
            cardCount++;
        }
        else
        {
            Debug.LogError("Could not load Resources/UI/card.prefab");
        }
    }

    public void subtractCard()
    {
        cardCount--;
    }
}
