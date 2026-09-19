using System;
using UnityEngine;
using System.Collections.Generic;

// Class that controls Creatures' behaviour and dynamic data

// todo: create ability class and abilities array in this class

public class Creature : MonoBehaviour
{
    public CreatureData creatureData;

    public Vector3Int cell; // which cell  it's in, updated in CreaturesGrid, will break if not in sync with creaturesGrid.Creatures
    public int team;
    private int healthValue; // current health
    [SerializeField] private int shieldValue;
    private int cooldownRemaining; // cooldown remaining for next action (Ability/Attack/Move)

    private HealthBar healthBar;
    private SpriteRenderer sprite;
    public CreaturesGrid creaturesGrid;
    public TerrainTilemap terrainTilemap;

    public List<Ability> abilities = new List<Ability>();

    // Initialize creature based on CreatureData (Can get from CreatureData.Load(STRING))
    public void Initialize(CreatureData cd)
    {
        creatureData = cd;

        healthValue = creatureData.Health;
        shieldValue = creatureData.Shield;

        cooldownRemaining = creatureData.CooldownAction;
        
        sprite = GetComponent<SpriteRenderer>();
        SetupSprite();
        AttachHealthbar();

        if (creatureData.Name == "Flamingo")
        {
            abilities.Add(new Water_Defense_Buff(this));
        }
    }

    public int getHealth()
    {
        return healthValue;
    }
    
    public void TakeDamage(int damage)
    {
        healthValue = Mathf.Max(healthValue - damage, 0);
        healthBar.SetHealth(healthValue, creatureData.Health);
        
        if (healthValue <= 0)
        {
            foreach (Ability ability in new List<Ability>(abilities))
                ability.OnDeath();

            creaturesGrid.RemoveCreature(cell.x, cell.y);
        }
    }

    public int GetShield()
    {
        return shieldValue;
    }

    public void AddShield(int amount)
    {
        shieldValue += amount;
    }

    public void ResetShield()
    {
        shieldValue = creatureData.Shield;
    }

    // Called every tick by GameManager
    public void Tick()
    {
        DecrementCooldown();
        if(cooldownRemaining > 0) return;
        
        // to do: add Ability (Or in attack/move/spawn based on ability method) call here
        bool attacked = Attack();
        if(!attacked) Move();
    }

    // Move towards nearest enemy and reset cooldown
    public bool Move()
    {   
        Vector3Int target_cell = Pathfinder.GetNextStep(
            cell,
            Pathfinder.GetNearestEnemyInRange(cell, 10, team),
            this);
        
        if(target_cell == cell) return false;
        
        
        if(creaturesGrid.Creatures[target_cell.x, target_cell.y] == null) {
            creaturesGrid.Swap(cell.x, cell.y, target_cell.x, target_cell.y, true);
            ResetCooldown();
            return true;
        }

        return false;
    }

    // Attack nearest enemy in range and reset cooldown
    public bool Attack()
    {
        if(cooldownRemaining > 0)
            return false;

        Vector3Int cell_to_attack = Pathfinder.GetNearestEnemyInRange(cell, creatureData.AttackRange, team);
        //Debug.Log($"Attacking from cell {cell}, nearest cell {cell_to_attack}");
        
        if(cell_to_attack == cell) return false;
        else
        {
            Creature creature_to_attack = creaturesGrid.Creatures[cell_to_attack.x, cell_to_attack.y].GetComponent<Creature>();
            
            if(creature_to_attack == null) return false;

            creature_to_attack.TakeDamage(creatureData.Attack);
            ResetCooldown();
            return true;
        }
    }

    public void DecrementCooldown()
    {
        cooldownRemaining -= 1;
    }

    public void ResetCooldown()
    {
        cooldownRemaining = creatureData.CooldownAction;
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

    public void OnCombatStart()
    {
        foreach (Ability ability in abilities)
        {
            ability.OnStart();
        }
    }
}
