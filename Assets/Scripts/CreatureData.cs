using UnityEngine;

[System.Serializable]
public class CreatureData : MonoBehaviour
{
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
    public float CooldownMove;
    public float CooldownAttack;
    public int AttackRange;

    public CreatureArchetype Class;
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
