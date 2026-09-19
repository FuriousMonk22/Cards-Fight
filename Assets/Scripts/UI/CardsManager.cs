using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class CardsManager : MonoBehaviour
{
    private GameObject cardPrefab;
    private GameObject modifierCardPrefab;
    private int creatureCardCount;
    private int modifierCardCount;
    private const int CardLimit = 3;

    // Creature name -> spawn weight
    private Dictionary<string, int> creatureTypes = new Dictionary<string, int>()
    {
        { "Template", 1 },
        { "Flamingo", 2 }
    };

    private Dictionary<string, int> modifierTypes = new Dictionary<string, int>()
    {
        { "One_Up", 1 }
    };

    private static Dictionary<string, Func<Ability>> modifierConstructors =
        new Dictionary<string, Func<Ability>>()
        {
            { "One_Up", () => new one_up(null) }
        };

    private void Start()
    {
        cardPrefab = Resources.Load<GameObject>("UI/card");
        modifierCardPrefab = Resources.Load<GameObject>("UI/Modifier_Card");
    }

    private void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            CreateCard();
            CreateModifierCard();
        }
    }

    private string GetRandomCreature()
    {
        int totalWeight = 0;

        foreach (KeyValuePair<string, int> creature in creatureTypes)
        {
            totalWeight += creature.Value;
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);

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

    private string GetRandomModifier()
    {
        int totalWeight = 0;

        foreach (KeyValuePair<string, int> modifier in modifierTypes)
            totalWeight += modifier.Value;

        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        foreach (KeyValuePair<string, int> modifier in modifierTypes)
        {
            if (randomValue < modifier.Value)
                return modifier.Key;

            randomValue -= modifier.Value;
        }

        return null;
    }

    private static Ability CreateModifier(string modifierName)
    {
        if (modifierConstructors.TryGetValue(modifierName, out Func<Ability> constructor))
            return constructor();

        return null;
    }

    private void CreateCard()
    {
        if (creatureCardCount >= CardLimit)
            return;

        GameObject cardHolder = new GameObject("CardHolder");
        cardHolder.transform.SetParent(transform, false);

        RectTransform rect = cardHolder.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(126f, 186f);

        CardHolder holder = cardHolder.AddComponent<CardHolder>();
        cardHolder.transform.SetAsFirstSibling();

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

            creatureCardCount++;

            Debug.Log($"Created card: {randomCreature}");
        }
        else
        {
            Debug.LogError("Could not load Resources/UI/card.prefab");
        }
    }

    private void CreateModifierCard()
    {
        if (modifierCardCount >= CardLimit)
            return;

        if (modifierCardPrefab == null)
        {
            Debug.LogError("Could not load Resources/UI/Modifier_Card.prefab");
            return;
        }

        GameObject cardHolder = new GameObject("ModifierCardHolder");
        cardHolder.transform.SetParent(transform, false);

        RectTransform rect = cardHolder.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(126f, 186f);

        CardHolder holder = cardHolder.AddComponent<CardHolder>();
        cardHolder.transform.SetAsLastSibling();
        GameObject card = Instantiate(modifierCardPrefab, cardHolder.transform, false);

        string modifierName = GetRandomModifier();
        Ability modifier = CreateModifier(modifierName);

        if (modifier == null)
        {
            Destroy(cardHolder);
            return;
        }

        modifier.Name = "One Up";
        modifier.Description = "Respawns this creature in the nearest empty tile within 3x3 on death.";

        ModifierCard modifierCard = card.GetComponent<ModifierCard>();
        modifierCard.SetAbility(modifier);

        RectTransform cardRect = card.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.localScale = Vector3.one;
        }

        holder.Initialize(true);
        modifierCardCount++;

        Debug.Log("Created modifier card: One Up");
    }

    public void SubtractCreatureCard()
    {
        creatureCardCount = Mathf.Max(0, creatureCardCount - 1);
    }

    public void SubtractModifierCard()
    {
        modifierCardCount = Mathf.Max(0, modifierCardCount - 1);
    }
}
