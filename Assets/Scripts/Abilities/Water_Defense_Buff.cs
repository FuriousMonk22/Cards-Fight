using UnityEngine;

public class Water_Defense_Buff : Ability
{
    private Creature owner;
    private CreaturesGrid creaturesGrid;
    private TerrainTilemap terrainTilemap;

    private int defenseBuff = 5;

    public Water_Defense_Buff(Creature owner)
    {
        this.owner = owner;
        creaturesGrid = owner.creaturesGrid;
        terrainTilemap = Pathfinder.TerrainTM;
    }

    public override void OnStart()
    {
        Debug.Log($"Water Defense Buff activated by {owner.name}");

        foreach (GameObject go in creaturesGrid.Creatures)
        {
            if (go == null)
                continue;

            Creature creature = go.GetComponent<Creature>();

            if (creature == null)
                continue;

            TerrainTileData tileData =
                terrainTilemap.GetTileData(creature.cell);

            Debug.Log(
                $"{creature.name} | Team: {creature.team} | " +
                $"Swimmable: {tileData.isSwimmable} | Shield: {creature.GetShield()}"
            );

            if (tileData.isSwimmable &&
                creature.team == owner.team)
            {
                creature.AddShield(defenseBuff);

                Debug.Log(
                    $"{creature.name} BUFFED -> Shield: {creature.GetShield()}"
                );
            }
        }
    }

    public override void OnDeath()
    {
    }

    public override void OnMove()
    {
    }

    public override void OnAttack()
    {
    }
}