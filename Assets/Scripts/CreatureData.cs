using UnityEngine;
using TMPro;

[System.Serializable]
public class CreatureData : MonoBehaviour
{
    private const string CreaturePath = "Creatures/";

    public string Name;
    public Sprite Sprite;
    public int team;

    public bool canSwim;
    public bool canWalk;
    public bool canFly;

    public int Health;
    public int Attack;
    public int Shield;
    public float CriticalChance;
    public float DodgeChance;
    public float CooldownMove;
    public float CooldownAttack;
    public int AttackRange;

    public CreatureArchetype Class;

    private TextMeshPro healthText;

    void Start()
    {
        CreateHealthText();
    }

    private void CreateHealthText()
    {
        GameObject textObject = new GameObject("HealthText");

        textObject.transform.SetParent(transform);
        textObject.transform.localPosition = new Vector3(0f, 0f, 0f);

        healthText = textObject.AddComponent<TextMeshPro>();

        healthText.text = Health.ToString();
        healthText.fontSize = 10;
        healthText.alignment = TextAlignmentOptions.Center;

        healthText.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        healthText.color = Color.red;

        healthText.renderer.sortingOrder = 100;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;

        if (Health < 0)
            Health = 0;

        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = Health.ToString();
        }
    }

    public static CreatureData Load(string creatureName)
    {
        return Resources.Load<GameObject>(CreaturePath + creatureName)
            .GetComponent<CreatureData>();
    }
}

public enum CreatureArchetype
{
    Balanced,
    Attacker,
    Tank,
    Assassin,
    Evader,
    Ranger,
    Speedster,
    Special,
}