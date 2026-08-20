using UnityEngine;

[System.Serializable]
public class CreatureData : MonoBehaviour
{
    private const string CreaturePath = "Creatures/";
    private const string HealthbarPath = "Healthbar";

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

    private int maxHealth;
    private HealthBar healthBar;

    void Start()
    {
        maxHealth = Health;
        AttachHealthbar();
    }

    private void AttachHealthbar()
    {
        GameObject healthbarPrefab = Resources.Load<GameObject>(HealthbarPath);

        if (healthbarPrefab == null)
        {
            Debug.LogError("Could not find healthbar at Resources/Healthbar");
            return;
        }

        // Add the healthbar to the game as a child of the creature.
        GameObject healthbarObject = Instantiate(healthbarPrefab, transform);

        // Position it 10 pixels lower relative to the creature.
        healthbarObject.transform.localPosition = new Vector3(0f, -0.55f, 0f);

        // Get the HealthBar component.
        healthBar = healthbarObject.GetComponent<HealthBar>();

        if (healthBar == null)
        {
            Debug.LogError("Healthbar prefab does not have a HealthBar component.");
            return;
        }

        // Initialize the healthbar.
        healthBar.SetHealth(Health, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        Health = Mathf.Max(Health, 0);

        if (healthBar != null)
        {
            healthBar.SetHealth(Health, maxHealth);
        }
    }

    public static CreatureData Load(string creatureName)
    {
        GameObject prefab =
            Resources.Load<GameObject>(CreaturePath + creatureName);

        if (prefab == null)
        {
            Debug.LogError(
                $"Could not find creature at Resources/{CreaturePath}{creatureName}"
            );

            return null;
        }

        return prefab.GetComponent<CreatureData>();
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
