using UnityEngine;

public class Creature : MonoBehaviour
{
    public CreatureData creatureData;

    public int team;
    private int healthValue;

    private HealthBar healthBar;
    private SpriteRenderer sprite;

    // Initialize creature based on CreatureData (Can get from CreatureData.Load(STRING))
    public void Initialize(CreatureData cd)
    {
        creatureData = cd;
        healthValue = creatureData.Health;
        
        sprite = GetComponent<SpriteRenderer>();
        SetupSprite();
        AttachHealthbar();
    }

    public int getHealth()
    {
        return healthValue;
    }
    
    public void TakeDamage(int damage)
    {
        healthValue = Mathf.Max(healthValue - damage, 0);
        healthBar.SetHealth(healthValue, creatureData.Health);
    }

    // Create healthbar object and add offset pos
    private void AttachHealthbar()
    {
        GameObject healthbarPrefab = Resources.Load<GameObject>("Healthbar");
        GameObject healthbarObject = Instantiate(healthbarPrefab, transform);
        healthbarObject.transform.localPosition = new Vector3(0f, -0.55f, 0f);
        healthBar = healthbarObject.GetComponent<HealthBar>();
        healthBar.SetHealth(healthValue, healthValue);
    }

    // adds sprite component (creatureData must be initialized)
    private void SetupSprite()
    {
        sprite = gameObject.AddComponent<SpriteRenderer>();
        sprite.sprite = creatureData.Sprite;
        sprite.sortingOrder = 1;
    }
}
