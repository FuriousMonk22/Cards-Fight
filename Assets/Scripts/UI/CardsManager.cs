using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CardsManager : MonoBehaviour
{
    private GameObject cardPrefab;
    private int cardCount;
    private int cardLimit = 8;

    // Creature name -> spawn weight
    private Dictionary<string, int> creatureTypes = new Dictionary<string, int>()
    {
        { "Template", 1 },
        { "Flamingo", 2 }
    };

    private void Start()
    {
        cardPrefab = Resources.Load<GameObject>("UI/card");
    }

    private void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            CreateCard();
        }
    }

    private string GetRandomCreature()
    {
        int totalWeight = 0;

        foreach (KeyValuePair<string, int> creature in creatureTypes)
        {
            totalWeight += creature.Value;
        }

        int randomValue = Random.Range(0, totalWeight);

        foreach (KeyValuePair<string, int> creature in creatureTypes)
        {
            if (randomValue < creature.Value)
            {
                return creature.Key;
            }

            randomValue -= creature.Value;
        }

        return "Template";
    }

    private void CreateCard()
    {
        if (cardCount >= cardLimit)
            return;

        GameObject cardHolder = new GameObject("CardHolder");
        cardHolder.transform.SetParent(transform, false);

        RectTransform rect = cardHolder.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(126f, 186f);

        CardHolder holder = cardHolder.AddComponent<CardHolder>();

        if (cardPrefab != null)
        {
            GameObject card =
                Instantiate(cardPrefab, cardHolder.transform, false);

            // Random creature for THIS card
            string randomCreature = GetRandomCreature();

            CreatureCard creatureCard = card.GetComponent<CreatureCard>();
            creatureCard.SetCreature(randomCreature);

            RectTransform cardRect = card.GetComponent<RectTransform>();

            if (cardRect != null)
            {
                cardRect.anchoredPosition = Vector2.zero;
                cardRect.localScale = Vector3.one;
            }

            holder.Initialize();

            cardCount++;

            Debug.Log($"Created card: {randomCreature}");
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