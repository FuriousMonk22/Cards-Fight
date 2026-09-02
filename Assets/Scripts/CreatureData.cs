using UnityEngine;

// Dataclass for creatures, has metadata and stats

[System.Serializable]
public class CreatureData : MonoBehaviour
{
    private const string CreaturePath = "Creatures/";

    public string Name;
    public Sprite Sprite;

    public bool canSwim;
    public bool canWalk;
    public bool canFly;

    public int Health;
    public int Attack;
    public int Shield;
    public float CriticalChance;
    public float DodgeChance;
    public int CooldownAction;
    public int AttackRange;

    public CreatureArchetype Class;

    // Load creaturedata from Resources
    public static CreatureData Load(string creatureName)
    {
        return Resources.Load<CreatureData>(CreaturePath + creatureName);
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